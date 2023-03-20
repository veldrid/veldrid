#define GL_VALIDATE_SHADER_RESOURCE_NAMES
#define GL_VALIDATE_VERTEX_INPUT_ELEMENTS

using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

namespace Veldrid.OpenGL
{
    internal sealed unsafe class OpenGLPipeline : Pipeline, OpenGLDeferredResource
    {
        private const uint GL_INVALID_INDEX = 0xFFFFFFFF;
        private readonly OpenGLGraphicsDevice _gd;

#if !VALIDATE_USAGE
        public ResourceLayout[] ResourceLayouts { get; }
#endif

        // Graphics Pipeline
        public Shader[] GraphicsShaders { get; }
        public VertexLayoutDescription[] VertexLayouts { get; }
        public BlendStateDescription BlendState { get; }
        public DepthStencilStateDescription DepthStencilState { get; }
        public RasterizerStateDescription RasterizerState { get; }
        public PrimitiveTopology PrimitiveTopology { get; }

        // Compute Pipeline
        public override bool IsComputePipeline { get; }
        public Shader? ComputeShader { get; }

        private uint _program;
        private uint _vao;

        private bool _disposeRequested;
        private bool _disposed;

        private SetBindingsInfo[] _setInfos = Array.Empty<SetBindingsInfo>();

        public uint[] VertexStrides { get; }

        public uint Program => _program;
        public uint Vao => _vao;

        public uint GetUniformBufferCount(uint setSlot) => _setInfos[setSlot].UniformBufferCount;
        public uint GetShaderStorageBufferCount(uint setSlot) => _setInfos[setSlot].ShaderStorageBufferCount;

        public override string? Name { get; set; }

        public override bool IsDisposed => _disposeRequested;

        public OpenGLPipeline(OpenGLGraphicsDevice gd, in GraphicsPipelineDescription description)
            : base(description)
        {
            _gd = gd;
            GraphicsShaders = Util.ShallowClone(description.ShaderSet.Shaders);
            VertexLayouts = Util.ShallowClone(description.ShaderSet.VertexLayouts);
            BlendState = description.BlendState.ShallowClone();
            DepthStencilState = description.DepthStencilState;
            RasterizerState = description.RasterizerState;
            PrimitiveTopology = description.PrimitiveTopology;

            VertexStrides = new uint[VertexLayouts.Length];
            for (int i = 0; i < VertexStrides.Length; i++)
            {
                VertexStrides[i] = VertexLayouts[i].Stride;
            }

#if !VALIDATE_USAGE
            ResourceLayouts = Util.ShallowClone(description.ResourceLayouts);
#endif
        }

        public OpenGLPipeline(OpenGLGraphicsDevice gd, in ComputePipelineDescription description)
            : base(description)
        {
            _gd = gd;
            GraphicsShaders = Array.Empty<Shader>();
            VertexLayouts = Array.Empty<VertexLayoutDescription>();
            IsComputePipeline = true;
            ComputeShader = description.ComputeShader;
            VertexStrides = Array.Empty<uint>();
#if !VALIDATE_USAGE
            ResourceLayouts = Util.ShallowClone(description.ResourceLayouts);
#endif
        }

