using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System;

using static Veldrid.WebGL.WebGLConstants;

namespace Veldrid.WebGL
{
    internal unsafe class WebGLPipeline : Pipeline
    {
        private const uint GL_INVALID_INDEX = 0xFFFFFFFF;
        private readonly WebGLGraphicsDevice _gd;

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
        public Shader ComputeShader { get; }

        private WebGLDotNET.WebGLProgram _program;
        private bool _disposed;

        private SetBindingsInfo[] _setInfos;

        public int[] VertexStrides { get; }

        public WebGLDotNET.WebGLProgram Program => _program;

        public uint GetUniformBufferCount(uint setSlot) => _setInfos[setSlot].UniformBufferCount;
        public uint GetShaderStorageBufferCount(uint setSlot) => _setInfos[setSlot].ShaderStorageBufferCount;

        public override string Name { get; set; }

        public WebGLPipeline(WebGLGraphicsDevice gd, ref GraphicsPipelineDescription description)
            : base(ref description)
        {
            _gd = gd;
            GraphicsShaders = Util.ShallowClone(description.ShaderSet.Shaders);
            VertexLayouts = Util.ShallowClone(description.ShaderSet.VertexLayouts);
            BlendState = description.BlendState.ShallowClone();
            DepthStencilState = description.DepthStencilState;
            RasterizerState = description.RasterizerState;
            PrimitiveTopology = description.PrimitiveTopology;

            int numVertexBuffers = description.ShaderSet.VertexLayouts.Length;
            VertexStrides = new int[numVertexBuffers];
            for (int i = 0; i < numVertexBuffers; i++)
            {
                VertexStrides[i] = (int)description.ShaderSet.VertexLayouts[i].Stride;
            }

#if !VALIDATE_USAGE
            ResourceLayouts = Util.ShallowClone(description.ResourceLayouts);
#endif

            _program = _gd.Ctx.CreateProgram();
            _gd.CheckError();
            foreach (Shader stage in GraphicsShaders)
            {
                WebGLShader wglShader = Util.AssertSubtype<Shader, WebGLShader>(stage);
                _gd.Ctx.AttachShader(_program, wglShader.WglShader);
                _gd.CheckError();
            }

            uint slot = 0;
            foreach (VertexLayoutDescription layoutDesc in VertexLayouts)
            {
                for (int i = 0; i < layoutDesc.Elements.Length; i++)
                {
                    string elementName = layoutDesc.Elements[i].Name;
                    _gd.Ctx.BindAttribLocation(_program, slot, elementName);
                    _gd.CheckError();

                    slot += 1;
                }
            }

            _gd.Ctx.LinkProgram(_program);
            _gd.CheckError();

#if DEBUG && GL_VALIDATE_VERTEX_INPUT_ELEMENTS
            slot = 0;
            foreach (VertexLayoutDescription layoutDesc in VertexLayouts)
            {
                for (int i = 0; i < layoutDesc.Elements.Length; i++)
                {
                    string elementName = layoutDesc.Elements[i].Name;
                    int location = _gd.Ctx.GetAttribLocation(_program, elementName);
                    if (location == -1)
                    {
                        throw new VeldridException("There was no attribute variable with the name " + layoutDesc.Elements[i].Name);
                    }
                    slot += 1;
                }
            }
#endif

            bool linkSuccess = (bool)_gd.Ctx.GetProgramParameter(_program, LINK_STATUS);
            _gd.CheckError();
            if (!linkSuccess)
            {
                string log = _gd.Ctx.GetProgramInfoLog(_program);
                _gd.CheckError();
                throw new VeldridException($"Error linking GL program: {log}");
            }

            ProcessResourceSetLayouts(ResourceLayouts);
        }

