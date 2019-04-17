namespace StbSharp
{
	public class Image
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public int SourceComp { get; set; }
		public int Comp { get; set; }
		public byte[] Data { get; set; }

		public unsafe Image CreateResized(int newWidth, int newHeight)
		{
			var result = new Image
			{
				Comp = Comp,
				SourceComp = SourceComp,
				Data = new byte[newWidth*newHeight*Comp],
				Width = newWidth,
				Height = newHeight
			};

			fixed (byte* input = Data)
			{
				fixed (byte* output = result.Data)
				{
					StbImageResize.stbir_resize_uint8(input, Width, Height, Width*Comp, output, newWidth, newHeight, newWidth*Comp,
						Comp);
				}
			}

			return result;
		}
	}
}