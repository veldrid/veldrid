using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLPipeline : Pipeline, OpenGLDeferredResource
    {
        private const uint GL_INVALID_INDEX = 0xFFFFFFFF;
        private readonly OpenGLGraphicsDevice _gd;
        private uint _program;
        private bool _disposed;

        private SetBindingsInfo[] _setInfos;

        public PipelineDescription Description { get; }

        public int[] VertexStrides { get; }

        public uint Program => _program;

        public uint GetUniformBufferCount(uint setSlot) => _setInfos[setSlot].UniformBufferCount;

        public OpenGLPipeline(OpenGLGraphicsDevice gd, ref PipelineDescription description)
        {
            _gd = gd;
            Description = description;

            int numVertexBuffers = description.ShaderSet.VertexLayouts.Length;
            VertexStrides = new int[numVertexBuffers];
            for (int i = 0; i < numVertexBuffers; i++)
            {
                VertexStrides[i] = (int)description.ShaderSet.VertexLayouts[i].Stride;
            }
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
            ShaderSetDescription shaderSet = Description.ShaderSet;
            _program = glCreateProgram();
            CheckLastError();
            foreach (ShaderStageDescription stageDesc in shaderSet.ShaderStages)
            {
                OpenGLShader glShader = Util.AssertSubtype<Shader, OpenGLShader>(stageDesc.Shader);
                glShader.EnsureResourcesCreated();
                glAttachShader(_program, glShader.Shader);
                CheckLastError();
            }

            uint slot = 0;
            foreach (VertexLayoutDescription layoutDesc in shaderSet.VertexLayouts)
            {
                for (int i = 0; i < layoutDesc.Elements.Length; i++)
                {
                    string elementName = layoutDesc.Elements[i].Name;
                    int byteCount = Encoding.UTF8.GetByteCount(elementName) + 1;
                    byte* elementNamePtr = stackalloc byte[byteCount];
                    fixed (char* charPtr = elementName)
                    {
                        int bytesWritten = Encoding.UTF8.GetBytes(charPtr, elementName.Length, elementNamePtr, byteCount);
                        Debug.Assert(bytesWritten == byteCount - 1);
                    }
                    elementNamePtr[byteCount - 1] = 0; // Add null terminator.

                    glBindAttribLocation(_program, slot, elementNamePtr);
                    CheckLastError();

                    slot += 1;
                }
            }

            glLinkProgram(_program);
            CheckLastError();

#if DEBUG && GL_VALIDATE_VERTEX_INPUT_ELEMENTS
            slot = 0;
            foreach (VertexLayoutDescription layoutDesc in shaderSet.VertexLayouts)
            {
                for (int i = 0; i < layoutDesc.Elements.Length; i++)
                {
                    string elementName = layoutDesc.Elements[i].Name;
                    int byteCount = Encoding.UTF8.GetByteCount(elementName) + 1;
                    byte* elementNamePtr = stackalloc byte[byteCount];
                    fixed (char* charPtr = elementName)
                    {
                        int bytesWritten = Encoding.UTF8.GetBytes(charPtr, elementName.Length, elementNamePtr, byteCount);
                        Debug.Assert(bytesWritten == byteCount - 1);
                    }
                    elementNamePtr[byteCount - 1] = 0; // Add null terminator.

                    int location = glGetAttribLocation(_program, elementNamePtr);
                    if (location == -1)
                    {
                        throw new VeldridException("There was no attribute variable with the name " + layoutDesc.Elements[i].Name);
                    }
                    slot += 1;
                }
            }
#endif

            int linkStatus;
            glGetProgramiv(_program, GetProgramParameterName.LinkStatus, &linkStatus);
            CheckLastError();
            if (linkStatus != 1)
            {
                byte* infoLog = stackalloc byte[4096];
                uint bytesWritten;
                glGetProgramInfoLog(_program, 4096, &bytesWritten, infoLog);
                CheckLastError();
                string log = Encoding.UTF8.GetString(infoLog, (int)bytesWritten);
                throw new VeldridException($"Error linking GL program: {log}");
            }

            int resourceLayoutCount = Description.ResourceLayouts.Length;
            _setInfos = new SetBindingsInfo[resourceLayoutCount];
            int lastTextureLocation = -1;
            int relativeTextureIndex = -1;
            for (uint setSlot = 0; setSlot < resourceLayoutCount; setSlot++)
            {
                ResourceLayout setLayout = Description.ResourceLayouts[setSlot];
                OpenGLResourceLayout glSetLayout = Util.AssertSubtype<ResourceLayout, OpenGLResourceLayout>(setLayout);
                ResourceLayoutElementDescription[] resources = glSetLayout.Description.Elements;

                Dictionary<uint, OpenGLUniformBinding> uniformBindings = new Dictionary<uint, OpenGLUniformBinding>();
                Dictionary<uint, OpenGLTextureBindingSlotInfo> textureBindings = new Dictionary<uint, OpenGLTextureBindingSlotInfo>();
                Dictionary<uint, OpenGLSamplerBindingSlotInfo> samplerBindings = new Dictionary<uint, OpenGLSamplerBindingSlotInfo>();

                List<int> samplerTrackedRelativeTextureIndices = new List<int>();
                for (uint i = 0; i < resources.Length; i++)
                {
                    ResourceLayoutElementDescription resource = resources[i];
                    if (resource.Kind == ResourceKind.UniformBuffer)
                    {
                        string resourceName = resource.Name;
                        int byteCount = Encoding.UTF8.GetByteCount(resourceName) + 1;
                        byte* resourceNamePtr = stackalloc byte[byteCount];
                        fixed (char* charPtr = resourceName)
                        {
                            int bytesWritten = Encoding.UTF8.GetBytes(charPtr, resourceName.Length, resourceNamePtr, byteCount);
                            Debug.Assert(bytesWritten == byteCount - 1);
                        }
                        resourceNamePtr[byteCount - 1] = 0; // Add null terminator.

                        uint blockIndex = glGetUniformBlockIndex(_program, resourceNamePtr);
                        if (blockIndex != GL_INVALID_INDEX)
                        {
                            uniformBindings[i] = new OpenGLUniformBinding(_program, blockIndex);
                        }
                        else
                        {
                            // TODO: Support raw uniform values, not wrapped in a uniform block.
                            throw new System.NotImplementedException();
                        }
                    }
                    else if (resource.Kind == ResourceKind.TextureView)
                    {
                        string resourceName = resource.Name;
                        int byteCount = Encoding.UTF8.GetByteCount(resourceName) + 1;
                        byte* resourceNamePtr = stackalloc byte[byteCount];
                        fixed (char* charPtr = resourceName)
                        {
                            int bytesWritten = Encoding.UTF8.GetBytes(charPtr, resourceName.Length, resourceNamePtr, byteCount);
                            Debug.Assert(bytesWritten == byteCount - 1);
                        }
                        resourceNamePtr[byteCount - 1] = 0; // Add null terminator.
                        int location = glGetUniformLocation(_program, resourceNamePtr);
                        relativeTextureIndex += 1;
                        textureBindings[i] = new OpenGLTextureBindingSlotInfo() { RelativeIndex = relativeTextureIndex, UniformLocation = location };
                        lastTextureLocation = location;
                        samplerTrackedRelativeTextureIndices.Add(relativeTextureIndex);
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

                _setInfos[setSlot] = new SetBindingsInfo(uniformBindings, textureBindings, samplerBindings);
            }

            Created = true;
        }

        public OpenGLUniformBinding GetUniformBindingForSlot(uint set, uint slot)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            SetBindingsInfo setInfo = _setInfos[set];
            return setInfo.GetUniformBindingForSlot(slot);
        }

        public OpenGLTextureBindingSlotInfo GetTextureBindingInfo(uint set, uint slot)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            SetBindingsInfo setInfo = _setInfos[set];
            return setInfo.GetTextureBindingInfo(slot);
        }

        public OpenGLSamplerBindingSlotInfo GetSamplerBindingInfo(uint set, uint slot)
        {
            Debug.Assert(_setInfos != null, "EnsureResourcesCreated must be called before accessing resource set information.");
            SetBindingsInfo setInfo = _setInfos[set];
            return setInfo.GetSamplerBindingInfo(slot);
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyGLResources()
        {
            if (!_disposed)
            {
                _disposed = true;
                glDeleteProgram(_program);
                CheckLastError();
            }
        }
    }

    internal struct SetBindingsInfo
    {
        private readonly Dictionary<uint, OpenGLUniformBinding> _uniformBindings;
        private readonly Dictionary<uint, OpenGLTextureBindingSlotInfo> _textureBindings;
        private readonly Dictionary<uint, OpenGLSamplerBindingSlotInfo> _samplerBindings;

        public uint UniformBufferCount { get; }

        public SetBindingsInfo(
            Dictionary<uint, OpenGLUniformBinding> uniformBindings,
            Dictionary<uint, OpenGLTextureBindingSlotInfo> textureBindings,
            Dictionary<uint, OpenGLSamplerBindingSlotInfo> samplerBindings)
        {
            _uniformBindings = uniformBindings;
            UniformBufferCount = (uint)uniformBindings.Count;
            _textureBindings = textureBindings;
            _samplerBindings = samplerBindings;
        }

        public OpenGLTextureBindingSlotInfo GetTextureBindingInfo(uint slot)
        {
            if (!_textureBindings.TryGetValue(slot, out OpenGLTextureBindingSlotInfo binding))
            {
                throw new VeldridException("There is no texture in slot " + slot);
            }

            return binding;
        }

        public OpenGLSamplerBindingSlotInfo GetSamplerBindingInfo(uint slot)
        {
            if (!_samplerBindings.TryGetValue(slot, out OpenGLSamplerBindingSlotInfo binding))
            {
                throw new VeldridException("There is no sampler in slot " + slot);
            }

            return binding;
        }

        public OpenGLUniformBinding GetUniformBindingForSlot(uint slot)
        {
            if (!_uniformBindings.TryGetValue(slot, out OpenGLUniformBinding binding))
            {
                throw new VeldridException("There is no uniform buffer in slot " + slot);
            }

            return binding;
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

    internal class OpenGLUniformBinding
    {
        public uint Program { get; }
        public uint BlockLocation { get; }

        public OpenGLUniformBinding(uint program, uint blockLocation)
        {
            Program = program;
            BlockLocation = blockLocation;
        }
    }
}