        private void ProcessResourceSetLayouts(ResourceLayout[] layouts)
        {
            int resourceLayoutCount = layouts.Length;
            _setInfos = new SetBindingsInfo[resourceLayoutCount];
            int relativeTextureIndex = -1;
            int relativeImageIndex = -1;
            for (uint setSlot = 0; setSlot < resourceLayoutCount; setSlot++)
            {
                ResourceLayout setLayout = layouts[setSlot];
                WebGLResourceLayout glSetLayout = Util.AssertSubtype<ResourceLayout, WebGLResourceLayout>(setLayout);
                ResourceLayoutElementDescription[] resources = glSetLayout.Elements;

                Dictionary<uint, WebGLUniformBinding> uniformBindings = new Dictionary<uint, WebGLUniformBinding>();
                Dictionary<uint, WebGLTextureBindingSlotInfo> textureBindings = new Dictionary<uint, WebGLTextureBindingSlotInfo>();
                Dictionary<uint, WebGLSamplerBindingSlotInfo> samplerBindings = new Dictionary<uint, WebGLSamplerBindingSlotInfo>();
                Dictionary<uint, WebGLShaderStorageBinding> storageBufferBindings = new Dictionary<uint, WebGLShaderStorageBinding>();

                List<int> samplerTrackedRelativeTextureIndices = new List<int>();
                for (uint i = 0; i < resources.Length; i++)
                {
                    ResourceLayoutElementDescription resource = resources[i];
                    if (resource.Kind == ResourceKind.UniformBuffer)
                    {
                        string resourceName = resource.Name;
                        uint blockIndex = _gd.Ctx.GetUniformBlockIndex(_program, resourceName);
                        _gd.CheckError();
                        if (blockIndex != GL_INVALID_INDEX)
                        {
                            int blockSize = (int)_gd.Ctx.GetActiveUniformBlockParameter(_program, blockIndex, UNIFORM_BLOCK_DATA_SIZE);
                            _gd.CheckError();
                            uniformBindings[i] = new WebGLUniformBinding(_program, blockIndex, (uint)blockSize);
                        }
                    }
                    else if (resource.Kind == ResourceKind.TextureReadOnly)
                    {
                        string resourceName = resource.Name;
                        WebGLDotNET.WebGLUniformLocation location = _gd.Ctx.GetUniformLocation(_program, resourceName);
                        _gd.CheckError();
                        relativeTextureIndex += 1;
                        textureBindings[i] = new WebGLTextureBindingSlotInfo() { RelativeIndex = relativeTextureIndex, UniformLocation = location };
                        WebGLDotNET.WebGLUniformLocation lastTextureLocation = location;
                        samplerTrackedRelativeTextureIndices.Add(relativeTextureIndex);
                    }
                    else if (resource.Kind == ResourceKind.TextureReadWrite)
                    {
                        string resourceName = resource.Name;
                        WebGLDotNET.WebGLUniformLocation location = _gd.Ctx.GetUniformLocation(_program, resourceName);
                        _gd.CheckError();
                        relativeImageIndex += 1;
                        textureBindings[i] = new WebGLTextureBindingSlotInfo() { RelativeIndex = relativeImageIndex, UniformLocation = location };
                    }
                    else if (resource.Kind == ResourceKind.StructuredBufferReadOnly
                        || resource.Kind == ResourceKind.StructuredBufferReadWrite)
                    {
                        throw new NotSupportedException();
                    }
                    else
                    {
                        Debug.Assert(resource.Kind == ResourceKind.Sampler);

                        int[] relativeIndices = samplerTrackedRelativeTextureIndices.ToArray();
                        samplerTrackedRelativeTextureIndices.Clear();
                        samplerBindings[i] = new WebGLSamplerBindingSlotInfo()
                        {
                            RelativeIndices = relativeIndices
                        };
                    }
                }

                _setInfos[setSlot] = new SetBindingsInfo(uniformBindings, textureBindings, samplerBindings, storageBufferBindings);
            }
        }

        public bool GetUniformBindingForSlot(uint set, uint slot, out WebGLUniformBinding binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            SetBindingsInfo setInfo = _setInfos[set];
            return setInfo.GetUniformBindingForSlot(slot, out binding);
        }

        public bool GetTextureBindingInfo(uint set, uint slot, out WebGLTextureBindingSlotInfo binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            SetBindingsInfo setInfo = _setInfos[set];
            return setInfo.GetTextureBindingInfo(slot, out binding);
        }

