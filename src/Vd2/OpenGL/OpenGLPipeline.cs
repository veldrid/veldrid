using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using Vd2.OpenGLBinding;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Vd2.OpenGL
{
    internal unsafe class OpenGLPipeline : Pipeline, OpenGLDeferredResource
    {
        private const uint GL_INVALID_INDEX = 0xFFFFFFFF;
        private readonly OpenGLGraphicsDevice _gd;
        private uint _program;
        private readonly Dictionary<uint, OpenGLTextureBindingSlotInfo> _textureBindings = new Dictionary<uint, OpenGLTextureBindingSlotInfo>();
        private readonly Dictionary<uint, OpenGLTextureBindingSlotInfo> _samplerBindings = new Dictionary<uint, OpenGLTextureBindingSlotInfo>();
        private readonly Dictionary<uint, OpenGLUniformBinding> _constantBindings = new Dictionary<uint, OpenGLUniformBinding>();

        public PipelineDescription Description { get; }

        public uint Program => _program;

        public OpenGLPipeline(OpenGLGraphicsDevice gd, ref PipelineDescription description)
        {
            _gd = gd;
            Description = description;
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
                        throw new VdException("There was no attribute variable with the name " + layoutDesc.Elements[i].Name);
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
                throw new VdException($"Error linking GL program: {log}");
            }

            OpenGLResourceLayout glResourceLayout
                = Util.AssertSubtype<ResourceLayout, OpenGLResourceLayout>(Description.ResourceLayout);
            ResourceLayoutElementDescription[] resources = glResourceLayout.Description.Elements;
            int lastTextureLocation = -1;
            int relativeTextureIndex = -1;
            for (uint i = 0; i < resources.Length; i++)
            {
                ResourceLayoutElementDescription resource = resources[i];
                if (resource.Kind == ResourceKind.Uniform)
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
                        _constantBindings[i] = new OpenGLUniformBinding(_program, blockIndex);
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                        //int uniformLocation = glGetUniformLocation(_program, resource.Name);

                        //OpenGLUniformStorageAdapter storageAdapter = new OpenGLUniformStorageAdapter(_program, uniformLocation);
                        //_constantBindings[i] = new OpenGLUniformBinding(_program, storageAdapter);
                    }
                }
                else if (resource.Kind == ResourceKind.Texture)
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
                    _textureBindings[i] = new OpenGLTextureBindingSlotInfo() { RelativeIndex = relativeTextureIndex, UniformLocation = location };
                    lastTextureLocation = location;
                }
                else
                {
                    Debug.Assert(resource.Kind == ResourceKind.Sampler);

                    // TODO: Samplers should be able to bind to multiple texture slots
                    // if multiple textures are declared without an intervening sampler. For example:
                    //     Slot    Resource
                    // -------------------------
                    //     [0]     Texture0
                    //     [1]     Sampler0
                    //     [2]     Texture1
                    //     [3]     Texture2
                    //     [4]     Sampler1*
                    // Sampler1 should be active for both Texture1 and Texture2.

                    _samplerBindings[i] = new OpenGLTextureBindingSlotInfo() { RelativeIndex = relativeTextureIndex, UniformLocation = lastTextureLocation };
                }
            }

            Created = true;
        }

        public OpenGLTextureBindingSlotInfo GetTextureBindingInfo(uint slot)
        {
            if (!_textureBindings.TryGetValue(slot, out OpenGLTextureBindingSlotInfo binding))
            {
                throw new VdException("There is no texture in slot " + slot);
            }

            return binding;
        }

        public OpenGLTextureBindingSlotInfo GetSamplerBindingInfo(uint slot)
        {
            if (!_samplerBindings.TryGetValue(slot, out OpenGLTextureBindingSlotInfo binding))
            {
                throw new VdException("There is no sampler in slot " + slot);
            }

            return binding;
        }

        public OpenGLUniformBinding GetUniformBindingForSlot(uint slot)
        {
            if (!_constantBindings.TryGetValue(slot, out OpenGLUniformBinding binding))
            {
                throw new VdException("There is no constant buffer in slot " + slot);
            }

            return binding;
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyGLResources()
        {
            glDeleteProgram(_program);
            CheckLastError();
        }
    }

    public struct OpenGLTextureBindingSlotInfo
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

    public class OpenGLUniformBinding
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