        public bool Created { get; private set; }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources();
            }
        }

        private void CreateGLResources()
        {
            if (!IsComputePipeline)
            {
                CreateGraphicsGLResources();
            }
            else
            {
                CreateComputeGLResources();
            }

            Created = true;
        }

        [SkipLocalsInit]
        private void CreateGraphicsGLResources()
        {
            _program = glCreateProgram();
            CheckLastError();

            foreach (Shader stage in GraphicsShaders)
            {
                OpenGLShader glShader = Util.AssertSubtype<Shader, OpenGLShader>(stage);
                glShader.EnsureResourcesCreated();

                glAttachShader(_program, glShader.Shader);
                CheckLastError();
            }

            Span<byte> byteBuffer = stackalloc byte[4096];

            uint slot = 0;
            foreach (VertexLayoutDescription layoutDesc in VertexLayouts)
            {
                for (int i = 0; i < layoutDesc.Elements.Length; i++)
                {
                    string elementName = layoutDesc.Elements[i].Name;
                    BindAttribLocation(slot, elementName, ref byteBuffer);
                    slot += 1;
                }
            }

            glLinkProgram(_program);
            CheckLastError();

#if GL_VALIDATE_VERTEX_INPUT_ELEMENTS
            if (_gd.IsDebug)
            {
                slot = 0;
                foreach (VertexLayoutDescription layoutDesc in VertexLayouts)
                {
                    for (int i = 0; i < layoutDesc.Elements.Length; i++)
                    {
                        string elementName = layoutDesc.Elements[i].Name;
                        int location = GetAttribLocation(elementName, ref byteBuffer);
                        if (location == -1)
                        {
                            throw new VeldridException(
                                "There was no attribute variable with the name " + layoutDesc.Elements[i].Name + ". " +
                                "The compiler may have optimized out unused attribute variables.");
                        }
                        slot += 1;
                    }
                }
            }
#endif

            ProcessLinkedProgram(byteBuffer);

            _vao = CreateVertexArrayObject();
        }

        private uint CreateVertexArrayObject()
        {
            // Save the previous bound vertex array
            int restoreVertexArray = 0;
            glGetIntegerv(GetPName.VertexArrayBinding, &restoreVertexArray);

            uint vao;
            glGenVertexArrays(1, &vao);
            CheckLastError();

            glBindVertexArray(vao);
            CheckLastError();

            uint totalSlotsBound = 0;
            VertexLayoutDescription[] layouts = VertexLayouts;

            bool separateBinding = _gd.Extensions.ARB_vertex_attrib_binding;

            for (int i = 0; i < layouts.Length; i++)
            {
                VertexLayoutDescription input = layouts[i];

                uint offset = 0;

                for (uint slot = 0; slot < input.Elements.Length; slot++)
                {
                    uint actualSlot = totalSlotsBound + slot;

                    glEnableVertexAttribArray(actualSlot);
                    CheckLastError();

                    if (separateBinding)
                    {
                        ref readonly VertexElementDescription element = ref input.Elements[slot];

                        int elementCount = FormatHelpers.GetElementCount(element.Format);
                        VertexAttribPointerType type = OpenGLFormats.VdToGLVertexAttribPointerType(
                            element.Format,
                            out bool normalized,
                            out bool isInteger);

                        uint actualOffset = element.Offset != 0 ? element.Offset : offset;

                        if (isInteger && !normalized)
                        {
                            glVertexAttribIFormat(
                                actualSlot,
                                elementCount,
                                type,
                                actualOffset);
                        }
                        else
                        {
                            glVertexAttribFormat(
                                actualSlot,
                                elementCount,
                                type,
                                normalized,
                                actualOffset);
                        }
                        CheckLastError();

                        glVertexAttribBinding(actualSlot, (uint)i);
                        CheckLastError();

                        offset += FormatSizeHelpers.GetSizeInBytes(element.Format);
                    }
                    else
                    {
                        glVertexAttribDivisor(actualSlot, input.InstanceStepRate);
                        CheckLastError();
                    }
                }

                if (separateBinding)
                {
                    glVertexBindingDivisor((uint)i, input.InstanceStepRate);
                    CheckLastError();
                }

                totalSlotsBound += (uint)input.Elements.Length;
            }

            glBindVertexArray((uint)restoreVertexArray);
            CheckLastError();

            return vao;
        }

        private int GetAttribLocation(ReadOnlySpan<char> elementName, ref Span<byte> byteBuffer)
        {
            Util.GetNullTerminatedUtf8(elementName, ref byteBuffer);

            int location;
            fixed (byte* byteBufferPtr = byteBuffer)
            {
                location = glGetAttribLocation(_program, byteBufferPtr);
            }
            CheckLastError();

            return location;
        }

        private void BindAttribLocation(uint slot, ReadOnlySpan<char> elementName, ref Span<byte> byteBuffer)
        {
            Util.GetNullTerminatedUtf8(elementName, ref byteBuffer);

            fixed (byte* byteBufferPtr = byteBuffer)
            {
                glBindAttribLocation(_program, slot, byteBufferPtr);
            }
            CheckLastError();
        }

        private void ProcessResourceSetLayouts(ResourceLayout[] layouts, Span<byte> byteBuffer)
        {
            int resourceLayoutCount = layouts.Length;
            _setInfos = new SetBindingsInfo[resourceLayoutCount];
            int relativeTextureIndex = -1;
            int relativeImageIndex = -1;
            uint storageBlockIndex = 0; // Tracks OpenGL ES storage buffers.

            for (uint setSlot = 0; setSlot < resourceLayoutCount; setSlot++)
            {
                ResourceLayout setLayout = layouts[setSlot];
                OpenGLResourceLayout glSetLayout = Util.AssertSubtype<ResourceLayout, OpenGLResourceLayout>(setLayout);
                ResourceLayoutElementDescription[] resources = glSetLayout.Elements;

                Dictionary<uint, OpenGLUniformBinding> uniformBindings = new();
                Dictionary<uint, OpenGLTextureBindingSlotInfo> textureBindings = new();
                Dictionary<uint, OpenGLSamplerBindingSlotInfo> samplerBindings = new();
                Dictionary<uint, OpenGLShaderStorageBinding> storageBufferBindings = new();

                List<int> samplerTrackedRelativeTextureIndices = new();
                for (uint i = 0; i < resources.Length; i++)
                {
                    ResourceLayoutElementDescription resource = resources[i];
                    if (resource.Kind == ResourceKind.UniformBuffer)
                    {
                        uint blockIndex = GetUniformBlockIndex(resource.Name, ref byteBuffer);
                        if (blockIndex != GL_INVALID_INDEX)
                        {
                            int blockSize;
                            glGetActiveUniformBlockiv(_program, blockIndex, ActiveUniformBlockParameter.UniformBlockDataSize, &blockSize);
                            CheckLastError();
                            uniformBindings[i] = new OpenGLUniformBinding(_program, blockIndex, (uint)blockSize);
                        }
                    }
                    else if (resource.Kind == ResourceKind.TextureReadOnly)
                    {
                        int location = GetUniformLocation(resource.Name, ref byteBuffer);
                        relativeTextureIndex += 1;
                        textureBindings[i] = new OpenGLTextureBindingSlotInfo()
                        {
                            RelativeIndex = relativeTextureIndex,
                            UniformLocation = location
                        };
                        samplerTrackedRelativeTextureIndices.Add(relativeTextureIndex);
                    }
                    else if (resource.Kind == ResourceKind.TextureReadWrite)
                    {
                        int location = GetUniformLocation(resource.Name, ref byteBuffer);
                        relativeImageIndex += 1;
                        textureBindings[i] = new OpenGLTextureBindingSlotInfo()
                        {
                            RelativeIndex = relativeImageIndex,
                            UniformLocation = location
                        };
                    }
                    else if (resource.Kind == ResourceKind.StructuredBufferReadOnly
                        || resource.Kind == ResourceKind.StructuredBufferReadWrite)
                    {
                        uint storageBlockBinding;
                        if (_gd.Extensions.ARB_program_interface_query && _gd.BackendType == GraphicsBackend.OpenGL)
                        {
                            storageBlockBinding = GetProgramResourceIndex(
                                resource.Name, ProgramInterface.ShaderStorageBlock, ref byteBuffer);
                        }
                        else
                        {
                            storageBlockBinding = storageBlockIndex;
                            storageBlockIndex += 1;
                        }

                        storageBufferBindings[i] = new OpenGLShaderStorageBinding(storageBlockBinding);
                    }
                    else
                    {
                        Debug.Assert(resource.Kind == ResourceKind.Sampler);

                        int[] relativeIndices = samplerTrackedRelativeTextureIndices.ToArray();
                        samplerTrackedRelativeTextureIndices.Clear();
                        samplerBindings[i] = new OpenGLSamplerBindingSlotInfo()
                        {
                            RelativeIndices = relativeIndices
                        };
                    }
                }

                _setInfos[setSlot] = new SetBindingsInfo(uniformBindings, textureBindings, samplerBindings, storageBufferBindings);
            }
        }

        private uint GetUniformBlockIndex(ReadOnlySpan<char> resourceName, ref Span<byte> byteBuffer)
        {
            Util.GetNullTerminatedUtf8(resourceName, ref byteBuffer);

            uint blockIndex;
            fixed (byte* byteBufferPtr = byteBuffer)
            {
                blockIndex = glGetUniformBlockIndex(_program, byteBufferPtr);
            }
            CheckLastError();

            if (blockIndex == GL_INVALID_INDEX && _gd.IsDebug)
            {
                ReportInvalidUniformBlockName(resourceName, byteBuffer);
            }
            return blockIndex;
        }

        private int GetUniformLocation(ReadOnlySpan<char> resourceName, ref Span<byte> byteBuffer)
        {
            Util.GetNullTerminatedUtf8(resourceName, ref byteBuffer);

            int location;
            fixed (byte* byteBufferPtr = byteBuffer)
            {
                location = glGetUniformLocation(_program, byteBufferPtr);
            }
            CheckLastError();

            if (location == -1 && _gd.IsDebug)
            {
                ReportInvalidUniformName(resourceName, byteBuffer);
            }
            return location;
        }

        private uint GetProgramResourceIndex(ReadOnlySpan<char> resourceName, ProgramInterface resourceType, ref Span<byte> byteBuffer)
        {
            Util.GetNullTerminatedUtf8(resourceName, ref byteBuffer);

            uint index;
            fixed (byte* byteBufferPtr = byteBuffer)
            {
                index = glGetProgramResourceIndex(_program, resourceType, byteBufferPtr);
            }
            CheckLastError();

            if (index == GL_INVALID_INDEX && _gd.IsDebug)
            {
                ReportInvalidResourceName(resourceName, resourceType, ref byteBuffer);
            }
            return index;
        }

        [Conditional("GL_VALIDATE_SHADER_RESOURCE_NAMES")]
        private void ReportInvalidUniformBlockName(ReadOnlySpan<char> uniformName, Span<byte> byteBuffer)
        {
            VerifyLastError();

            uint uniformBufferIndex = 0;

            List<string> names = new();
            while (true)
            {
                uint actualLength;
                fixed (byte* byteBufferPtr = byteBuffer)
                {
                    glGetActiveUniformBlockName(_program, uniformBufferIndex, (uint)byteBuffer.Length, &actualLength, byteBufferPtr);
                }

                if (glGetError() != 0)
                {
                    break;
                }

                string name = Encoding.UTF8.GetString(byteBuffer.Slice(0, (int)actualLength));
                names.Add(name);
                uniformBufferIndex++;
            }

            throw new VeldridException(
                $"Unable to bind uniform block buffer \"{uniformName}\" by name. " +
                $"Valid names for this pipeline are: {string.Join(", ", names)}");
        }

        [Conditional("GL_VALIDATE_SHADER_RESOURCE_NAMES")]
        private void ReportInvalidUniformName(ReadOnlySpan<char> uniformName, Span<byte> byteBuffer)
        {
            VerifyLastError();

            uint uniformIndex = 0;

            List<string> names = new();
            while (true)
            {
                uint actualLength;
                int size;
                uint type;
                fixed (byte* byteBufferPtr = byteBuffer)
                {
                    glGetActiveUniform(_program, uniformIndex, (uint)byteBuffer.Length,
                        &actualLength, &size, &type, byteBufferPtr);
                }

                if (glGetError() != 0)
                {
                    break;
                }

                string name = Encoding.UTF8.GetString(byteBuffer.Slice(0, (int)actualLength));
                names.Add(name);
                uniformIndex++;
            }

            throw new VeldridException(
                $"Unable to bind uniform \"{uniformName}\" by name. " +
                $"Valid names for this pipeline are: {string.Join(", ", names)}");
        }

        [Conditional("GL_VALIDATE_SHADER_RESOURCE_NAMES")]
        private void ReportInvalidResourceName(ReadOnlySpan<char> resourceName, ProgramInterface resourceType, ref Span<byte> byteBuffer)
        {
            VerifyLastError();

            int maxLength = 0;
            int resourceCount = 0;
            glGetProgramInterfaceiv(_program, resourceType, ProgramInterfaceParameterName.MaxNameLength, &maxLength);
            glGetProgramInterfaceiv(_program, resourceType, ProgramInterfaceParameterName.ActiveResources, &resourceCount);

            if (maxLength > byteBuffer.Length)
                byteBuffer = new byte[maxLength];

            List<string> names = new();
            for (uint resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
            {
                uint actualLength;
                fixed (byte* byteBufferPtr = byteBuffer)
                {
                    glGetProgramResourceName(_program, resourceType, resourceIndex, (uint)maxLength, &actualLength, byteBufferPtr);
                }

                if (glGetError() != 0)
                {
                    break;
                }

                string name = Encoding.UTF8.GetString(byteBuffer.Slice(0, (int)actualLength));
                names.Add(name);
            }

            throw new VeldridException(
                $"Unable to bind {resourceType} \"{resourceName}\" by name. " +
                $"Valid names for this pipeline are: {string.Join(", ", names)}");
        }

        [SkipLocalsInit]
        private void CreateComputeGLResources()
        {
            _program = glCreateProgram();
            CheckLastError();

            OpenGLShader glShader = Util.AssertSubtype<Shader, OpenGLShader>(ComputeShader!);
            glShader.EnsureResourcesCreated();

            glAttachShader(_program, glShader.Shader);
            CheckLastError();

            glLinkProgram(_program);
            CheckLastError();

            Span<byte> byteBuffer = stackalloc byte[4096];
            ProcessLinkedProgram(byteBuffer);
        }

        private void ProcessLinkedProgram(Span<byte> byteBuffer)
        {
            int linkStatus = 0;
            glGetProgramiv(_program, GetProgramParameterName.LinkStatus, &linkStatus);
            CheckLastError();

            if (linkStatus != 1)
            {
                int infoLogLength;
                glGetProgramiv(_program, GetProgramParameterName.InfoLogLength, &infoLogLength);
                CheckLastError();

                if (infoLogLength > byteBuffer.Length)
                    byteBuffer = new byte[infoLogLength];

                uint bytesWritten = 0;
                fixed (byte* infoLog = byteBuffer)
                {
                    glGetProgramInfoLog(_program, (uint)byteBuffer.Length, &bytesWritten, infoLog);
                }
                CheckLastError();

                string log = Util.UTF8.GetString(byteBuffer[..(int)bytesWritten]);
                throw new VeldridException($"Error linking GL program: {log}");
            }

            ProcessResourceSetLayouts(ResourceLayouts, byteBuffer);
        }

        public bool GetUniformBindingForSlot(uint set, uint slot, out OpenGLUniformBinding binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            return _setInfos[set].GetUniformBindingForSlot(slot, out binding);
        }

        public bool GetTextureBindingInfo(uint set, uint slot, out OpenGLTextureBindingSlotInfo binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            return _setInfos[set].GetTextureBindingInfo(slot, out binding);
        }

        public bool GetSamplerBindingInfo(uint set, uint slot, out OpenGLSamplerBindingSlotInfo binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            return _setInfos[set].GetSamplerBindingInfo(slot, out binding);
        }

        public bool GetStorageBufferBindingForSlot(uint set, uint slot, out OpenGLShaderStorageBinding binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            return _setInfos[set].GetStorageBufferBindingForSlot(slot, out binding);
        }

        public override void Dispose()
        {
            if (!_disposeRequested)
            {
                _disposeRequested = true;
                _gd.EnqueueDisposal(this);
            }
        }

        public void DestroyGLResources()
        {
            if (!_disposed)
            {
                _disposed = true;

                glDeleteProgram(_program);
                CheckLastError();

                uint vao = _vao;
                glDeleteVertexArrays(1, &vao);
                CheckLastError();
            }
        }
    }

    internal readonly struct SetBindingsInfo
    {
        private readonly Dictionary<uint, OpenGLUniformBinding> _uniformBindings;
        private readonly Dictionary<uint, OpenGLTextureBindingSlotInfo> _textureBindings;
        private readonly Dictionary<uint, OpenGLSamplerBindingSlotInfo> _samplerBindings;
        private readonly Dictionary<uint, OpenGLShaderStorageBinding> _storageBufferBindings;

        public uint UniformBufferCount { get; }
        public uint ShaderStorageBufferCount { get; }

        public SetBindingsInfo(
            Dictionary<uint, OpenGLUniformBinding> uniformBindings,
            Dictionary<uint, OpenGLTextureBindingSlotInfo> textureBindings,
            Dictionary<uint, OpenGLSamplerBindingSlotInfo> samplerBindings,
            Dictionary<uint, OpenGLShaderStorageBinding> storageBufferBindings)
        {
            _uniformBindings = uniformBindings;
            UniformBufferCount = (uint)uniformBindings.Count;
            _textureBindings = textureBindings;
            _samplerBindings = samplerBindings;
            _storageBufferBindings = storageBufferBindings;
            ShaderStorageBufferCount = (uint)storageBufferBindings.Count;
        }

        public bool GetTextureBindingInfo(uint slot, out OpenGLTextureBindingSlotInfo binding)
        {
            return _textureBindings.TryGetValue(slot, out binding);
        }

        public bool GetSamplerBindingInfo(uint slot, out OpenGLSamplerBindingSlotInfo binding)
        {
            return _samplerBindings.TryGetValue(slot, out binding);
        }

        public bool GetUniformBindingForSlot(uint slot, out OpenGLUniformBinding binding)
        {
            return _uniformBindings.TryGetValue(slot, out binding);
        }

        public bool GetStorageBufferBindingForSlot(uint slot, out OpenGLShaderStorageBinding binding)
        {
            return _storageBufferBindings.TryGetValue(slot, out binding);
        }
    }

    internal struct OpenGLTextureBindingSlotInfo
    {
        /// <summary>
        /// The relative index of this binding with relation to the other textures used by a shader.
        /// Generally, this is the texture unit that the binding will be placed into.
        /// </summary>
        public int RelativeIndex;

        /// <summary>
        /// The uniform location of the binding in the shader program.
        /// </summary>
        public int UniformLocation;
    }

    internal struct OpenGLSamplerBindingSlotInfo
    {
        /// <summary>
        /// The relative indices of this binding with relation to the other textures used by a shader.
        /// Generally, these are the texture units that the sampler will be bound to.
        /// </summary>
        public int[] RelativeIndices;
    }

    internal readonly struct OpenGLUniformBinding
    {
        public uint Program { get; }
        public uint BlockLocation { get; }
        public uint BlockSize { get; }

        public OpenGLUniformBinding(uint program, uint blockLocation, uint blockSize)
        {
            Program = program;
            BlockLocation = blockLocation;
            BlockSize = blockSize;
        }
    }

    internal readonly struct OpenGLShaderStorageBinding
    {
        public uint StorageBlockBinding { get; }

        public OpenGLShaderStorageBinding(uint storageBlockBinding)
        {
            StorageBlockBinding = storageBlockBinding;
        }
    }
}
