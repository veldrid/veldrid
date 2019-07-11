//using SixLabors.ImageSharp.PixelFormats;
//using System.Numerics;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
//using SixLabors.Fonts;
//using System.Linq;
//using Veldrid;
//using SixLabors.ImageSharp.Advanced;
//using System;
//using SixLabors.Primitives;
//using System.Runtime.CompilerServices;
//using System.IO;
//using System.Runtime.InteropServices;

//namespace Snake
//{
//    public class TextRenderer
//    {
//        private readonly GraphicsDevice _gd;
//        private readonly Texture _texture;

//        public TextureView TextureView { get; }

//        private readonly Font _font;
//        private readonly Image<Rgba32> _image;

//        public TextRenderer(GraphicsDevice gd)
//        {
//            _gd = gd;
//            uint width = 250;
//            uint height = 100;
//            _texture = gd.ResourceFactory.CreateTexture(
//                TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
//            TextureView = gd.ResourceFactory.CreateTextureView(_texture);

//            FontCollection fc = new FontCollection();
//            FontFamily family = fc.Install(Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "Sunflower-Medium.ttf"));
//            _font = family.CreateFont(28);
                
//            _image = new Image<Rgba32>(250, 100);
//        }

//        public unsafe void DrawText(string text)
//        {
//            fixed (void* data = &MemoryMarshal.GetReference(_image.GetPixelSpan()))
//            {
//                Unsafe.InitBlock(data, 0, (uint)(_image.Width * _image.Height * 4));
//            }

//            _image.Mutate(ctx =>
//            {
//                ctx.DrawText(
//                    new TextGraphicsOptions
//                    {
//                        WrapTextWidth = _image.Width,
//                        Antialias = true,
//                        HorizontalAlignment = HorizontalAlignment.Center
//                    },
//                    text,
//                    _font,
//                    Rgba32.White,
//                    new PointF());
//            });

//            fixed (void* data = &MemoryMarshal.GetReference(_image.GetPixelSpan()))
//            {
//                uint size = (uint)(_image.Width * _image.Height * 4);
//                _gd.UpdateTexture(_texture, (IntPtr)data, size, 0, 0, 0, _texture.Width, _texture.Height, 1, 0, 0);
//            }
//        }
//    }
//}
