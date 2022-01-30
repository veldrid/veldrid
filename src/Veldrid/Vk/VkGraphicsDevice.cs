using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkGraphicsDevice : GraphicsDevice
    {
        private static readonly FixedUtf8String s_name = "Veldrid-VkGraphicsDevice";
        private static readonly Lazy<bool> s_isSupported = new(CheckIsSupported, isThreadSafe: true);

        private VkInstance _instance;
        private VkPhysicalDevice _physicalDevice;
        private string _deviceName;
        private string _vendorName;
        private GraphicsApiVersion _apiVersion;
        private string _driverName;
        private string _driverInfo;
        private VkDeviceMemoryManager _memoryManager;
        private VkPhysicalDeviceProperties _physicalDeviceProperties;
        private VkPhysicalDeviceFeatures _physicalDeviceFeatures;
        private VkPhysicalDeviceMemoryProperties _physicalDeviceMemProperties;
        private VkDevice _device;
        private uint _graphicsQueueIndex;
        private uint _presentQueueIndex;
        private VkCommandPool _graphicsCommandPool;
        private readonly object _graphicsCommandPoolLock = new();
        private VkQueue _graphicsQueue;
        private readonly object _graphicsQueueLock = new();
        private VkDebugReportCallbackEXT _debugCallbackHandle;
        private PFN_vkDebugReportCallbackEXT? _debugCallbackFunc;
        private bool _debugMarkerEnabled;
        private bool _driverDebug;
        private vkDebugMarkerSetObjectNameEXT_t _setObjectNameDelegate;
        private vkCmdDebugMarkerBeginEXT_t _markerBegin;
        private vkCmdDebugMarkerEndEXT_t _markerEnd;
        private vkCmdDebugMarkerInsertEXT_t _markerInsert;
        private readonly ConcurrentDictionary<VkFormat, VkFilter> _filters = new();
        private readonly BackendInfoVulkan _vulkanInfo;

        private const int SharedCommandPoolCount = 4;
        private Stack<SharedCommandPool> _sharedGraphicsCommandPools = new();
        private VkDescriptorPoolManager _descriptorPoolManager;
        private bool _standardValidationSupported;
        private bool _khronosValidationSupported;
        private bool _standardClipYDirection;
        private vkGetBufferMemoryRequirements2_t _getBufferMemoryRequirements2;
        private vkGetImageMemoryRequirements2_t _getImageMemoryRequirements2;
        private vkGetPhysicalDeviceProperties2_t _getPhysicalDeviceProperties2;
        private vkCreateMetalSurfaceEXT_t _createMetalSurfaceEXT;

        // Staging Resources
        private const uint MinStagingBufferSize = 64;
        private const uint MaxStagingBufferSize = 512;

        private readonly object _stagingResourcesLock = new();
        private readonly List<VkTexture> _availableStagingTextures = new();
        private readonly List<VkBuffer> _availableStagingBuffers = new();

        private readonly Dictionary<VkCommandBuffer, VkTexture> _submittedStagingTextures = new();
        private readonly Dictionary<VkCommandBuffer, VkBuffer> _submittedStagingBuffers = new();
        private readonly Dictionary<VkCommandBuffer, SharedCommandPool> _submittedSharedCommandPools = new();

        public override string DeviceName => _deviceName;

        public override string VendorName => _vendorName;

        public override GraphicsApiVersion ApiVersion => _apiVersion;

        public override GraphicsBackend BackendType => GraphicsBackend.Vulkan;

        public override bool IsUvOriginTopLeft => true;

        public override bool IsDepthRangeZeroToOne => true;

        public override bool IsClipSpaceYInverted => !_standardClipYDirection;

        public override bool IsDriverDebug => _driverDebug;

        public override Swapchain? MainSwapchain => _mainSwapchain;

        public override GraphicsDeviceFeatures Features { get; }

        public override bool GetVulkanInfo(out BackendInfoVulkan info)
        {
            info = _vulkanInfo;
            return true;
        }

        public VkInstance Instance => _instance;
        public VkDevice Device => _device;
        public VkPhysicalDevice PhysicalDevice => _physicalDevice;
        public VkPhysicalDeviceMemoryProperties PhysicalDeviceMemProperties => _physicalDeviceMemProperties;
        public VkQueue GraphicsQueue => _graphicsQueue;
        public uint GraphicsQueueIndex => _graphicsQueueIndex;
        public uint PresentQueueIndex => _presentQueueIndex;
        public bool DebugMarkerEnabled => _debugMarkerEnabled;
        public string DriverName => _driverName;
        public string DriverInfo => _driverInfo;
        public VkDeviceMemoryManager MemoryManager => _memoryManager;
        public VkDescriptorPoolManager DescriptorPoolManager => _descriptorPoolManager;
        public vkCmdDebugMarkerBeginEXT_t MarkerBegin => _markerBegin;
        public vkCmdDebugMarkerEndEXT_t MarkerEnd => _markerEnd;
        public vkCmdDebugMarkerInsertEXT_t MarkerInsert => _markerInsert;
        public vkGetBufferMemoryRequirements2_t GetBufferMemoryRequirements2 => _getBufferMemoryRequirements2;
        public vkGetImageMemoryRequirements2_t GetImageMemoryRequirements2 => _getImageMemoryRequirements2;
        public vkCreateMetalSurfaceEXT_t CreateMetalSurfaceEXT => _createMetalSurfaceEXT;

        private readonly object _submittedFencesLock = new();
        private readonly ConcurrentQueue<Vulkan.VkFence> _availableSubmissionFences = new();
        private readonly List<FenceSubmissionInfo> _submittedFences = new();
        private readonly VkSwapchain? _mainSwapchain;

        private readonly List<FixedUtf8String> _surfaceExtensions = new();

        public VkGraphicsDevice(GraphicsDeviceOptions options, SwapchainDescription? scDesc)
            : this(options, scDesc, new VulkanDeviceOptions())
        {
        }

        public VkGraphicsDevice(GraphicsDeviceOptions options, SwapchainDescription? scDesc, VulkanDeviceOptions vkOptions)
        {
            CreateInstance(options.Debug, vkOptions);
            IsDebug = options.Debug;

            VkSurfaceKHR surface = VkSurfaceKHR.Null;
            if (scDesc != null)
            {
                surface = VkSurfaceUtil.CreateSurface(this, _instance, scDesc.Value.Source);
            }

            CreatePhysicalDevice();
            CreateLogicalDevice(surface, options.PreferStandardClipSpaceYDirection, vkOptions);

            _memoryManager = new VkDeviceMemoryManager(
                _device,
                _physicalDevice,
                _physicalDeviceProperties.limits.bufferImageGranularity,
                1024,
                _getBufferMemoryRequirements2!,
                _getImageMemoryRequirements2!);

            Features = new GraphicsDeviceFeatures(
                computeShader: true,
                geometryShader: _physicalDeviceFeatures.geometryShader,
                tessellationShaders: _physicalDeviceFeatures.tessellationShader,
                multipleViewports: _physicalDeviceFeatures.multiViewport,
                samplerLodBias: true,
                drawBaseVertex: true,
                drawBaseInstance: true,
                drawIndirect: true,
                drawIndirectBaseInstance: _physicalDeviceFeatures.drawIndirectFirstInstance,
                fillModeWireframe: _physicalDeviceFeatures.fillModeNonSolid,
                samplerAnisotropy: _physicalDeviceFeatures.samplerAnisotropy,
                depthClipDisable: _physicalDeviceFeatures.depthClamp,
                texture1D: true,
                independentBlend: _physicalDeviceFeatures.independentBlend,
                structuredBuffer: true,
                subsetTextureView: true,
                commandListDebugMarkers: _debugMarkerEnabled,
                bufferRangeBinding: true,
                shaderFloat64: _physicalDeviceFeatures.shaderFloat64);

            ResourceFactory = new VkResourceFactory(this);

            if (scDesc != null)
            {
                SwapchainDescription desc = scDesc.Value;
                _mainSwapchain = new VkSwapchain(this, desc, surface);
            }

            CreateDescriptorPool();
            CreateGraphicsCommandPool();
            for (int i = 0; i < SharedCommandPoolCount; i++)
            {
                _sharedGraphicsCommandPools.Push(new SharedCommandPool(this, true));
            }

            _vulkanInfo = new BackendInfoVulkan(this);

            PostDeviceCreated();
        }

        public override ResourceFactory ResourceFactory { get; }

        private protected override void SubmitCommandsCore(CommandList cl, Fence? fence)
        {
            SubmitCommandList(cl, 0, null, 0, null, fence);
        }

        private void SubmitCommandList(
            CommandList cl,
            uint waitSemaphoreCount,
            VkSemaphore* waitSemaphoresPtr,
            uint signalSemaphoreCount,
            VkSemaphore* signalSemaphoresPtr,
            Fence? fence)
        {
            VkCommandList vkCL = Util.AssertSubtype<CommandList, VkCommandList>(cl);

            // A fence may complete before Veldrid gets notified of the
            // corresponding VkCommandBuffer completion, so check fences here
            CheckSubmittedFences();

            VkCommandBuffer cb = vkCL.CommandBufferSubmitted();

            SubmitCommandBuffer(
                vkCL, cb, waitSemaphoreCount, waitSemaphoresPtr, signalSemaphoreCount, signalSemaphoresPtr, fence);
        }

        private void SubmitCommandBuffer(
            VkCommandList? vkCL,
            VkCommandBuffer vkCB,
            uint waitSemaphoreCount,
            VkSemaphore* waitSemaphoresPtr,
            uint signalSemaphoreCount,
            VkSemaphore* signalSemaphoresPtr,
            Fence? fence)
        {
            VkSubmitInfo si = VkSubmitInfo.New();
            si.commandBufferCount = 1;
            si.pCommandBuffers = &vkCB;
            VkPipelineStageFlags waitDstStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            si.pWaitDstStageMask = &waitDstStageMask;

            si.pWaitSemaphores = waitSemaphoresPtr;
            si.waitSemaphoreCount = waitSemaphoreCount;
            si.pSignalSemaphores = signalSemaphoresPtr;
            si.signalSemaphoreCount = signalSemaphoreCount;

            Vulkan.VkFence vkFence;
            Vulkan.VkFence submissionFence;
            if (fence != null)
            {
                vkFence = Util.AssertSubtype<Fence, VkFence>(fence).DeviceFence;
                submissionFence = GetFreeSubmissionFence();
            }
            else
            {
                vkFence = GetFreeSubmissionFence();
                submissionFence = vkFence;
            }

            lock (_graphicsQueueLock)
            {
                VkResult result = vkQueueSubmit(_graphicsQueue, 1, ref si, vkFence);
                CheckResult(result);
                if (fence != null)
                {
                    result = vkQueueSubmit(_graphicsQueue, 0, null, submissionFence);
                    CheckResult(result);
                }
            }

            lock (_submittedFencesLock)
            {
                _submittedFences.Add(new FenceSubmissionInfo(submissionFence, vkCL, vkCB));
            }
        }

        private void CheckSubmittedFences()
        {
            lock (_submittedFencesLock)
            {
                for (int i = 0; i < _submittedFences.Count; i++)
                {
                    FenceSubmissionInfo fsi = _submittedFences[i];
                    if (vkGetFenceStatus(_device, fsi.Fence) == VkResult.Success)
                    {
                        CompleteFenceSubmission(fsi);
                        _submittedFences.RemoveAt(i);
                        i -= 1;
                    }
                }
            }
        }

        private void CompleteFenceSubmission(FenceSubmissionInfo fsi)
        {
            Vulkan.VkFence fence = fsi.Fence;
            VkCommandBuffer completedCB = fsi.CommandBuffer;
            fsi.CommandList?.CommandBufferCompleted(completedCB);
            VkResult resetResult = vkResetFences(_device, 1, ref fence);
            CheckResult(resetResult);
            ReturnSubmissionFence(fence);
            lock (_stagingResourcesLock)
            {
                if (_submittedStagingTextures.Remove(completedCB, out VkTexture? stagingTex))
                {
                    _availableStagingTextures.Add(stagingTex);
                }
                if (_submittedStagingBuffers.Remove(completedCB, out VkBuffer? stagingBuffer))
                {
                    if (stagingBuffer.SizeInBytes <= MaxStagingBufferSize)
                    {
                        _availableStagingBuffers.Add(stagingBuffer);
                    }
                    else
                    {
                        stagingBuffer.Dispose();
                    }
                }
                if (_submittedSharedCommandPools.Remove(completedCB, out SharedCommandPool? sharedPool))
                {
                    lock (_graphicsCommandPoolLock)
                    {
                        if (sharedPool.IsCached)
                        {
                            _sharedGraphicsCommandPools.Push(sharedPool);
                        }
                        else
                        {
                            sharedPool.Destroy();
                        }
                    }
                }
            }
        }

        private void ReturnSubmissionFence(Vulkan.VkFence fence)
        {
            _availableSubmissionFences.Enqueue(fence);
        }

        private Vulkan.VkFence GetFreeSubmissionFence()
        {
            if (_availableSubmissionFences.TryDequeue(out Vulkan.VkFence availableFence))
            {
                return availableFence;
            }
            else
            {
                VkFenceCreateInfo fenceCI = VkFenceCreateInfo.New();
                VkResult result = vkCreateFence(_device, ref fenceCI, null, out Vulkan.VkFence newFence);
                CheckResult(result);
                return newFence;
            }
        }

        private protected override void SwapBuffersCore(Swapchain swapchain)
        {
            VkSwapchain vkSC = Util.AssertSubtype<Swapchain, VkSwapchain>(swapchain);
            VkSwapchainKHR deviceSwapchain = vkSC.DeviceSwapchain;
            VkPresentInfoKHR presentInfo = VkPresentInfoKHR.New();
            presentInfo.swapchainCount = 1;
            presentInfo.pSwapchains = &deviceSwapchain;
            uint imageIndex = vkSC.ImageIndex;
            presentInfo.pImageIndices = &imageIndex;

            object presentLock = vkSC.PresentQueueIndex == _graphicsQueueIndex ? _graphicsQueueLock : vkSC;
            lock (presentLock)
            {
                VkResult presentResult = vkQueuePresentKHR(vkSC.PresentQueue, &presentInfo);
                if (presentResult != VkResult.Success &&
                    presentResult != VkResult.SuboptimalKHR &&
                    presentResult != VkResult.ErrorOutOfDateKHR)
                {
                    ThrowResult(presentResult);
                }

                Vulkan.VkFence fence = vkSC.ImageAvailableFence;
                if (vkSC.AcquireNextImage(_device, VkSemaphore.Null, fence))
                {
                    VkResult waitResult = vkWaitForFences(_device, 1, &fence, true, ulong.MaxValue);
                    CheckResult(waitResult);

                    VkResult resetResult = vkResetFences(_device, 1, &fence);
                    CheckResult(resetResult);
                }
            }
        }

        internal void SetResourceName(DeviceResource resource, ReadOnlySpan<char> name)
        {
            if (_debugMarkerEnabled)
            {
                SetResourceNameCore(resource, name);
            }
        }

        private void SetResourceNameCore(DeviceResource resource, ReadOnlySpan<char> name)
        {
            switch (resource)
            {
                case VkBuffer buffer:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.BufferEXT, buffer.DeviceBuffer.Handle, name);
                    break;

                case VkFramebuffer framebuffer:
                    SetDebugMarkerName(
                        VkDebugReportObjectTypeEXT.FramebufferEXT,
                        framebuffer.CurrentFramebuffer.Handle,
                        name);
                    break;

                case VkPipeline pipeline:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.PipelineEXT, pipeline.DevicePipeline.Handle, name);
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.PipelineLayoutEXT, pipeline.PipelineLayout.Handle, name);
                    break;

                case VkResourceLayout resourceLayout:
                    SetDebugMarkerName(
                        VkDebugReportObjectTypeEXT.DescriptorSetLayoutEXT,
                        resourceLayout.DescriptorSetLayout.Handle,
                        name);
                    break;

                case VkResourceSet resourceSet:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.DescriptorSetEXT, resourceSet.DescriptorSet.Handle, name);
                    break;

                case VkSampler sampler:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.SamplerEXT, sampler.DeviceSampler.Handle, name);
                    break;

                case VkShader shader:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.ShaderModuleEXT, shader.ShaderModule.Handle, name);
                    break;

                case VkTexture tex:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.ImageEXT, tex.OptimalDeviceImage.Handle, name);
                    break;

                case VkTextureView texView:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.ImageViewEXT, texView.ImageView.Handle, name);
                    break;

                case VkFence fence:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.FenceEXT, fence.DeviceFence.Handle, name);
                    break;

                case VkSwapchain sc:
                    SetDebugMarkerName(VkDebugReportObjectTypeEXT.SwapchainKHREXT, sc.DeviceSwapchain.Handle, name);
                    break;

                default:
                    break;
            }
        }

        [SkipLocalsInit]
        internal void SetDebugMarkerName(VkDebugReportObjectTypeEXT type, ulong target, ReadOnlySpan<char> name)
        {
            Span<byte> utf8Buffer = stackalloc byte[1024];
            Util.GetNullTerminatedUtf8(name, ref utf8Buffer);
            SetDebugMarkerName(type, target, utf8Buffer);
        }

        internal void SetDebugMarkerName(VkDebugReportObjectTypeEXT type, ulong target, ReadOnlySpan<byte> nameUtf8)
        {
            Debug.Assert(_setObjectNameDelegate != null);

            fixed (byte* utf8Ptr = nameUtf8)
            {
                VkDebugMarkerObjectNameInfoEXT nameInfo = VkDebugMarkerObjectNameInfoEXT.New();
                nameInfo.objectType = type;
                nameInfo.@object = target;
                nameInfo.pObjectName = utf8Ptr;

                VkResult result = _setObjectNameDelegate(_device, &nameInfo);
                CheckResult(result);
            }
        }

        private void CreateInstance(bool debug, VulkanDeviceOptions options)
        {
            HashSet<string> availableInstanceLayers = new(EnumerateInstanceLayers());
            HashSet<string> availableInstanceExtensions = new(GetInstanceExtensions());

            VkInstanceCreateInfo instanceCI = VkInstanceCreateInfo.New();
            VkApplicationInfo applicationInfo = new();
            applicationInfo.apiVersion = new VkVersion(1, 0, 0);
            applicationInfo.applicationVersion = new VkVersion(1, 0, 0);
            applicationInfo.engineVersion = new VkVersion(1, 0, 0);
            applicationInfo.pApplicationName = s_name;
            applicationInfo.pEngineName = s_name;

            instanceCI.pApplicationInfo = &applicationInfo;

            List<IntPtr> instanceExtensions = new();
            List<IntPtr> instanceLayers = new();

            if (availableInstanceExtensions.Contains(CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME))
            {
                _surfaceExtensions.Add(CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME);
            }

            _surfaceExtensions.AddRange(GetSurfaceExtensions(availableInstanceExtensions));

            foreach (FixedUtf8String? ext in _surfaceExtensions)
            {
                instanceExtensions.Add(ext);
            }

            bool hasDeviceProperties2 = availableInstanceExtensions.Contains(CommonStrings.VK_KHR_get_physical_device_properties2);
            if (hasDeviceProperties2)
            {
                instanceExtensions.Add(CommonStrings.VK_KHR_get_physical_device_properties2);
            }

            string[] requestedInstanceExtensions = options.InstanceExtensions ?? Array.Empty<string>();
            List<FixedUtf8String> tempStrings = new();
            try
            {
                foreach (string requiredExt in requestedInstanceExtensions)
                {
                    if (!availableInstanceExtensions.Contains(requiredExt))
                    {
                        throw new VeldridException($"The required instance extension was not available: {requiredExt}");
                    }

                    FixedUtf8String utf8Str = new(requiredExt);
                    instanceExtensions.Add(utf8Str);
                    tempStrings.Add(utf8Str);
                }

                bool debugReportExtensionAvailable = false;
                if (debug)
                {
                    if (availableInstanceExtensions.Contains(CommonStrings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME))
                    {
                        _driverDebug = true;
                        debugReportExtensionAvailable = true;
                        instanceExtensions.Add(CommonStrings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME);
                    }
                    if (availableInstanceLayers.Contains(CommonStrings.StandardValidationLayerName))
                    {
                        _standardValidationSupported = true;
                        instanceLayers.Add(CommonStrings.StandardValidationLayerName);
                    }
                    if (availableInstanceLayers.Contains(CommonStrings.KhronosValidationLayerName))
                    {
                        _khronosValidationSupported = true;
                        instanceLayers.Add(CommonStrings.KhronosValidationLayerName);
                    }
                }

                fixed (IntPtr* ppInstanceExtensions = CollectionsMarshal.AsSpan(instanceExtensions))
                fixed (IntPtr* ppInstanceLayers = CollectionsMarshal.AsSpan(instanceLayers))
                {
                    instanceCI.enabledExtensionCount = (uint)instanceExtensions.Count;
                    instanceCI.ppEnabledExtensionNames = (byte**)ppInstanceExtensions;

                    instanceCI.enabledLayerCount = (uint)instanceLayers.Count;
                    if (instanceLayers.Count > 0)
                    {
                        instanceCI.ppEnabledLayerNames = (byte**)ppInstanceLayers;
                    }

                    VkResult result = vkCreateInstance(ref instanceCI, null, out _instance);
                    CheckResult(result);

                    vkEnumerateInstanceVersion? instanceVersion =
                        GetInstanceProcAddr<vkEnumerateInstanceVersion>("vkEnumerateInstanceVersion");

                    if (instanceVersion != null)
                    {
                        VkVersion version;
                        instanceVersion(&version.value);

                        VkVersion currentVersion = new(instanceCI.pApplicationInfo->apiVersion);
                        if (version.Minor > currentVersion.Minor)
                        {
                            vkDestroyInstance(_instance, null);

                            instanceCI.pApplicationInfo->apiVersion = version;

                            result = vkCreateInstance(ref instanceCI, null, out _instance);
                            CheckResult(result);
                        }
                    }
                }

                if (HasSurfaceExtension(CommonStrings.VK_EXT_METAL_SURFACE_EXTENSION_NAME))
                {
                    _createMetalSurfaceEXT = GetInstanceProcAddr<vkCreateMetalSurfaceEXT_t>("vkCreateMetalSurfaceEXT");
                }

                if (debug && debugReportExtensionAvailable)
                {
                    EnableDebugCallback();
                }

                if (hasDeviceProperties2)
                {
                    _getPhysicalDeviceProperties2 = GetInstanceProcAddr<vkGetPhysicalDeviceProperties2_t>("vkGetPhysicalDeviceProperties2")
                        ?? GetInstanceProcAddr<vkGetPhysicalDeviceProperties2_t>("vkGetPhysicalDeviceProperties2KHR");
                }
            }
            finally
            {
                foreach (FixedUtf8String tempStr in tempStrings)
                {
                    tempStr.Dispose();
                }
            }
        }

        private static IEnumerable<FixedUtf8String> GetSurfaceExtensions(HashSet<string> instanceExtensions)
        {
            if (instanceExtensions.Contains(CommonStrings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME))
            {
                yield return CommonStrings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME;
            }
            if (instanceExtensions.Contains(CommonStrings.VK_KHR_ANDROID_SURFACE_EXTENSION_NAME))
            {
                yield return (CommonStrings.VK_KHR_ANDROID_SURFACE_EXTENSION_NAME);
            }
            if (instanceExtensions.Contains(CommonStrings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME))
            {
                yield return (CommonStrings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
            }
            if (instanceExtensions.Contains(CommonStrings.VK_KHR_WAYLAND_SURFACE_EXTENSION_NAME))
            {
                yield return (CommonStrings.VK_KHR_WAYLAND_SURFACE_EXTENSION_NAME);
            }
            if (instanceExtensions.Contains(CommonStrings.VK_EXT_METAL_SURFACE_EXTENSION_NAME))
            {
                yield return (CommonStrings.VK_EXT_METAL_SURFACE_EXTENSION_NAME);
            }

            // Legacy MoltenVK extensions
            if (instanceExtensions.Contains(CommonStrings.VK_MVK_MACOS_SURFACE_EXTENSION_NAME))
            {
                yield return (CommonStrings.VK_MVK_MACOS_SURFACE_EXTENSION_NAME);
            }
            if (instanceExtensions.Contains(CommonStrings.VK_MVK_IOS_SURFACE_EXTENSION_NAME))
            {
                yield return (CommonStrings.VK_MVK_IOS_SURFACE_EXTENSION_NAME);
            }
        }

        public bool HasSurfaceExtension(FixedUtf8String extension)
        {
            return _surfaceExtensions.Contains(extension);
        }

        public void EnableDebugCallback(VkDebugReportFlagsEXT flags = VkDebugReportFlagsEXT.WarningEXT | VkDebugReportFlagsEXT.ErrorEXT)
        {
            IntPtr createFnPtr;
            using (FixedUtf8String debugExtFnName = "vkCreateDebugReportCallbackEXT")
            {
                createFnPtr = vkGetInstanceProcAddr(_instance, debugExtFnName);
            }
            if (createFnPtr == IntPtr.Zero)
            {
                return;
            }

            _debugCallbackFunc = DebugCallback;
            IntPtr debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);
            VkDebugReportCallbackCreateInfoEXT debugCallbackCI = VkDebugReportCallbackCreateInfoEXT.New();
            debugCallbackCI.flags = flags;
            debugCallbackCI.pfnCallback = debugFunctionPtr;

            vkCreateDebugReportCallbackEXT_d createDelegate = Marshal.GetDelegateForFunctionPointer<vkCreateDebugReportCallbackEXT_d>(createFnPtr);
            VkResult result = createDelegate(_instance, &debugCallbackCI, IntPtr.Zero, out _debugCallbackHandle);
            CheckResult(result);
        }

        private uint DebugCallback(
            uint flags,
            VkDebugReportObjectTypeEXT objectType,
            ulong @object,
            UIntPtr location,
            int messageCode,
            byte* pLayerPrefix,
            byte* pMessage,
            void* pUserData)
        {
            string message = Util.GetString(pMessage);
            VkDebugReportFlagsEXT debugReportFlags = (VkDebugReportFlagsEXT)flags;

#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif

            string fullMessage = $"[{debugReportFlags}] ({objectType}) {message}";

            if (debugReportFlags == VkDebugReportFlagsEXT.ErrorEXT)
            {
                throw new VeldridException("A Vulkan validation error was encountered: " + fullMessage);
            }

            Console.WriteLine(fullMessage);
            return 0;
        }

        private void CreatePhysicalDevice()
        {
            uint deviceCount = 0;
            vkEnumeratePhysicalDevices(_instance, ref deviceCount, null);
            if (deviceCount == 0)
            {
                throw new InvalidOperationException("No physical devices exist.");
            }

            VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[deviceCount];
            vkEnumeratePhysicalDevices(_instance, ref deviceCount, ref physicalDevices[0]);
            // Just use the first one.
            _physicalDevice = physicalDevices[0];

            vkGetPhysicalDeviceProperties(_physicalDevice, out _physicalDeviceProperties);
            fixed (byte* utf8NamePtr = _physicalDeviceProperties.deviceName)
            {
                _deviceName = Util.UTF8.GetString(utf8NamePtr, (int)MaxPhysicalDeviceNameSize).TrimEnd('\0');
            }

            _vendorName = "id:" + _physicalDeviceProperties.vendorID.ToString("x8");
            _apiVersion = GraphicsApiVersion.Unknown;
            _driverInfo = "version:" + _physicalDeviceProperties.driverVersion.ToString("x8");

            vkGetPhysicalDeviceFeatures(_physicalDevice, out _physicalDeviceFeatures);

            vkGetPhysicalDeviceMemoryProperties(_physicalDevice, out _physicalDeviceMemProperties);
        }

        public VkExtensionProperties[] GetDeviceExtensionProperties()
        {
            uint propertyCount = 0;
            VkResult result = vkEnumerateDeviceExtensionProperties(_physicalDevice, (byte*)null, &propertyCount, null);
            CheckResult(result);
            VkExtensionProperties[] props = new VkExtensionProperties[(int)propertyCount];
            fixed (VkExtensionProperties* properties = props)
            {
                result = vkEnumerateDeviceExtensionProperties(_physicalDevice, (byte*)null, &propertyCount, properties);
                CheckResult(result);
            }
            return props;
        }

        private void CreateLogicalDevice(VkSurfaceKHR surface, bool preferStandardClipY, VulkanDeviceOptions options)
        {
            GetQueueFamilyIndices(surface);

            HashSet<uint> familyIndices = new() { _graphicsQueueIndex, _presentQueueIndex };
            VkDeviceQueueCreateInfo* queueCreateInfos = stackalloc VkDeviceQueueCreateInfo[familyIndices.Count];
            uint queueCreateInfosCount = (uint)familyIndices.Count;

            int i = 0;
            foreach (uint index in familyIndices)
            {
                VkDeviceQueueCreateInfo queueCreateInfo = VkDeviceQueueCreateInfo.New();
                queueCreateInfo.queueFamilyIndex = _graphicsQueueIndex;
                queueCreateInfo.queueCount = 1;
                float priority = 1f;
                queueCreateInfo.pQueuePriorities = &priority;
                queueCreateInfos[i] = queueCreateInfo;
                i += 1;
            }

            VkPhysicalDeviceFeatures deviceFeatures = _physicalDeviceFeatures;

            VkExtensionProperties[] props = GetDeviceExtensionProperties();

            HashSet<string> requiredDeviceExtensions = new(options.DeviceExtensions ?? Array.Empty<string>());

            bool hasMemReqs2 = false;
            bool hasDedicatedAllocation = false;
            bool hasDriverProperties = false;
            IntPtr[] activeExtensions = new IntPtr[props.Length];
            uint activeExtensionCount = 0;

            fixed (VkExtensionProperties* properties = props)
            {
                for (int property = 0; property < props.Length; property++)
                {
                    string extensionName = Util.GetString(properties[property].extensionName);
                    if (extensionName == "VK_EXT_debug_marker")
                    {
                        requiredDeviceExtensions.Remove(extensionName);
                        _debugMarkerEnabled = true;
                    }
                    else if (extensionName == "VK_EXT_debug_utils")
                    {
                        // TODO: debug_utils are obsolete on AMD, modern replacement required

                        requiredDeviceExtensions.Remove(extensionName);
                        _debugMarkerEnabled = true;
                    }
                    else if (extensionName == "VK_KHR_swapchain")
                    {
                        requiredDeviceExtensions.Remove(extensionName);
                    }
                    else if (preferStandardClipY && extensionName == "VK_KHR_maintenance1")
                    {
                        requiredDeviceExtensions.Remove(extensionName);
                        _standardClipYDirection = true;
                    }
                    else if (extensionName == "VK_KHR_get_memory_requirements2")
                    {
                        requiredDeviceExtensions.Remove(extensionName);
                        hasMemReqs2 = true;
                    }
                    else if (extensionName == "VK_KHR_dedicated_allocation")
                    {
                        requiredDeviceExtensions.Remove(extensionName);
                        hasDedicatedAllocation = true;
                    }
                    else if (extensionName == "VK_KHR_driver_properties")
                    {
                        requiredDeviceExtensions.Remove(extensionName);
                        hasDriverProperties = true;
                    }
                    else if (requiredDeviceExtensions.Remove(extensionName))
                    {
                    }
                    else
                    {
                        continue;
                    }
                    activeExtensions[activeExtensionCount++] = (IntPtr)properties[property].extensionName;
                }
            }

            if (requiredDeviceExtensions.Count != 0)
            {
                string missingList = string.Join(", ", requiredDeviceExtensions);
                throw new VeldridException(
                    $"The following Vulkan device extensions were not available: {missingList}");
            }

            VkDeviceCreateInfo deviceCreateInfo = VkDeviceCreateInfo.New();
            deviceCreateInfo.queueCreateInfoCount = queueCreateInfosCount;
            deviceCreateInfo.pQueueCreateInfos = queueCreateInfos;

            deviceCreateInfo.pEnabledFeatures = &deviceFeatures;

            StackList<IntPtr> layerNames = new();
            if (_standardValidationSupported)
            {
                layerNames.Add(CommonStrings.StandardValidationLayerName);
            }
            if (_khronosValidationSupported)
            {
                layerNames.Add(CommonStrings.KhronosValidationLayerName);
            }
            deviceCreateInfo.enabledLayerCount = layerNames.Count;
            deviceCreateInfo.ppEnabledLayerNames = (byte**)layerNames.Data;

            fixed (IntPtr* activeExtensionsPtr = activeExtensions)
            {
                deviceCreateInfo.enabledExtensionCount = activeExtensionCount;
                deviceCreateInfo.ppEnabledExtensionNames = (byte**)activeExtensionsPtr;

                VkResult result = vkCreateDevice(_physicalDevice, ref deviceCreateInfo, null, out _device);
                CheckResult(result);
            }

            vkGetDeviceQueue(_device, _graphicsQueueIndex, 0, out _graphicsQueue);

            if (_debugMarkerEnabled)
            {
                _setObjectNameDelegate = Marshal.GetDelegateForFunctionPointer<vkDebugMarkerSetObjectNameEXT_t>(
                    GetInstanceProcAddr("vkDebugMarkerSetObjectNameEXT"));
                _markerBegin = Marshal.GetDelegateForFunctionPointer<vkCmdDebugMarkerBeginEXT_t>(
                    GetInstanceProcAddr("vkCmdDebugMarkerBeginEXT"));
                _markerEnd = Marshal.GetDelegateForFunctionPointer<vkCmdDebugMarkerEndEXT_t>(
                    GetInstanceProcAddr("vkCmdDebugMarkerEndEXT"));
                _markerInsert = Marshal.GetDelegateForFunctionPointer<vkCmdDebugMarkerInsertEXT_t>(
                    GetInstanceProcAddr("vkCmdDebugMarkerInsertEXT"));
            }
            if (hasDedicatedAllocation && hasMemReqs2)
            {
                _getBufferMemoryRequirements2 = GetDeviceProcAddr<vkGetBufferMemoryRequirements2_t>("vkGetBufferMemoryRequirements2")
                    ?? GetDeviceProcAddr<vkGetBufferMemoryRequirements2_t>("vkGetBufferMemoryRequirements2KHR");
                _getImageMemoryRequirements2 = GetDeviceProcAddr<vkGetImageMemoryRequirements2_t>("vkGetImageMemoryRequirements2")
                    ?? GetDeviceProcAddr<vkGetImageMemoryRequirements2_t>("vkGetImageMemoryRequirements2KHR");
            }
            if (_getPhysicalDeviceProperties2 != null && hasDriverProperties)
            {
                VkPhysicalDeviceProperties2KHR deviceProps = VkPhysicalDeviceProperties2KHR.New();
                VkPhysicalDeviceDriverProperties driverProps = VkPhysicalDeviceDriverProperties.New();

                deviceProps.pNext = &driverProps;
                _getPhysicalDeviceProperties2(_physicalDevice, &deviceProps);

                string driverName = Util.UTF8.GetString(
                    driverProps.driverName, VkPhysicalDeviceDriverProperties.DriverNameLength).TrimEnd('\0');

                string driverInfo = Util.UTF8.GetString(
                    driverProps.driverInfo, VkPhysicalDeviceDriverProperties.DriverInfoLength).TrimEnd('\0');

                VkConformanceVersion conforming = driverProps.conformanceVersion;
                _apiVersion = new GraphicsApiVersion(conforming.major, conforming.minor, conforming.subminor, conforming.patch);
                _driverName = driverName;
                _driverInfo = driverInfo;
            }
        }

        private IntPtr GetInstanceProcAddr(string name)
        {
            Span<byte> byteBuffer = stackalloc byte[1024];

            Util.GetNullTerminatedUtf8(name, ref byteBuffer);
            fixed (byte* utf8Ptr = byteBuffer)
            {
                return vkGetInstanceProcAddr(_instance, utf8Ptr);
            }
        }

        private T GetInstanceProcAddr<T>(string name)
        {
            IntPtr funcPtr = GetInstanceProcAddr(name);
            if (funcPtr != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
            }
            throw new EntryPointNotFoundException(name);
        }

        [SkipLocalsInit]
        private IntPtr GetDeviceProcAddr(ReadOnlySpan<char> name)
        {
            Span<byte> byteBuffer = stackalloc byte[1024];

            Util.GetNullTerminatedUtf8(name, ref byteBuffer);
            fixed (byte* utf8Ptr = byteBuffer)
            {
                return vkGetDeviceProcAddr(_device, utf8Ptr);
            }
        }

        private T GetDeviceProcAddr<T>(string name)
        {
            IntPtr funcPtr = GetDeviceProcAddr(name);
            if (funcPtr != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
            }
            throw new EntryPointNotFoundException(name);
        }

        private void GetQueueFamilyIndices(VkSurfaceKHR surface)
        {
            uint queueFamilyCount = 0;
            vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, null);
            VkQueueFamilyProperties[] qfp = new VkQueueFamilyProperties[queueFamilyCount];
            vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, out qfp[0]);

            bool foundGraphics = false;
            bool foundPresent = surface == VkSurfaceKHR.Null;

            for (uint i = 0; i < qfp.Length; i++)
            {
                if ((qfp[i].queueFlags & VkQueueFlags.Graphics) != 0)
                {
                    _graphicsQueueIndex = i;
                    foundGraphics = true;
                }

                if (!foundPresent)
                {
                    vkGetPhysicalDeviceSurfaceSupportKHR(_physicalDevice, i, surface, out VkBool32 presentSupported);
                    if (presentSupported)
                    {
                        _presentQueueIndex = i;
                        foundPresent = true;
                    }
                }

                if (foundGraphics && foundPresent)
                {
                    return;
                }
            }
        }

        private void CreateDescriptorPool()
        {
            _descriptorPoolManager = new VkDescriptorPoolManager(this);
        }

        private void CreateGraphicsCommandPool()
        {
            VkCommandPoolCreateInfo commandPoolCI = VkCommandPoolCreateInfo.New();
            commandPoolCI.flags = VkCommandPoolCreateFlags.ResetCommandBuffer;
            commandPoolCI.queueFamilyIndex = _graphicsQueueIndex;
            VkResult result = vkCreateCommandPool(_device, ref commandPoolCI, null, out _graphicsCommandPool);
            CheckResult(result);
        }

        private protected override MappedResource MapCore(
            MappableResource resource, uint offsetInBytes, uint sizeInBytes, MapMode mode, uint subresource)
        {
            VkMemoryBlock memoryBlock;
            IntPtr mappedPtr = IntPtr.Zero;
            uint rowPitch = 0;
            uint depthPitch = 0;

            if (resource is VkBuffer buffer)
            {
                memoryBlock = buffer.Memory;
                sizeInBytes = buffer.SizeInBytes;
            }
            else
            {
                VkTexture texture = Util.AssertSubtype<MappableResource, VkTexture>(resource);
                Util.GetMipLevelAndArrayLayer(texture, subresource, out uint mipLevel, out uint arrayLayer);
                VkSubresourceLayout layout = texture.GetSubresourceLayout(mipLevel, arrayLayer);
                memoryBlock = texture.Memory;
                offsetInBytes += (uint)layout.offset;
                rowPitch = (uint)layout.rowPitch;
                depthPitch = (uint)layout.depthPitch;
            }

            if (memoryBlock.DeviceMemory.Handle != 0)
            {
                if (memoryBlock.IsPersistentMapped)
                {
                    mappedPtr = (IntPtr)memoryBlock.BlockMappedPointer;
                }
                else
                {
                    void* ret;
                    VkResult result = vkMapMemory(
                        _device, memoryBlock.DeviceMemory, memoryBlock.Offset, memoryBlock.Size, 0, &ret);
                    CheckResult(result);

                    mappedPtr = (IntPtr)ret;
                }
            }

            byte* dataPtr = (byte*)mappedPtr.ToPointer() + offsetInBytes;
            return new MappedResource(
                resource,
                mode,
                (IntPtr)dataPtr,
                offsetInBytes,
                sizeInBytes,
                subresource,
                rowPitch,
                depthPitch);
        }

        private protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            VkMemoryBlock memoryBlock;
            if (resource is VkBuffer buffer)
            {
                memoryBlock = buffer.Memory;
            }
            else
            {
                VkTexture tex = Util.AssertSubtype<MappableResource, VkTexture>(resource);
                memoryBlock = tex.Memory;
            }

            if (memoryBlock.DeviceMemory.Handle != 0 && !memoryBlock.IsPersistentMapped)
            {
                vkUnmapMemory(_device, memoryBlock.DeviceMemory);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Debug.Assert(_submittedFences.Count == 0);
            foreach (Vulkan.VkFence fence in _availableSubmissionFences)
            {
                vkDestroyFence(_device, fence, null);
            }

            _mainSwapchain?.Dispose();
            if (_debugCallbackFunc != null)
            {
                _debugCallbackFunc = null;
                FixedUtf8String debugExtFnName = "vkDestroyDebugReportCallbackEXT";
                IntPtr destroyFuncPtr = vkGetInstanceProcAddr(_instance, debugExtFnName);
                vkDestroyDebugReportCallbackEXT_d destroyDel
                    = Marshal.GetDelegateForFunctionPointer<vkDestroyDebugReportCallbackEXT_d>(destroyFuncPtr);
                destroyDel(_instance, _debugCallbackHandle, null);
            }

            _descriptorPoolManager.DestroyAll();
            vkDestroyCommandPool(_device, _graphicsCommandPool, null);

            Debug.Assert(_submittedStagingTextures.Count == 0);
            foreach (VkTexture tex in _availableStagingTextures)
            {
                tex.Dispose();
            }

            Debug.Assert(_submittedStagingBuffers.Count == 0);
            foreach (VkBuffer buffer in _availableStagingBuffers)
            {
                buffer.Dispose();
            }

            lock (_graphicsCommandPoolLock)
            {
                while (_sharedGraphicsCommandPools.Count > 0)
                {
                    SharedCommandPool sharedPool = _sharedGraphicsCommandPools.Pop();
                    sharedPool.Destroy();
                }
            }

            _memoryManager.Dispose();

            VkResult result = vkDeviceWaitIdle(_device);
            CheckResult(result);
            vkDestroyDevice(_device, null);
            vkDestroyInstance(_instance, null);
        }

        private protected override void WaitForIdleCore()
        {
            lock (_graphicsQueueLock)
            {
                vkQueueWaitIdle(_graphicsQueue);
            }

            CheckSubmittedFences();
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            VkImageUsageFlags usageFlags = VkImageUsageFlags.Sampled;
            usageFlags |= depthFormat ? VkImageUsageFlags.DepthStencilAttachment : VkImageUsageFlags.ColorAttachment;

            vkGetPhysicalDeviceImageFormatProperties(
                _physicalDevice,
                VkFormats.VdToVkPixelFormat(format),
                VkImageType.Image2D,
                VkImageTiling.Optimal,
                usageFlags,
                VkImageCreateFlags.None,
                out VkImageFormatProperties formatProperties);

            VkSampleCountFlags vkSampleCounts = formatProperties.sampleCounts;
            if ((vkSampleCounts & VkSampleCountFlags.Count32) == VkSampleCountFlags.Count32)
            {
                return TextureSampleCount.Count32;
            }
            else if ((vkSampleCounts & VkSampleCountFlags.Count16) == VkSampleCountFlags.Count16)
            {
                return TextureSampleCount.Count16;
            }
            else if ((vkSampleCounts & VkSampleCountFlags.Count8) == VkSampleCountFlags.Count8)
            {
                return TextureSampleCount.Count8;
            }
            else if ((vkSampleCounts & VkSampleCountFlags.Count4) == VkSampleCountFlags.Count4)
            {
                return TextureSampleCount.Count4;
            }
            else if ((vkSampleCounts & VkSampleCountFlags.Count2) == VkSampleCountFlags.Count2)
            {
                return TextureSampleCount.Count2;
            }

            return TextureSampleCount.Count1;
        }

        private protected override bool GetPixelFormatSupportCore(
            PixelFormat format,
            TextureType type,
            TextureUsage usage,
            out PixelFormatProperties properties)
        {
            VkFormat vkFormat = VkFormats.VdToVkPixelFormat(format, (usage & TextureUsage.DepthStencil) != 0);
            VkImageType vkType = VkFormats.VdToVkTextureType(type);
            VkImageTiling tiling = usage == TextureUsage.Staging ? VkImageTiling.Linear : VkImageTiling.Optimal;
            VkImageUsageFlags vkUsage = VkFormats.VdToVkTextureUsage(usage);

            VkResult result = vkGetPhysicalDeviceImageFormatProperties(
                _physicalDevice,
                vkFormat,
                vkType,
                tiling,
                vkUsage,
                VkImageCreateFlags.None,
                out VkImageFormatProperties vkProps);

            if (result == VkResult.ErrorFormatNotSupported)
            {
                properties = default;
                return false;
            }
            CheckResult(result);

            properties = new PixelFormatProperties(
               vkProps.maxExtent.width,
               vkProps.maxExtent.height,
               vkProps.maxExtent.depth,
               vkProps.maxMipLevels,
               vkProps.maxArrayLayers,
               (uint)vkProps.sampleCounts);
            return true;
        }

        internal VkFilter GetFormatFilter(VkFormat format)
        {
            if (!_filters.TryGetValue(format, out VkFilter filter))
            {
                vkGetPhysicalDeviceFormatProperties(_physicalDevice, format, out VkFormatProperties vkFormatProps);
                filter = (vkFormatProps.optimalTilingFeatures & VkFormatFeatureFlags.SampledImageFilterLinear) != 0
                    ? VkFilter.Linear
                    : VkFilter.Nearest;
                _filters.TryAdd(format, filter);
            }

            return filter;
        }

        private protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(buffer);
            VkBuffer? copySrcVkBuffer = null;
            IntPtr mappedPtr;
            byte* destPtr;
            bool isPersistentMapped = vkBuffer.Memory.IsPersistentMapped;
            if (isPersistentMapped)
            {
                mappedPtr = (IntPtr)vkBuffer.Memory.BlockMappedPointer;
                destPtr = (byte*)mappedPtr + bufferOffsetInBytes;
            }
            else
            {
                copySrcVkBuffer = GetFreeStagingBuffer(sizeInBytes);
                mappedPtr = (IntPtr)copySrcVkBuffer.Memory.BlockMappedPointer;
                destPtr = (byte*)mappedPtr;
            }

            Unsafe.CopyBlock(destPtr, source.ToPointer(), sizeInBytes);

            if (copySrcVkBuffer != null)
            {
                SharedCommandPool pool = GetFreeCommandPool();
                VkCommandBuffer cb = pool.BeginNewCommandBuffer();

                VkBufferCopy copyRegion = new()
                {
                    dstOffset = bufferOffsetInBytes,
                    size = sizeInBytes
                };
                vkCmdCopyBuffer(cb, copySrcVkBuffer.DeviceBuffer, vkBuffer.DeviceBuffer, 1, ref copyRegion);

                pool.EndAndSubmit(cb);
                lock (_stagingResourcesLock)
                {
                    _submittedStagingBuffers.Add(cb, copySrcVkBuffer);
                }
            }
        }

        private SharedCommandPool GetFreeCommandPool()
        {
            SharedCommandPool? sharedPool = null;
            lock (_graphicsCommandPoolLock)
            {
                if (_sharedGraphicsCommandPools.Count > 0)
                {
                    sharedPool = _sharedGraphicsCommandPools.Pop();
                }
            }

            if (sharedPool == null)
            {
                sharedPool = new SharedCommandPool(this, false);
            }

            return sharedPool;
        }

        private IntPtr MapBuffer(VkBuffer buffer, uint numBytes)
        {
            if (buffer.Memory.IsPersistentMapped)
            {
                return (IntPtr)buffer.Memory.BlockMappedPointer;
            }
            else
            {
                void* mappedPtr;
                VkResult result = vkMapMemory(Device, buffer.Memory.DeviceMemory, buffer.Memory.Offset, numBytes, 0, &mappedPtr);
                CheckResult(result);
                return (IntPtr)mappedPtr;
            }
        }

        private void UnmapBuffer(VkBuffer buffer)
        {
            if (!buffer.Memory.IsPersistentMapped)
            {
                vkUnmapMemory(Device, buffer.Memory.DeviceMemory);
            }
        }

        private protected override void UpdateTextureCore(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
            bool isStaging = (vkTex.Usage & TextureUsage.Staging) != 0;
            if (isStaging)
            {
                VkSubresourceLayout layout = vkTex.GetSubresourceLayout(mipLevel, arrayLayer);
                byte* imageBasePtr = (byte*)vkTex.Memory.BlockMappedPointer + layout.offset;

                uint srcRowPitch = FormatHelpers.GetRowPitch(width, texture.Format);
                uint srcDepthPitch = FormatHelpers.GetDepthPitch(srcRowPitch, height, texture.Format);
                Util.CopyTextureRegion(
                    source.ToPointer(),
                    0, 0, 0,
                    srcRowPitch, srcDepthPitch,
                    imageBasePtr,
                    x, y, z,
                    (uint)layout.rowPitch, (uint)layout.depthPitch,
                    width, height, depth,
                    texture.Format);
            }
            else
            {
                VkTexture stagingTex = GetFreeStagingTexture(width, height, depth, texture.Format);
                UpdateTexture(stagingTex, source, sizeInBytes, 0, 0, 0, width, height, depth, 0, 0);
                SharedCommandPool pool = GetFreeCommandPool();
                VkCommandBuffer cb = pool.BeginNewCommandBuffer();
                VkCommandList.CopyTextureCore_VkCommandBuffer(
                    cb,
                    stagingTex, 0, 0, 0, 0, 0,
                    texture, x, y, z, mipLevel, arrayLayer,
                    width, height, depth, 1);
                lock (_stagingResourcesLock)
                {
                    _submittedStagingTextures.Add(cb, stagingTex);
                }
                pool.EndAndSubmit(cb);
            }
        }

        private VkTexture GetFreeStagingTexture(uint width, uint height, uint depth, PixelFormat format)
        {
            uint totalSize = FormatHelpers.GetRegionSize(width, height, depth, format);
            lock (_stagingResourcesLock)
            {
                for (int i = 0; i < _availableStagingTextures.Count; i++)
                {
                    VkTexture tex = _availableStagingTextures[i];
                    if (tex.Memory.Size >= totalSize)
                    {
                        _availableStagingTextures.RemoveAt(i);
                        tex.SetStagingDimensions(width, height, depth, format);
                        return tex;
                    }
                }
            }

            uint texWidth = Math.Max(256, width);
            uint texHeight = Math.Max(256, height);
            VkTexture newTex = (VkTexture)ResourceFactory.CreateTexture(TextureDescription.Texture3D(
                texWidth, texHeight, depth, 1, format, TextureUsage.Staging));
            newTex.SetStagingDimensions(width, height, depth, format);

            return newTex;
        }

        private VkBuffer GetFreeStagingBuffer(uint size)
        {
            lock (_stagingResourcesLock)
            {
                for (int i = 0; i < _availableStagingBuffers.Count; i++)
                {
                    VkBuffer buffer = _availableStagingBuffers[i];
                    if (buffer.SizeInBytes >= size)
                    {
                        _availableStagingBuffers.RemoveAt(i);
                        return buffer;
                    }
                }
            }

            uint newBufferSize = Math.Max(MinStagingBufferSize, size);
            VkBuffer newBuffer = (VkBuffer)ResourceFactory.CreateBuffer(
                new BufferDescription(newBufferSize, BufferUsage.StagingWrite));
            return newBuffer;
        }

        public override void ResetFence(Fence fence)
        {
            Vulkan.VkFence vkFence = Util.AssertSubtype<Fence, VkFence>(fence).DeviceFence;
            vkResetFences(_device, 1, ref vkFence);
        }

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout)
        {
            Vulkan.VkFence vkFence = Util.AssertSubtype<Fence, VkFence>(fence).DeviceFence;
            VkResult result = vkWaitForFences(_device, 1, ref vkFence, true, nanosecondTimeout);
            return result == VkResult.Success;
        }

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout)
        {
            int fenceCount = fences.Length;
            Vulkan.VkFence* fencesPtr = stackalloc Vulkan.VkFence[fenceCount];
            for (int i = 0; i < fenceCount; i++)
            {
                fencesPtr[i] = Util.AssertSubtype<Fence, VkFence>(fences[i]).DeviceFence;
            }

            VkResult result = vkWaitForFences(_device, (uint)fenceCount, fencesPtr, waitAll, nanosecondTimeout);
            return result == VkResult.Success;
        }

        internal static bool IsSupported()
        {
            return s_isSupported.Value;
        }

        private static bool CheckIsSupported()
        {
            if (!IsVulkanLoaded())
            {
                return false;
            }

            VkInstanceCreateInfo instanceCI = VkInstanceCreateInfo.New();
            VkApplicationInfo applicationInfo = new();
            applicationInfo.apiVersion = new VkVersion(1, 0, 0);
            applicationInfo.applicationVersion = new VkVersion(1, 0, 0);
            applicationInfo.engineVersion = new VkVersion(1, 0, 0);
            applicationInfo.pApplicationName = s_name;
            applicationInfo.pEngineName = s_name;

            instanceCI.pApplicationInfo = &applicationInfo;

            VkResult result = vkCreateInstance(ref instanceCI, null, out VkInstance testInstance);
            if (result != VkResult.Success)
            {
                return false;
            }

            uint physicalDeviceCount = 0;
            result = vkEnumeratePhysicalDevices(testInstance, ref physicalDeviceCount, null);
            if (result != VkResult.Success || physicalDeviceCount == 0)
            {
                vkDestroyInstance(testInstance, null);
                return false;
            }

            vkDestroyInstance(testInstance, null);

            HashSet<string> instanceExtensions = new(GetInstanceExtensions());
            if (!instanceExtensions.Contains(CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME))
            {
                return false;
            }

            foreach (FixedUtf8String surfaceExtension in GetSurfaceExtensions(instanceExtensions))
            {
                if (instanceExtensions.Contains(surfaceExtension))
                {
                    return true;
                }
            }

            return false;
        }

        internal void ClearColorTexture(VkTexture texture, VkClearColorValue color)
        {
            uint effectiveLayers = texture.ArrayLayers;
            if ((texture.Usage & TextureUsage.Cubemap) != 0)
            {
                effectiveLayers *= 6;
            }
            VkImageSubresourceRange range = new(
                 VkImageAspectFlags.Color,
                 0,
                 texture.MipLevels,
                 0,
                 effectiveLayers);
            SharedCommandPool pool = GetFreeCommandPool();
            VkCommandBuffer cb = pool.BeginNewCommandBuffer();
            texture.TransitionImageLayout(cb, 0, texture.MipLevels, 0, effectiveLayers, VkImageLayout.TransferDstOptimal);
            vkCmdClearColorImage(cb, texture.OptimalDeviceImage, VkImageLayout.TransferDstOptimal, &color, 1, &range);
            VkImageLayout colorLayout = texture.IsSwapchainTexture ? VkImageLayout.PresentSrcKHR : VkImageLayout.ColorAttachmentOptimal;
            texture.TransitionImageLayout(cb, 0, texture.MipLevels, 0, effectiveLayers, colorLayout);
            pool.EndAndSubmit(cb);
        }

        internal void ClearDepthTexture(VkTexture texture, VkClearDepthStencilValue clearValue)
        {
            uint effectiveLayers = texture.ArrayLayers;
            if ((texture.Usage & TextureUsage.Cubemap) != 0)
            {
                effectiveLayers *= 6;
            }
            VkImageAspectFlags aspect = FormatHelpers.IsStencilFormat(texture.Format)
                ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                : VkImageAspectFlags.Depth;
            VkImageSubresourceRange range = new(
                aspect,
                0,
                texture.MipLevels,
                0,
                effectiveLayers);
            SharedCommandPool pool = GetFreeCommandPool();
            VkCommandBuffer cb = pool.BeginNewCommandBuffer();
            texture.TransitionImageLayout(cb, 0, texture.MipLevels, 0, effectiveLayers, VkImageLayout.TransferDstOptimal);
            vkCmdClearDepthStencilImage(
                cb,
                texture.OptimalDeviceImage,
                VkImageLayout.TransferDstOptimal,
                &clearValue,
                1,
                &range);
            texture.TransitionImageLayout(cb, 0, texture.MipLevels, 0, effectiveLayers, VkImageLayout.DepthStencilAttachmentOptimal);
            pool.EndAndSubmit(cb);
        }

        internal override uint GetUniformBufferMinOffsetAlignmentCore()
            => (uint)_physicalDeviceProperties.limits.minUniformBufferOffsetAlignment;

        internal override uint GetStructuredBufferMinOffsetAlignmentCore()
            => (uint)_physicalDeviceProperties.limits.minStorageBufferOffsetAlignment;

        internal void TransitionImageLayout(VkTexture texture, VkImageLayout layout)
        {
            SharedCommandPool pool = GetFreeCommandPool();
            VkCommandBuffer cb = pool.BeginNewCommandBuffer();
            texture.TransitionImageLayout(cb, 0, texture.MipLevels, 0, texture.ArrayLayers, layout);
            pool.EndAndSubmit(cb);
        }

        private class SharedCommandPool
        {
            private readonly VkGraphicsDevice _gd;
            private readonly VkCommandPool _pool;
            private readonly VkCommandBuffer _cb;

            public bool IsCached { get; }

            public SharedCommandPool(VkGraphicsDevice gd, bool isCached)
            {
                _gd = gd;
                IsCached = isCached;

                VkCommandPoolCreateInfo commandPoolCI = VkCommandPoolCreateInfo.New();
                commandPoolCI.flags = VkCommandPoolCreateFlags.Transient | VkCommandPoolCreateFlags.ResetCommandBuffer;
                commandPoolCI.queueFamilyIndex = _gd.GraphicsQueueIndex;
                VkResult result = vkCreateCommandPool(_gd.Device, ref commandPoolCI, null, out _pool);
                CheckResult(result);

                VkCommandBufferAllocateInfo allocateInfo = VkCommandBufferAllocateInfo.New();
                allocateInfo.commandBufferCount = 1;
                allocateInfo.level = VkCommandBufferLevel.Primary;
                allocateInfo.commandPool = _pool;
                result = vkAllocateCommandBuffers(_gd.Device, ref allocateInfo, out _cb);
                CheckResult(result);
            }

            public VkCommandBuffer BeginNewCommandBuffer()
            {
                VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;
                VkResult result = vkBeginCommandBuffer(_cb, ref beginInfo);
                CheckResult(result);

                return _cb;
            }

            public void EndAndSubmit(VkCommandBuffer cb)
            {
                VkResult result = vkEndCommandBuffer(cb);
                CheckResult(result);
                _gd.CheckSubmittedFences();
                _gd.SubmitCommandBuffer(null, cb, 0, null, 0, null, null);
                lock (_gd._stagingResourcesLock)
                {
                    _gd._submittedSharedCommandPools.Add(cb, this);
                }
            }

            internal void Destroy()
            {
                vkDestroyCommandPool(_gd.Device, _pool, null);
            }
        }

        private struct FenceSubmissionInfo
        {
            public Vulkan.VkFence Fence;
            public VkCommandList? CommandList;
            public VkCommandBuffer CommandBuffer;

            public FenceSubmissionInfo(Vulkan.VkFence fence, VkCommandList? commandList, VkCommandBuffer commandBuffer)
            {
                Fence = fence;
                CommandList = commandList;
                CommandBuffer = commandBuffer;
            }
        }
    }

    internal unsafe delegate VkResult vkCreateDebugReportCallbackEXT_d(
        VkInstance instance,
        VkDebugReportCallbackCreateInfoEXT* createInfo,
        IntPtr allocatorPtr,
        out VkDebugReportCallbackEXT ret);

    internal unsafe delegate void vkDestroyDebugReportCallbackEXT_d(
        VkInstance instance,
        VkDebugReportCallbackEXT callback,
        VkAllocationCallbacks* pAllocator);

    internal unsafe delegate VkResult vkEnumerateInstanceVersion(uint* pApiVersion);

    internal unsafe delegate VkResult vkDebugMarkerSetObjectNameEXT_t(VkDevice device, VkDebugMarkerObjectNameInfoEXT* pNameInfo);
    internal unsafe delegate void vkCmdDebugMarkerBeginEXT_t(VkCommandBuffer commandBuffer, VkDebugMarkerMarkerInfoEXT* pMarkerInfo);
    internal unsafe delegate void vkCmdDebugMarkerEndEXT_t(VkCommandBuffer commandBuffer);
    internal unsafe delegate void vkCmdDebugMarkerInsertEXT_t(VkCommandBuffer commandBuffer, VkDebugMarkerMarkerInfoEXT* pMarkerInfo);

    internal unsafe delegate void vkGetBufferMemoryRequirements2_t(VkDevice device, VkBufferMemoryRequirementsInfo2KHR* pInfo, VkMemoryRequirements2KHR* pMemoryRequirements);
    internal unsafe delegate void vkGetImageMemoryRequirements2_t(VkDevice device, VkImageMemoryRequirementsInfo2KHR* pInfo, VkMemoryRequirements2KHR* pMemoryRequirements);

    internal unsafe delegate void vkGetPhysicalDeviceProperties2_t(VkPhysicalDevice physicalDevice, void* properties);

    // VK_EXT_metal_surface

    internal unsafe delegate VkResult vkCreateMetalSurfaceEXT_t(
        VkInstance instance,
        VkMetalSurfaceCreateInfoEXT* pCreateInfo,
        VkAllocationCallbacks* pAllocator,
        VkSurfaceKHR* pSurface);

    internal unsafe struct VkMetalSurfaceCreateInfoEXT
    {
        public const VkStructureType VK_STRUCTURE_TYPE_METAL_SURFACE_CREATE_INFO_EXT = (VkStructureType)1000217000;

        public VkStructureType sType;
        public void* pNext;
        public uint flags;
        public void* pLayer;
    }

    internal unsafe struct VkPhysicalDeviceDriverProperties
    {
        public const int DriverNameLength = 256;
        public const int DriverInfoLength = 256;
        public const VkStructureType VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_DRIVER_PROPERTIES = (VkStructureType)1000196000;

        public VkStructureType sType;
        public void* pNext;
        public VkDriverId driverID;
        public fixed byte driverName[DriverNameLength];
        public fixed byte driverInfo[DriverInfoLength];
        public VkConformanceVersion conformanceVersion;

        public static VkPhysicalDeviceDriverProperties New()
        {
            return new VkPhysicalDeviceDriverProperties() { sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_DRIVER_PROPERTIES };
        }
    }

    internal enum VkDriverId
    {
    }

    internal struct VkConformanceVersion
    {
        public byte major;
        public byte minor;
        public byte subminor;
        public byte patch;
    }
}
