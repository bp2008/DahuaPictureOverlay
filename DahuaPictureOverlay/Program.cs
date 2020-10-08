using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DahuaPictureOverlay
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("DahuaPictureOverlay " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
			if (args.Length == 1)
			{
				try
				{
					byte[] outData;
					using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(args[0])))
					using (Image bmp = Image.FromStream(ms))
					{
						double aspect = bmp.Width / (double)bmp.Height;
						int w = 128;
						int h = 128;
						if (aspect > 1)
						{
							h = (int)Math.Round(w / aspect);
							while (w * h > 15300)
							{
								w -= 1;
								h = (int)Math.Round(w / aspect);
							}
						}
						else
						{
							w = (int)Math.Round(h * aspect);
							while (w * h > 15300)
							{
								h -= 1;
								w = (int)Math.Round(h * aspect);
							}
						}
						using (Bitmap thumb = (Bitmap)bmp.GetThumbnailImage(w, h, () => false, IntPtr.Zero))
						{
							byte[] rgba = new byte[thumb.Width * thumb.Height * 4];
							BitmapData bmpData = thumb.LockBits(new Rectangle(0, 0, thumb.Width, thumb.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
							Marshal.Copy(bmpData.Scan0, rgba, 0, rgba.Length);
							thumb.UnlockBits(bmpData);
							outData = BmpFormat.WriteBMPFromBGRA((uint)thumb.Width, (uint)thumb.Height, rgba);
						}
					}
					if (File.Exists("out.bmp"))
					{
						string response;
						do
						{
							Console.WriteLine("File \"out.bmp\" already exists.  Overwrite? (y/n)");
							response = Console.ReadLine();
						}
						while (response.ToLower() != "y" && response.ToLower() != "n");
						if (response.ToLower() != "y")
							return;
					}
					File.WriteAllBytes("out.bmp", outData);
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("An error has occurred!");
					Console.WriteLine();
					Console.WriteLine(ex.ToString());
					Console.ResetColor();
					Console.WriteLine();
					Console.WriteLine("Press ENTER to exit");
					Console.ReadLine();
				}
			}
			else
				Console.WriteLine("Please drag an image onto this program.");
		}
	}
	public static class MemoryStreamExtensions
	{
		public static MemoryStream UInt16LE(this MemoryStream s, ushort value)
		{
			byte[] buf = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buf);
			s.Write(buf, 0, buf.Length);
			return s;
		}
		public static MemoryStream Int16LE(this MemoryStream s, short value)
		{
			byte[] buf = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buf);
			s.Write(buf, 0, buf.Length);
			return s;
		}
		public static MemoryStream UInt32LE(this MemoryStream s, uint value)
		{
			byte[] buf = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buf);
			s.Write(buf, 0, buf.Length);
			return s;
		}
		public static MemoryStream Int32LE(this MemoryStream s, int value)
		{
			byte[] buf = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buf);
			s.Write(buf, 0, buf.Length);
			return s;
		}
	}
}
