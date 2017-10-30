using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLTextureCube : TextureCube, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _texture;

        private string _name;
        private bool _nameChanged;
        public string Name { get => _name; set { _name = value; _nameChanged = true; } }

        public uint Texture => _texture;
        public override uint Width { get; }
        public override uint Height { get; }
        public override PixelFormat Format { get; }
        public override uint MipLevels { get; }
        public override uint ArrayLayers { get; }
        public override TextureUsage Usage { get; }
        public GLPixelFormat GLPixelFormat { get; }
        public GLPixelType GLPixelType { get; }
        public PixelInternalFormat GLInternalFormat { get; }

        public OpenGLTextureCube(OpenGLGraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;

            Width = description.Width;
            Height = description.Height;
            Format = description.Format;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Usage = description.Usage;

            GLPixelFormat = OpenGLFormats.VdToGLPixelFormat(Format);
            GLPixelType = OpenGLFormats.VdToGLPixelType(Format);
            GLInternalFormat = OpenGLFormats.VdToGLPixelInternalFormat(Format);
        }

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
            glGenTextures(1, out _texture);
            CheckLastError();

            glBindTexture(TextureTarget.TextureCubeMap, _texture);
            CheckLastError();

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