        public bool GetSamplerBindingInfo(uint set, uint slot, out WebGLSamplerBindingSlotInfo binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            SetBindingsInfo setInfo = _setInfos[set];
            return setInfo.GetSamplerBindingInfo(slot, out binding);
        }

        public bool GetStorageBufferBindingForSlot(uint set, uint slot, out WebGLShaderStorageBinding binding)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            SetBindingsInfo setInfo = _setInfos[set];
            return setInfo.GetStorageBufferBindingForSlot(slot, out binding);

        }

        public override void Dispose()
        {
            Program.Dispose();
        }
    }

    internal struct SetBindingsInfo
    {
        private readonly Dictionary<uint, WebGLUniformBinding> _uniformBindings;
        private readonly Dictionary<uint, WebGLTextureBindingSlotInfo> _textureBindings;
        private readonly Dictionary<uint, WebGLSamplerBindingSlotInfo> _samplerBindings;
        private readonly Dictionary<uint, WebGLShaderStorageBinding> _storageBufferBindings;

        public uint UniformBufferCount { get; }
        public uint ShaderStorageBufferCount { get; }

        public SetBindingsInfo(
            Dictionary<uint, WebGLUniformBinding> uniformBindings,
            Dictionary<uint, WebGLTextureBindingSlotInfo> textureBindings,
            Dictionary<uint, WebGLSamplerBindingSlotInfo> samplerBindings,
            Dictionary<uint, WebGLShaderStorageBinding> storageBufferBindings)
        {
            _uniformBindings = uniformBindings;
            UniformBufferCount = (uint)uniformBindings.Count;
            _textureBindings = textureBindings;
            _samplerBindings = samplerBindings;
            _storageBufferBindings = storageBufferBindings;
            ShaderStorageBufferCount = (uint)storageBufferBindings.Count;
        }

        public bool GetTextureBindingInfo(uint slot, out WebGLTextureBindingSlotInfo binding)
        {
            return _textureBindings.TryGetValue(slot, out binding);
        }

        public bool GetSamplerBindingInfo(uint slot, out WebGLSamplerBindingSlotInfo binding)
        {
            return _samplerBindings.TryGetValue(slot, out binding);
        }

        public bool GetUniformBindingForSlot(uint slot, out WebGLUniformBinding binding)
        {
            return _uniformBindings.TryGetValue(slot, out binding);
        }

        public bool GetStorageBufferBindingForSlot(uint slot, out WebGLShaderStorageBinding binding)
        {
            return _storageBufferBindings.TryGetValue(slot, out binding);
        }
    }

    internal struct WebGLTextureBindingSlotInfo
    {
        /// <summary>
        /// The relative index of this binding with relation to the other textures used by a shader.
        /// Generally, this is the texture unit that the binding will be placed into.
        /// </summary>
        public int RelativeIndex;
        /// <summary>
        /// The uniform location of the binding in the shader program.
        /// </summary>
        public WebGLDotNET.WebGLUniformLocation UniformLocation;
    }

    internal struct WebGLSamplerBindingSlotInfo
    {
        /// <summary>
        /// The relative indices of this binding with relation to the other textures used by a shader.
        /// Generally, these are the texture units that the sampler will be bound to.
        /// </summary>
        public int[] RelativeIndices;
    }

    internal class WebGLUniformBinding
    {
        public WebGLDotNET.WebGLProgram Program { get; }
        public uint BlockLocation { get; }
        public uint BlockSize { get; }

        public WebGLUniformBinding(WebGLDotNET.WebGLProgram program, uint blockLocation, uint blockSize)
        {
            Program = program;
            BlockLocation = blockLocation;
            BlockSize = blockSize;
        }
    }

    internal class WebGLShaderStorageBinding
    {
        public uint StorageBlockBinding { get; }

        public WebGLShaderStorageBinding(uint storageBlockBinding)
        {
            StorageBlockBinding = storageBlockBinding;
        }
    }
}
