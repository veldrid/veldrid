using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkGraphicsDevice : GraphicsDevice
    {
        private static readonly FixedUtf8String s_name = "Veldrid-VkGraphicsDevice";

        private VkInstance _instance;
        private VkPhysicalDevice _physicalDevice;
        private VkDeviceMemoryManager _memoryManager;
        private VkSurfaceKHR _surface;
        private VkPhysicalDeviceProperties _physicalDeviceProperties;
        private VkPhysicalDeviceFeatures _physicalDeviceFeatures;
        private VkPhysicalDeviceMemoryProperties _physicalDeviceMemProperties;
        private VkDevice _device;
        private readonly VkSwapchainFramebuffer _scFB;
        private uint _graphicsQueueIndex;
        private uint _presentQueueIndex;
        private VkDescriptorPool _descriptorPool;
        private VkCommandPool _graphicsCommandPool;
        private readonly object _graphicsCommandPoolLock = new object();
        private VkFence _imageAvailableFence;
        private VkQueue _graphicsQueue;
        private readonly object _graphicsQueueLock = new object();
        private VkQueue _presentQueue;
        private VkDebugReportCallbackEXT _debugCallbackHandle;
        private PFN_vkDebugReportCallbackEXT _debugCallbackFunc;
        private readonly List<VkCommandList> _commandListsToDispose = new List<VkCommandList>();
        private readonly ConcurrentQueue<SharedCommandPool> _commandBuffersToFree
            = new ConcurrentQueue<SharedCommandPool>();
        private bool _debugMarkerEnabled;
        private vkDebugMarkerSetObjectNameEXT_d _setObjectNameDelegate;

        private readonly ConcurrentQueue<Vulkan.VkBuffer> _buffersToDestroy = new ConcurrentQueue<Vulkan.VkBuffer>();
        private readonly ConcurrentQueue<VkImage> _imagesToDestroy = new ConcurrentQueue<VkImage>();
        private readonly ConcurrentQueue<VkMemoryBlock> _memoriesToFree = new ConcurrentQueue<VkMemoryBlock>();

        private const int SharedCommandPoolCount = 4;
        private ConcurrentStack<SharedCommandPool> _sharedGraphicsCommandPools = new ConcurrentStack<SharedCommandPool>();

        // Disposal tracking
        private readonly object _deferredDisposalLock = new object();
        private readonly HashSet<VkDeferredDisposal> _deferredDisposals = new HashSet<VkDeferredDisposal>();
        private readonly object _commandListsLock = new object();
        private readonly List<VkCommandList> _submittedCommandLists = new List<VkCommandList>();

        public override GraphicsBackend BackendType => GraphicsBackend.Vulkan;

        public VkDevice Device => _device;
        public VkPhysicalDevice PhysicalDevice => _physicalDevice;
        public VkPhysicalDeviceMemoryProperties PhysicalDeviceMemProperties => _physicalDeviceMemProperties;
        public VkQueue GraphicsQueue => _graphicsQueue;
        public uint GraphicsQueueIndex => _graphicsQueueIndex;
        public VkQueue PresentQueue => _presentQueue;
        public uint PresentQueueIndex => _presentQueueIndex;
        public VkDeviceMemoryManager MemoryManager => _memoryManager;
        public VkDescriptorPool SharedDescriptorPool => _descriptorPool;

        public VkGraphicsDevice(GraphicsDeviceOptions options, VkSurfaceSource surfaceSource, uint width, uint height)
        {
            CreateInstance(options.Debug);
            CreateSurface(surfaceSource);
            CreatePhysicalDevice();
            CreateLogicalDevice();
            _memoryManager = new VkDeviceMemoryManager(_device, _physicalDevice);
            ResourceFactory = new VkResourceFactory(this);
            _scFB = new VkSwapchainFramebuffer(
                this,
                _surface,
                width,
                height,
                options.SyncToVerticalBlank,
                options.SwapchainDepthFormat);
            CreateDescriptorPool();
            CreateGraphicsCommandPool();
            for (int i = 0; i < SharedCommandPoolCount; i++)
            {
                _sharedGraphicsCommandPools.Push(new SharedCommandPool(this, true));
            }
            CreateFences();

            _scFB.AcquireNextImage(_device, VkSemaphore.Null, _imageAvailableFence);
            vkWaitForFences(_device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
            vkResetFences(_device, 1, ref _imageAvailableFence);

            PostDeviceCreated();
        }

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer => _scFB;

        public override bool SyncToVerticalBlank
        {
            get => _scFB.SyncToVerticalBlank;
            set => _scFB.SyncToVerticalBlank = value;
        }

        public override void ExecuteCommands(CommandList cl)
        {
            VkCommandList vkCL = Util.AssertSubtype<CommandList, VkCommandList>(cl);
            lock (_commandListsLock)
            {
                _submittedCommandLists.Add(vkCL);
                foreach (VkDeferredDisposal resource in vkCL.ReferencedResources)
                {
                    resource.ReferenceTracker.Increment();
                }
            }
            VkCommandBuffer vkCB = vkCL.CommandBuffer;
            SubmitCommandBuffer(vkCB);
        }

        private void SubmitCommandBuffer(VkCommandBuffer vkCB)
        {
            VkSubmitInfo si = VkSubmitInfo.New();
            si.commandBufferCount = 1;
            si.pCommandBuffers = &vkCB;
            VkPipelineStageFlags waitDstStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            si.pWaitDstStageMask = &waitDstStageMask;

            lock (_graphicsQueueLock)
            {
                vkQueueSubmit(_graphicsQueue, 1, ref si, VkFence.Null);
            }
        }

        internal void DeferredDisposal(VkDeferredDisposal vdd)
        {
            if (vdd.ReferenceTracker.ReferenceCount == 0)
            {
                vdd.DestroyResources();
            }
            else
            {
                lock (_deferredDisposalLock)
                {
                    _deferredDisposals.Add(vdd);
                }
            }
        }

        public void EnqueueDisposedCommandBuffer(VkCommandList vkCL)
        {
            lock (_commandListsToDispose)
            {
                _commandListsToDispose.Add(vkCL);
            }
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            _scFB.Resize(width, height);
            _scFB.AcquireNextImage(_device, VkSemaphore.Null, _imageAvailableFence);
            vkWaitForFences(_device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
            vkResetFences(_device, 1, ref _imageAvailableFence);
        }

        public override void SwapBuffers()
        {
            WaitForIdle();

            // Then, present the swapchain.
            VkPresentInfoKHR presentInfo = VkPresentInfoKHR.New();

            VkSwapchainKHR swapchain = _scFB.Swapchain;
            presentInfo.swapchainCount = 1;
            presentInfo.pSwapchains = &swapchain;
            uint imageIndex = _scFB.ImageIndex;
            presentInfo.pImageIndices = &imageIndex;

            vkQueuePresentKHR(_presentQueue, ref presentInfo);

            if (_scFB.AcquireNextImage(_device, VkSemaphore.Null, _imageAvailableFence))
            {
                vkWaitForFences(_device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
                vkResetFences(_device, 1, ref _imageAvailableFence);
            }
        }

        public override void SetResourceName(DeviceResource resource, string name)
        {
            if (_debugMarkerEnabled)
            {
                switch (resource)
                {
                    case VkBuffer buffer:
                        SetDebugMarkerName(VkDebugReportObjectTypeEXT.BufferEXT, buffer.DeviceBuffer.Handle, name);
                        break;
                    case VkCommandList commandList:
                        SetDebugMarkerName(
                            VkDebugReportObjectTypeEXT.CommandBufferEXT,
                            (ulong)commandList.CommandBuffer.Handle,
                            string.Format("{0}_CommandBuffer", name));
                        SetDebugMarkerName(
                            VkDebugReportObjectTypeEXT.CommandPoolEXT,
                            commandList.CommandPool.Handle,
                            string.Format("{0}_CommandPool", name));
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
                    default:
                        break;
                }
            }
        }

        private void SetDebugMarkerName(VkDebugReportObjectTypeEXT type, ulong target, string name)
        {
            Debug.Assert(_setObjectNameDelegate != null);

            VkDebugMarkerObjectNameInfoEXT nameInfo = VkDebugMarkerObjectNameInfoEXT.New();
            nameInfo.objectType = type;
            nameInfo.@object = target;

            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* utf8Ptr = stackalloc byte[byteCount];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
                nameInfo.pObjectName = utf8Ptr;
                VkResult result = _setObjectNameDelegate(_device, &nameInfo);
                CheckResult(result);
            }
        }

        private void FlushQueuedDisposables()
        {
            lock (_commandListsToDispose)
            {
                foreach (VkCommandList vkCB in _commandListsToDispose)
                {
                    vkCB.DestroyCommandPool();
                }

                _commandListsToDispose.Clear();
            }

            lock (_graphicsCommandPoolLock)
            {
                while (_commandBuffersToFree.TryDequeue(out SharedCommandPool sharedPool))
                {
                    if (sharedPool.IsCached)
                    {
                        sharedPool.Reset();
                        _sharedGraphicsCommandPools.Push(sharedPool);
                    }
                    else
                    {
                        sharedPool.Destroy();
                    }
                }
            }

            while (_buffersToDestroy.TryDequeue(out Vulkan.VkBuffer buffer))
            {
                vkDestroyBuffer(_device, buffer, null);
            }

            while (_imagesToDestroy.TryDequeue(out VkImage image))
            {
                vkDestroyImage(_device, image, null);
            }

            while (_memoriesToFree.TryDequeue(out VkMemoryBlock memory))
            {
                _memoryManager.Free(memory);
            }
        }

        private void CreateInstance(bool debug)
        {
            HashSet<string> availableInstanceLayers = new HashSet<string>(EnumerateInstanceLayers());
            HashSet<string> availableInstanceExtensions = new HashSet<string>(EnumerateInstanceExtensions());

            VkInstanceCreateInfo instanceCI = VkInstanceCreateInfo.New();
            VkApplicationInfo applicationInfo = new VkApplicationInfo();
            applicationInfo.apiVersion = new VkVersion(1, 0, 0);
            applicationInfo.applicationVersion = new VkVersion(1, 0, 0);
            applicationInfo.engineVersion = new VkVersion(1, 0, 0);
            applicationInfo.pApplicationName = s_name;
            applicationInfo.pEngineName = s_name;

            instanceCI.pApplicationInfo = &applicationInfo;

            StackList<IntPtr, Size64Bytes> instanceExtensions = new StackList<IntPtr, Size64Bytes>();
            StackList<IntPtr, Size64Bytes> instanceLayers = new StackList<IntPtr, Size64Bytes>();

            if (!availableInstanceExtensions.Contains(CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME))
            {
                throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME}");
            }

            instanceExtensions.Add(CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!availableInstanceExtensions.Contains(CommonStrings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME))
                {
                    throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME}");
                }

                instanceExtensions.Add(CommonStrings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (!availableInstanceExtensions.Contains(CommonStrings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME))
                {
                    throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME}");
                }

                instanceExtensions.Add(CommonStrings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
            }
            else
            {
                throw new NotSupportedException("This platform does not support Vulkan.");
            }

            bool debugReportExtensionAvailable = false;
            if (debug)
            {
                if (availableInstanceExtensions.Contains(CommonStrings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME))
                {
                    debugReportExtensionAvailable = true;
                    instanceExtensions.Add(CommonStrings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME);
                }
                if (availableInstanceLayers.Contains(CommonStrings.StandardValidationLayerName))
                {
                    instanceLayers.Add(CommonStrings.StandardValidationLayerName);
                }
            }

            instanceCI.enabledExtensionCount = instanceExtensions.Count;
            instanceCI.ppEnabledExtensionNames = (byte**)instanceExtensions.Data;

            instanceCI.enabledLayerCount = instanceLayers.Count;
            instanceCI.ppEnabledLayerNames = (byte**)instanceLayers.Data;

            VkResult result = vkCreateInstance(ref instanceCI, null, out _instance);
            CheckResult(result);

            if (debug && debugReportExtensionAvailable)
            {
                EnableDebugCallback();
            }
        }

        public void EnableDebugCallback(VkDebugReportFlagsEXT flags = VkDebugReportFlagsEXT.WarningEXT | VkDebugReportFlagsEXT.ErrorEXT)
        {
            Debug.WriteLine("Enabling Vulkan Debug callbacks.");
            _debugCallbackFunc = DebugCallback;
            IntPtr debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);
            VkDebugReportCallbackCreateInfoEXT debugCallbackCI = VkDebugReportCallbackCreateInfoEXT.New();
            debugCallbackCI.flags = flags;
            debugCallbackCI.pfnCallback = debugFunctionPtr;
            IntPtr createFnPtr;
            using (FixedUtf8String debugExtFnName = "vkCreateDebugReportCallbackEXT")
            {
                createFnPtr = vkGetInstanceProcAddr(_instance, debugExtFnName);
            }
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

            Console.WriteLine($"[{debugReportFlags}] ({objectType}) {message}");
            return 0;
        }

        private void CreateSurface(VkSurfaceSource surfaceSource)
        {
            _surface = surfaceSource.CreateSurface(_instance);
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
            string deviceName;
            fixed (byte* utf8NamePtr = _physicalDeviceProperties.deviceName)
            {
                deviceName = Encoding.UTF8.GetString(utf8NamePtr, (int)MaxPhysicalDeviceNameSize);
            }

            vkGetPhysicalDeviceFeatures(_physicalDevice, out _physicalDeviceFeatures);

            vkGetPhysicalDeviceMemoryProperties(_physicalDevice, out _physicalDeviceMemProperties);
        }

        private void CreateLogicalDevice()
        {
            GetQueueFamilyIndices();

            HashSet<uint> familyIndices = new HashSet<uint> { _graphicsQueueIndex, _presentQueueIndex };
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

            VkPhysicalDeviceFeatures deviceFeatures = new VkPhysicalDeviceFeatures();
            deviceFeatures.samplerAnisotropy = true;
            deviceFeatures.fillModeNonSolid = true;
            deviceFeatures.geometryShader = true;
            deviceFeatures.depthClamp = true;
            deviceFeatures.multiViewport = true;

            bool debugMarkerSupported = false;

            uint propertyCount = 0;
            VkResult result = vkEnumerateDeviceExtensionProperties(_physicalDevice, (byte*)null, &propertyCount, null);
            CheckResult(result);
            VkExtensionProperties* properties = stackalloc VkExtensionProperties[(int)propertyCount];
            result = vkEnumerateDeviceExtensionProperties(_physicalDevice, (byte*)null, &propertyCount, properties);
            CheckResult(result);

            for (int property = 0; property < propertyCount; property++)
            {
                if (Util.GetString(properties[property].extensionName) == "VK_EXT_debug_marker")
                {
                    debugMarkerSupported = true;
                    break;
                }
            }

            VkDeviceCreateInfo deviceCreateInfo = VkDeviceCreateInfo.New();
            deviceCreateInfo.queueCreateInfoCount = queueCreateInfosCount;
            deviceCreateInfo.pQueueCreateInfos = queueCreateInfos;

            deviceCreateInfo.pEnabledFeatures = &deviceFeatures;

            StackList<IntPtr> layerNames = new StackList<IntPtr>();
            layerNames.Add(CommonStrings.StandardValidationLayerName);
            deviceCreateInfo.enabledLayerCount = layerNames.Count;
            deviceCreateInfo.ppEnabledLayerNames = (byte**)layerNames.Data;

            StackList<IntPtr> extensionNames = new StackList<IntPtr>();
            extensionNames.Add(CommonStrings.VK_KHR_SWAPCHAIN_EXTENSION_NAME);
            if (debugMarkerSupported)
            {
                extensionNames.Add(CommonStrings.VK_EXT_DEBUG_MARKER_EXTENSION_NAME);
                _debugMarkerEnabled = true;
            }
            deviceCreateInfo.enabledExtensionCount = extensionNames.Count;
            deviceCreateInfo.ppEnabledExtensionNames = (byte**)extensionNames.Data;

            result = vkCreateDevice(_physicalDevice, ref deviceCreateInfo, null, out _device);
            CheckResult(result);

            vkGetDeviceQueue(_device, _graphicsQueueIndex, 0, out _graphicsQueue);
            vkGetDeviceQueue(_device, _presentQueueIndex, 0, out _presentQueue);

            if (debugMarkerSupported)
            {
                IntPtr setObjectNamePtr;
                using (FixedUtf8String debugExtFnName = "vkDebugMarkerSetObjectNameEXT")
                {
                    setObjectNamePtr = vkGetInstanceProcAddr(_instance, debugExtFnName);
                }

                _setObjectNameDelegate = Marshal.GetDelegateForFunctionPointer<vkDebugMarkerSetObjectNameEXT_d>(setObjectNamePtr);
            }
        }

        private void GetQueueFamilyIndices()
        {
            uint queueFamilyCount = 0;
            vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, null);
            VkQueueFamilyProperties[] qfp = new VkQueueFamilyProperties[queueFamilyCount];
            vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, out qfp[0]);

            bool foundGraphics = false;
            bool foundPresent = false;

            for (uint i = 0; i < qfp.Length; i++)
            {
                if ((qfp[i].queueFlags & VkQueueFlags.Graphics) != 0)
                {
                    _graphicsQueueIndex = i;
                    foundGraphics = true;
                }

                vkGetPhysicalDeviceSurfaceSupportKHR(_physicalDevice, i, _surface, out VkBool32 presentSupported);
                if (presentSupported)
                {
                    _presentQueueIndex = i;
                    foundPresent = true;
                }

                if (foundGraphics && foundPresent)
                {
                    break;
                }
            }
        }

        private void CreateDescriptorPool()
        {
            uint poolSizeCount = 5;
            VkDescriptorPoolSize* sizes = stackalloc VkDescriptorPoolSize[(int)poolSizeCount];
            sizes[0].type = VkDescriptorType.UniformBuffer;
            sizes[0].descriptorCount = 5000;
            sizes[1].type = VkDescriptorType.SampledImage;
            sizes[1].descriptorCount = 5000;
            sizes[2].type = VkDescriptorType.Sampler;
            sizes[2].descriptorCount = 5000;
            sizes[3].type = VkDescriptorType.StorageBuffer;
            sizes[3].descriptorCount = 5000;
            sizes[4].type = VkDescriptorType.StorageImage;
            sizes[4].descriptorCount = 5000;


            VkDescriptorPoolCreateInfo descriptorPoolCI = VkDescriptorPoolCreateInfo.New();
            descriptorPoolCI.flags = VkDescriptorPoolCreateFlags.FreeDescriptorSet;
            descriptorPoolCI.maxSets = 5000;
            descriptorPoolCI.pPoolSizes = sizes;
            descriptorPoolCI.poolSizeCount = poolSizeCount;

            VkResult result = vkCreateDescriptorPool(_device, ref descriptorPoolCI, null, out _descriptorPool);
            CheckResult(result);
        }

        private void CreateGraphicsCommandPool()
        {
            VkCommandPoolCreateInfo commandPoolCI = VkCommandPoolCreateInfo.New();
            commandPoolCI.flags = VkCommandPoolCreateFlags.ResetCommandBuffer;
            commandPoolCI.queueFamilyIndex = _graphicsQueueIndex;
            VkResult result = vkCreateCommandPool(_device, ref commandPoolCI, null, out _graphicsCommandPool);
            CheckResult(result);
        }

        private void CreateFences()
        {
            VkFenceCreateInfo fenceCI = VkFenceCreateInfo.New();
            fenceCI.flags = VkFenceCreateFlags.None;
            vkCreateFence(_device, ref fenceCI, null, out _imageAvailableFence);
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            VkMemoryBlock memoryBlock;
            IntPtr mappedPtr;
            uint sizeInBytes;
            uint offset = 0;
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
                memoryBlock = texture.GetMemoryBlock(subresource);
                VkSubresourceLayout layout = texture.GetSubresourceLayout(subresource);
                offset = (uint)layout.offset;
                sizeInBytes = (uint)layout.size;
                rowPitch = (uint)layout.rowPitch;
                depthPitch = (uint)layout.depthPitch;
            }

            if (memoryBlock.IsPersistentMapped)
            {
                mappedPtr = (IntPtr)memoryBlock.BlockMappedPointer;
            }
            else
            {
                mappedPtr = _memoryManager.Map(memoryBlock);
            }

            byte* dataPtr = (byte*)mappedPtr.ToPointer() + offset;
            return new MappedResource(
                resource,
                mode,
                (IntPtr)dataPtr,
                sizeInBytes,
                subresource,
                rowPitch,
                depthPitch);
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            VkMemoryBlock memoryBlock;
            if (resource is VkBuffer buffer)
            {
                memoryBlock = buffer.Memory;
            }
            else
            {
                Debug.Assert(resource is VkTexture);
                memoryBlock = ((VkTexture)resource).GetMemoryBlock(subresource);
            }

            if (!memoryBlock.IsPersistentMapped)
            {
                vkUnmapMemory(_device, memoryBlock.DeviceMemory);
            }
        }

        protected override void PlatformDispose()
        {
            WaitForIdle();

            _scFB.Dispose();
            vkDestroySurfaceKHR(_instance, _surface, null);
            if (_debugCallbackFunc != null)
            {
                _debugCallbackFunc = null;
                FixedUtf8String debugExtFnName = "vkDestroyDebugReportCallbackEXT";
                IntPtr destroyFuncPtr = vkGetInstanceProcAddr(_instance, debugExtFnName);
                vkDestroyDebugReportCallbackEXT_d destroyDel
                    = Marshal.GetDelegateForFunctionPointer<vkDestroyDebugReportCallbackEXT_d>(destroyFuncPtr);
                VkResult debugDestroyResult = destroyDel(_instance, _debugCallbackHandle, null);
                CheckResult(debugDestroyResult);
            }

            vkDestroyDescriptorPool(_device, _descriptorPool, null);
            vkDestroyCommandPool(_device, _graphicsCommandPool, null);
            vkDestroyFence(_device, _imageAvailableFence, null);

            while (_sharedGraphicsCommandPools.TryPop(out SharedCommandPool sharedPool))
            {
                sharedPool.Destroy();
            }

            _memoryManager.Dispose();

            VkResult result = vkDeviceWaitIdle(_device);
            CheckResult(result);
            vkDestroyDevice(_device, null);
            vkDestroyInstance(_instance, null);
        }

        public override void WaitForIdle()
        {
            vkQueueWaitIdle(_graphicsQueue);
            lock (_commandListsLock)
            {
                lock (_deferredDisposalLock)
                {
                    foreach (VkCommandList vkCL in _submittedCommandLists)
                    {
                        foreach (VkDeferredDisposal vdd in vkCL.ReferencedResources)
                        {
                            if (vdd.ReferenceTracker.Decrement() == 0)
                            {
                                if (_deferredDisposals.Remove(vdd))
                                {
                                    vdd.DestroyResources();
                                }
                            }
                        }
                    }
                    _submittedCommandLists.Clear();
                }
            }
            FlushQueuedDisposables();
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

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            VkBuffer vkBuffer = Util.AssertSubtype<Buffer, VkBuffer>(buffer);
            VkMemoryBlock memoryBlock = null;
            Vulkan.VkBuffer copySrcBuffer = Vulkan.VkBuffer.Null;
            IntPtr mappedPtr;
            bool isPersistentMapped = vkBuffer.Memory.IsPersistentMapped;
            if (isPersistentMapped)
            {
                mappedPtr = (IntPtr)vkBuffer.Memory.BlockMappedPointer;
            }
            else
            {
                VkBufferCreateInfo bufferCI = VkBufferCreateInfo.New();
                bufferCI.usage = VkBufferUsageFlags.TransferSrc;
                bufferCI.size = vkBuffer.BufferMemoryRequirements.size;
                VkResult result = vkCreateBuffer(Device, ref bufferCI, null, out copySrcBuffer);
                CheckResult(result);

                vkGetBufferMemoryRequirements(Device, copySrcBuffer, out VkMemoryRequirements memReqs);

                memoryBlock = MemoryManager.Allocate(
                    PhysicalDeviceMemProperties,
                    memReqs.memoryTypeBits,
                    VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                    true,
                    memReqs.size,
                    memReqs.alignment);

                result = vkBindBufferMemory(Device, copySrcBuffer, memoryBlock.DeviceMemory, memoryBlock.Offset);
                CheckResult(result);

                mappedPtr = (IntPtr)memoryBlock.BlockMappedPointer;
            }

            byte* destPtr = (byte*)mappedPtr + bufferOffsetInBytes;
            Unsafe.CopyBlock(destPtr, source.ToPointer(), sizeInBytes);

            if (!isPersistentMapped)
            {
                SharedCommandPool pool = GetFreeCommandPool();
                VkCommandBuffer cb = pool.BeginNewCommandBuffer();

                VkBufferCopy copyRegion = new VkBufferCopy { size = vkBuffer.BufferMemoryRequirements.size };
                vkCmdCopyBuffer(cb, copySrcBuffer, vkBuffer.DeviceBuffer, 1, ref copyRegion);

                _buffersToDestroy.Enqueue(copySrcBuffer);
                _memoriesToFree.Enqueue(memoryBlock);
                pool.EndAndSubmit(cb);
            }
        }

        private SharedCommandPool GetFreeCommandPool()
        {
            if (!_sharedGraphicsCommandPools.TryPop(out SharedCommandPool sharedPool))
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

        public override void UpdateTexture(
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
            VkTexture tex = Util.AssertSubtype<Texture, VkTexture>(texture);

            if (x != 0 || y != 0)
            {
                throw new NotImplementedException();
            }

            bool createStaging = (texture.Usage & TextureUsage.Staging) == 0;
            VkImage tempStagingImage = default(VkImage);
            VkMemoryBlock tempStagingMemory = default(VkMemoryBlock);
            void* mappedPtr;
            ulong rowPitch;

            if (createStaging)
            {
                // If the destination texture is not a staging texture, then create a temporary one.
                CreateImage(
                    Device,
                    PhysicalDeviceMemProperties,
                    MemoryManager,
                    width,
                    height,
                    depth,
                    1,
                    VkFormats.VdToVkPixelFormat(tex.Format),
                    VkImageTiling.Linear,
                    VkImageUsageFlags.TransferSrc,
                    VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                    out tempStagingImage,
                    out tempStagingMemory);

                VkImageSubresource subresource = new VkImageSubresource();
                subresource.aspectMask = VkImageAspectFlags.Color;
                subresource.mipLevel = 0;
                subresource.arrayLayer = 0;
                vkGetImageSubresourceLayout(Device, tempStagingImage, ref subresource, out VkSubresourceLayout stagingLayout);
                rowPitch = stagingLayout.rowPitch;

                VkResult result = vkMapMemory(Device, tempStagingMemory.DeviceMemory, tempStagingMemory.Offset, stagingLayout.size, 0, &mappedPtr);
                CheckResult(result);
            }
            else
            {
                uint subresource = tex.CalculateSubresource(mipLevel, arrayLayer);
                mappedPtr = tex.GetStagingMemoryBlock(subresource).BlockMappedPointer;

                VkImageSubresource vkIS = new VkImageSubresource();
                vkIS.aspectMask = VkImageAspectFlags.Color;
                vkIS.mipLevel = 0;
                vkIS.arrayLayer = 0;
                VkImage actualStagingImage = tex.GetStagingImage(subresource);
                vkGetImageSubresourceLayout(Device, actualStagingImage, ref vkIS, out VkSubresourceLayout stagingLayout);
                rowPitch = stagingLayout.rowPitch;
            }

            if (rowPitch == width)
            {
                System.Buffer.MemoryCopy(source.ToPointer(), mappedPtr, sizeInBytes, sizeInBytes);
            }
            else
            {
                uint pixelSizeInBytes = FormatHelpers.GetSizeInBytes(texture.Format);
                for (uint yy = 0; yy < height; yy++)
                {
                    byte* dstRowStart = ((byte*)mappedPtr) + (rowPitch * yy);
                    byte* srcRowStart = ((byte*)source.ToPointer()) + (width * yy * pixelSizeInBytes);
                    Unsafe.CopyBlock(dstRowStart, srcRowStart, width * pixelSizeInBytes);
                }
            }

            if (createStaging)
            {
                vkUnmapMemory(Device, tempStagingMemory.DeviceMemory);
                SharedCommandPool pool = GetFreeCommandPool();
                VkCommandBuffer cb = pool.BeginNewCommandBuffer();
                TransitionImageLayout(cb, tempStagingImage, 0, 1, 0, 1, VkImageLayout.Preinitialized, VkImageLayout.TransferSrcOptimal);
                tex.TransitionImageLayout(cb, mipLevel, 1, arrayLayer, 1, VkImageLayout.TransferDstOptimal);
                CopyFromStagingImage(cb, tempStagingImage, 0, tex.OptimalDeviceImage, mipLevel, width, height, arrayLayer);
                tex.TransitionImageLayout(cb, mipLevel, 1, arrayLayer, 1, VkImageLayout.ShaderReadOnlyOptimal);

                pool.EndAndSubmit(cb);

                _imagesToDestroy.Enqueue(tempStagingImage);
                _memoriesToFree.Enqueue(tempStagingMemory);
            }
        }

        protected void CopyFromStagingImage(
            VkCommandBuffer cb,
            VkImage srcImage,
            uint srcMipLevel,
            VkImage dstImage,
            uint dstMipLevel,
            uint width,
            uint height,
            uint baseArrayLayer = 0)
        {
            VkImageSubresourceLayers srcSubresource = new VkImageSubresourceLayers();
            srcSubresource.mipLevel = srcMipLevel;
            srcSubresource.layerCount = 1;
            srcSubresource.aspectMask = VkImageAspectFlags.Color;
            srcSubresource.baseArrayLayer = 0;

            VkImageSubresourceLayers dstSubresource = new VkImageSubresourceLayers();
            dstSubresource.mipLevel = dstMipLevel;
            dstSubresource.baseArrayLayer = baseArrayLayer;
            dstSubresource.layerCount = 1;
            dstSubresource.aspectMask = VkImageAspectFlags.Color;

            VkImageCopy region = new VkImageCopy();
            region.dstSubresource = dstSubresource;
            region.srcSubresource = srcSubresource;
            region.extent.width = width;
            region.extent.height = height;
            region.extent.depth = 1;

            vkCmdCopyImage(
                cb,
                srcImage,
                VkImageLayout.TransferSrcOptimal,
                dstImage,
                VkImageLayout.TransferDstOptimal,
                1,
                ref region);
        }

        private class SharedCommandPool
        {
            private readonly VkGraphicsDevice _gd;
            private readonly VkCommandPool _pool;
            public bool IsCached { get; }

            public SharedCommandPool(VkGraphicsDevice gd, bool isCached)
            {
                _gd = gd;
                IsCached = isCached;

                VkCommandPoolCreateInfo commandPoolCI = VkCommandPoolCreateInfo.New();
                commandPoolCI.flags = VkCommandPoolCreateFlags.Transient;
                commandPoolCI.queueFamilyIndex = _gd.GraphicsQueueIndex;
                VkResult result = vkCreateCommandPool(_gd.Device, ref commandPoolCI, null, out _pool);
                CheckResult(result);
            }

            public VkCommandBuffer BeginNewCommandBuffer()
            {
                VkCommandBufferAllocateInfo allocateInfo = VkCommandBufferAllocateInfo.New();
                allocateInfo.commandBufferCount = 1;
                allocateInfo.level = VkCommandBufferLevel.Primary;
                allocateInfo.commandPool = _pool;
                VkResult result = vkAllocateCommandBuffers(_gd.Device, ref allocateInfo, out VkCommandBuffer cb);
                CheckResult(result);

                VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;
                result = vkBeginCommandBuffer(cb, ref beginInfo);
                CheckResult(result);

                return cb;
            }

            public void EndAndSubmit(VkCommandBuffer cb)
            {
                VkResult result = vkEndCommandBuffer(cb);
                CheckResult(result);
                _gd.SubmitCommandBuffer(cb);
                _gd._commandBuffersToFree.Enqueue(this);
            }

            public void Reset()
            {
                VkResult result = vkResetCommandPool(_gd.Device, _pool, VkCommandPoolResetFlags.None);
                CheckResult(result);
            }

            internal void Destroy()
            {
                vkDestroyCommandPool(_gd.Device, _pool, null);
            }
        }
    }

    internal unsafe delegate VkResult vkCreateDebugReportCallbackEXT_d(
        VkInstance instance,
        VkDebugReportCallbackCreateInfoEXT* createInfo,
        IntPtr allocatorPtr,
        out VkDebugReportCallbackEXT ret);

    internal unsafe delegate VkResult vkDestroyDebugReportCallbackEXT_d(
        VkInstance instance,
        VkDebugReportCallbackEXT callback,
        VkAllocationCallbacks* pAllocator);

    internal unsafe delegate VkResult vkDebugMarkerSetObjectNameEXT_d(VkDevice device, VkDebugMarkerObjectNameInfoEXT* pNameInfo);
}
