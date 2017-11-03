using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLTexture : Texture, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _texture;

        private string _name;
        private bool _nameChanged;
        public string Name { get => _name; set { _name = value; _nameChanged = true; } }

        public uint Texture => _texture;

        public OpenGLTexture(OpenGLGraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;

            Width = description.Width;
            Height = description.Height;
            Depth = description.Depth;
            Format = description.Format;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Usage = description.Usage;

            GLPixelFormat = OpenGLFormats.VdToGLPixelFormat(Format);
            GLPixelType = OpenGLFormats.VdToGLPixelType(Format);
            GLInternalFormat = OpenGLFormats.VdToGLPixelInternalFormat(Format);

            if ((Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
            {
                GLPixelFormat = GLPixelFormat.DepthComponent;
                if (Format == PixelFormat.R16_UNorm)
                {
                    GLInternalFormat = PixelInternalFormat.DepthComponent16;
                }
                else if (Format == PixelFormat.R32_Float)
                {
                    GLInternalFormat = PixelInternalFormat.DepthComponent32f;
                }
            }

            if ((Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                TextureTarget = ArrayLayers == 1 ? TextureTarget.TextureCubeMap : TextureTarget.TextureCubeMapArray;
            }
            else if (Depth == 1)
            {
                TextureTarget = ArrayLayers == 1 ? TextureTarget.Texture2D : TextureTarget.Texture2DArray;
            }
            else
            {
                TextureTarget = TextureTarget.Texture3D;
            }
        }

        public override uint Width { get; }

        public override uint Height { get; }

        public override uint Depth { get; }

        public override PixelFormat Format { get; }

        public override uint MipLevels { get; }

        public override uint ArrayLayers { get; }

        public override TextureUsage Usage { get; }
        public GLPixelFormat GLPixelFormat { get; }
        public GLPixelType GLPixelType { get; }
        public PixelInternalFormat GLInternalFormat { get; }
        public TextureTarget TextureTarget { get; }

        public bool Created { get; private set; }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources();
            }
            if (_nameChanged)
            {
                _nameChanged = false;
                SetObjectLabel(ObjectLabelIdentifier.Texture, _texture, _name);
            }
        }

        private void CreateGLResources()
        {
            if (Depth != 1)
            {
                throw new NotImplementedException(); // TODO: Implement 3D textures.
            }

            glGenTextures(1, out _texture);
            CheckLastError();

            glBindTexture(TextureTarget, _texture);
            CheckLastError();

            // TODO: Remove this -- it is a hack working around improper sampler support.
            TextureMinFilter minFilter = MipLevels > 1 ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
            glTexParameteri(TextureTarget, TextureParameterName.TextureMinFilter, (int)minFilter);
            glTexParameteri(TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            if (TextureTarget == TextureTarget.Texture2D)
            {
                uint levelWidth = Width;
                uint levelHeight = Height;
                for (int currentLevel = 0; currentLevel < MipLevels; currentLevel++)
                {
                    // Set size, load empty data into texture
                    glTexImage2D(
                        TextureTarget.Texture2D,
                        currentLevel,
                        GLInternalFormat,
                        levelWidth,
                        levelHeight,
                        0, // border
                        GLPixelFormat,
                        GLPixelType,
                        null);
                    CheckLastError();

                    levelWidth = Math.Max(1, levelWidth / 2);
                    levelHeight = Math.Max(1, levelHeight / 2);
                }
            }
            else if (TextureTarget == TextureTarget.TextureCubeMap)
            {
                uint levelWidth = Width;
                uint levelHeight = Height;
                for (int currentLevel = 0; currentLevel < MipLevels; currentLevel++)
                {
                    for (int face = 0; face < 6; face++)
                    {
                        // Set size, load empty data into texture
                        glTexImage2D(
                            TextureTarget.TextureCubeMapPositiveX + face,
                            currentLevel,
                            GLInternalFormat,
                            levelWidth,
                            levelHeight,
                            0, // border
                            GLPixelFormat,
                            GLPixelType,
                            null);
                        CheckLastError();
                    }

                    levelWidth = Math.Max(1, levelWidth / 2);
                    levelHeight = Math.Max(1, levelHeight / 2);
                }
            }
            else
            {
                throw new NotImplementedException(); // TODO
            }

            Created = true;
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyGLResources()
        {
            glDeleteTextures(1, ref _texture);
            CheckLastError();
        }
    }
}