using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DahuaPictureOverlay
{
	public static class BmpFormat
	{
		public static byte[] WriteBMPFromRGBA(uint width, uint height, byte[] rgba, BmpHeaderType headerType = BmpHeaderType.BITMAPINFOHEADER)
		{
			return WriteBMP(width, height, rgba, headerType, ColorQuantization.MedianCutRGBA, MapRGBAColorToPaletteMethod);
		}

		public static byte[] WriteBMPFromBGRA(uint width, uint height, byte[] rgba, BmpHeaderType headerType = BmpHeaderType.BITMAPINFOHEADER)
		{
			return WriteBMP(width, height, rgba, headerType, ColorQuantization.MedianCutBGRA, MapBGRAColorToPaletteMethod);
		}
		private static byte[] WriteBMP(uint width, uint height, byte[] raw, BmpHeaderType headerType, Func<byte[], int, List<Color>> QuantizationMethod, Action<byte[], int, int, int, Color> MapColorToPaletteMethod)
		{

			using (MemoryStream ms = new MemoryStream())
			{
				////////////////////////
				// Bitmap file header //
				////////////////////////
				ms.WriteByte((byte)'B'); // Magic value identifying bitmap file
				ms.WriteByte((byte)'M'); // Magic value identifying bitmap file
				ms.Write(new byte[] { 0, 0, 0, 0 }, 0, 4); // Bitmap Size (little-endian) to be filled in later at offset 2
				ms.Write(new byte[] { 0, 0 }, 0, 2); // Reserved
				ms.Write(new byte[] { 0, 0 }, 0, 2); // Reserved
				ms.Write(new byte[] { 0, 0, 0, 0 }, 0, 4); // Offset of pixel array start (little-endian) to be filled in later at offset 10

				////////////////
				// DIB header //
				////////////////

				if (headerType == BmpHeaderType.BITMAPCOREHEADER)
				{
					// BITMAPCOREHEADER
					if (width > ushort.MaxValue)
						throw new ArgumentException("Width " + width + " is too large. Max: " + ushort.MaxValue);
					if (height > ushort.MaxValue)
						throw new ArgumentException("Height " + height + " is too large. Max: " + ushort.MaxValue);
					ms.UInt32LE(12); // Size of this header
					ms.UInt16LE((ushort)width); // Width in pixels
					ms.UInt16LE((ushort)height); // Height in pixels
					ms.UInt16LE(1); // Number of planes (must be 1)
					ms.UInt16LE(8); // Number of bits per pixel
				}
				else if (headerType == BmpHeaderType.BITMAPINFOHEADER)
				{
					// BITMAPINFOHEADER
					ms.UInt32LE(40); // Size of this header
					ms.UInt32LE(width); // Width in pixels
					ms.UInt32LE(height); // Height in pixels
					ms.UInt16LE(1); // Number of planes (must be 1)
					ms.UInt16LE(8); // Number of bits per pixel
					ms.UInt32LE(0); // bV4V4Compression
					ms.UInt32LE(width * height); // bV4SizeImage
					ms.UInt32LE(3780); // bV4XPelsPerMeter (pixels per meter width)
					ms.UInt32LE(3780); // bV4YPelsPerMeter (pixels per meter height)
					ms.UInt32LE(256); // bV4ClrUsed (number of colors used in the color table)
					ms.UInt32LE(256); // bV4ClrImportant (number of colors required for displaying the bitmap)
				}
				else if (headerType == BmpHeaderType.BITMAPV4HEADER)
				{
					// BITMAPV4HEADER
					ms.UInt32LE(108); // Size of this header
					ms.UInt32LE(width); // Width in pixels
					ms.UInt32LE(height); // Height in pixels
					ms.UInt16LE(1); // Number of planes (must be 1)
					ms.UInt16LE(8); // Number of bits per pixel
					ms.UInt32LE(0); // bV4V4Compression
					ms.UInt32LE(width * height); // bV4SizeImage
					ms.UInt32LE(3780); // bV4XPelsPerMeter (pixels per meter width)
					ms.UInt32LE(3780); // bV4YPelsPerMeter (pixels per meter height)
					ms.UInt32LE(256); // bV4ClrUsed (number of colors used in the color table)
					ms.UInt32LE(256); // bV4ClrImportant (number of colors required for displaying the bitmap)
					ms.UInt32LE(0); // bV4RedMask
					ms.UInt32LE(0); // bV4GreenMask
					ms.UInt32LE(0); // bV4BlueMask
					ms.UInt32LE(0); // bV4AlphaMask
					ms.UInt32LE(0x73524742); // bV4CSType (0x42475273 = "BGRs")
					ms.UInt32LE(0);         // X coordinate of red endpoint
					ms.UInt32LE(0);         // Y coordinate of red endpoint
					ms.UInt32LE(0);         // Z coordinate of red endpoint
					ms.UInt32LE(0);     // X coordinate of green endpoint
					ms.UInt32LE(0);        // Y coordinate of green endpoint
					ms.UInt32LE(0);       // Z coordinate of green endpoint
					ms.UInt32LE(0);        // X coordinate of blue endpoint
					ms.UInt32LE(0);         // Y coordinate of blue endpoint
					ms.UInt32LE(0);        // Z coordinate of blue endpoint
					ms.UInt32LE(0); // bV4GammaRed
					ms.UInt32LE(0); // bV4GammaGreen
					ms.UInt32LE(0); // bV4GammaBlue
				}


				/////////////////
				// Color Table //
				/////////////////
				List<Color> palette = QuantizationMethod(raw, 256);
				for (int i = 0; i < palette.Count; i++)
				{
					ms.Write(RGBA(palette[i].R, palette[i].G, palette[i].B, (byte)(255 - palette[i].A)), 0, 4);
					//if (i < 50)
					//{
					//	ms.Write(RGBA(255, 255, 255, 0), 0, 4);
					//}
					//else if (i < 100)
					//{
					//	ms.Write(RGBA(255, 0, 0, 0), 0, 4);
					//}
					//else if (i < 150)
					//{
					//	ms.Write(RGBA(0, 255, 0, 0), 0, 4);
					//}
					//else if (i < 200)
					//{
					//	ms.Write(RGBA(0, 0, 255, 0), 0, 4);
					//}
					//else
					//{
					//	ms.Write(RGBA(255, 0, 0, 128), 0, 4);
					//}
				}

				/////////////////
				// Pixel Array //
				/////////////////

				long rem;

				//{
				//	// Add padding if necessary to make the pixel array start at a 4-byte boundary.
				//	// This is not strictly necessary but it may help with buggy BMP decoders, and it costs at most 3 bytes of overhead.

				//	rem = 4 - ((int)ms.Length % 4);
				//	if (rem < 4)
				//	{
				//		for (int i = 0; i < rem; i++)
				//		{
				//			ms.WriteByte(0);
				//		}
				//	}
				//}

				uint pixArrayStart = (uint)ms.Length;
				// Pixels are written left to right in rows, bottom to top.
				int stride = (int)width * 4;
				//if (stride % 4 != 0)
				//	stride += (4 - stride % 4);
				Color c = new Color();
				for (int y = (int)height - 1; y >= 0; y--)
				{
					for (int x = 0; x < width; x++)
					{
						MapColorToPaletteMethod(raw, x, y, stride, c);
						ms.WriteByte((byte)ColorQuantization.GetBestColorIndex(c, palette));
					}
					// Add padding if necessary to make each row a multiple of 4 bytes.
					rem = 4 - (width % 4);
					if (rem < 4)
					{
						for (int i = 0; i < rem; i++)
							ms.WriteByte(0);
					}
				}

				// Fill in calculated values
				ms.Seek(2, SeekOrigin.Begin);
				ms.UInt32LE((uint)ms.Length);

				ms.Seek(10, SeekOrigin.Begin);
				ms.UInt32LE((uint)pixArrayStart);

				return ms.ToArray();
			}
		}
		static byte[] color(int i)
		{
			int rem = i % 4;
			byte R = (byte)((rem / 3.0) * 255);
			i /= 4;
			rem = i % 4;
			byte G = (byte)((rem / 3.0) * 255);
			i /= 4;
			rem = i % 4;
			byte B = (byte)((rem / 3.0) * 255);
			i /= 4;
			rem = i % 4;
			byte A = (byte)((rem / 3.0) * 255);
			return RGBA(R, G, B, A);
		}
		/// <summary>
		/// Returns BGR bytes for the specified RGB color.
		/// </summary>
		/// <param name="R">red channel</param>
		/// <param name="G">green channel</param>
		/// <param name="B">blue channel</param>
		/// <returns></returns>
		static byte[] RGB(byte R, byte G, byte B)
		{
			return new byte[] { B, G, R };
		}
		/// <summary>
		/// Returns BGR bytes for the specified RGB color.
		/// </summary>
		/// <param name="R">red channel</param>
		/// <param name="G">green channel</param>
		/// <param name="B">blue channel</param>
		/// <returns></returns>
		static byte[] RGBA(byte R, byte G, byte B, byte A)
		{
			return new byte[] { B, G, R, A };
		}

		internal static void MapRGBAColorToPaletteMethod(byte[] raw, int x, int y, int stride, Color c)
		{
			int i = (x * 4) + (y * stride);
			c.R = raw[i];
			c.G = raw[i + 1];
			c.B = raw[i + 2];
			c.A = raw[i + 3];
		}

		internal static void MapBGRAColorToPaletteMethod(byte[] raw, int x, int y, int stride, Color c)
		{
			int i = (x * 4) + (y * stride);
			c.B = raw[i];
			c.G = raw[i + 1];
			c.R = raw[i + 2];
			c.A = raw[i + 3];
		}
	}
	public enum BmpHeaderType
	{
		BITMAPCOREHEADER,
		BITMAPINFOHEADER,
		BITMAPV4HEADER
	}
}
