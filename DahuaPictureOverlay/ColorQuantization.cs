using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DahuaPictureOverlay
{
	public static class ColorQuantization
	{
		/// <summary>
		/// <para>Implementation of Median Cut color quantization</para>
		/// </summary>
		public static List<Color> MedianCutRGBA(byte[] RGBA, int paletteSize)
		{
			List<Color> colors = new List<Color>(RGBA.Length / 4);
			for (int i = 0; i < RGBA.Length; i += 4)
			{
				colors.Add(new Color(RGBA[i], RGBA[i + 1], RGBA[i + 2], RGBA[i + 3]));
			}
			return MedianCut(colors, paletteSize);
		}
		/// <summary>
		/// <para>Implementation of Median Cut color quantization</para>
		/// </summary>
		public static List<Color> MedianCutBGRA(byte[] RGBA, int paletteSize)
		{
			List<Color> colors = new List<Color>(RGBA.Length / 4);
			for (int i = 0; i < RGBA.Length; i += 4)
			{
				colors.Add(new Color(RGBA[i + 2], RGBA[i + 1], RGBA[i], RGBA[i + 3]));
			}
			return MedianCut(colors, paletteSize);
		}
		/// <summary>
		/// <para>Implementation of Median Cut color quantization</para>
		/// <para>Suppose we have an image with an arbitrary number of pixels and want to generate a palette of 16 colors.  Put all the pixels of the image (that is, their RGB values) in a bucket.  Find out which color channel (red, green, or blue) among the pixels in the bucket has the greatest range, then sort the pixels according to that channel's values. For example, if the blue channel has the greatest range, then a pixel with an RGB value of (32, 8, 16) is less than a pixel with an RGB value of (1, 2, 24), because 16 < 24. After the bucket has been sorted, move the upper half of the pixels into a new bucket. (It is this step that gives the median cut algorithm its name; the buckets are divided into two at the median of the list of pixels.) Repeat the process on both buckets, giving you 4 buckets, then repeat on all 4 buckets, giving you 8 buckets, then repeat on all 8, giving you 16 buckets. Average the pixels in each bucket and you have a palette of 16 colors.</para>

		/// <para>Since the number of buckets doubles with each iteration, this algorithm can only generate a palette with a number of colors that is a power of two. To generate, say, a 12-color palette, one might first generate a 16-color palette and merge some of the colors in some way.</para>
		/// </summary>
		public static List<Color> MedianCut(List<Color> inputColors, int paletteSize)
		{
			List<List<Color>> outputBuckets = new List<List<Color>>();
			outputBuckets.Add(inputColors);
			while (paletteSize > 1)
			{
				List<List<Color>> inputBuckets = new List<List<Color>>(outputBuckets);
				outputBuckets.Clear();
				paletteSize = paletteSize / 2;
				for (int i = inputBuckets.Count - 1; i >= 0; i--)
				{
					List<Color> bucket = inputBuckets[i];
					Color lowest = new Color(255, 255, 255, 255);
					Color highest = new Color(0, 0, 0, 0);
					foreach (Color c in bucket)
					{
						if (c.R < lowest.R)
							lowest.R = c.R;
						if (c.G < lowest.G)
							lowest.G = c.G;
						if (c.B < lowest.B)
							lowest.B = c.B;
						if (c.A < lowest.A)
							lowest.A = c.A;
						if (c.R > highest.R)
							highest.R = c.R;
						if (c.G > highest.G)
							highest.G = c.G;
						if (c.B > highest.B)
							highest.B = c.B;
						if (c.A > highest.A)
							highest.A = c.A;
					}
					Color difference = new Color(highest.R - lowest.R, highest.G - lowest.G, highest.B - lowest.B, highest.A - lowest.A);
					int channel = 0;
					int channelRange = difference.R;
					if (difference.G > channelRange)
					{
						channel = 1;
						channelRange = difference.G;
					}
					if (difference.B > channelRange)
					{
						channel = 2;
						channelRange = difference.B;
					}
					if (difference.A > channelRange)
					{
						channel = 3;
						channelRange = difference.A;
					}
					bucket.Sort((a, b) =>
					{
						return a.Channel(channel).CompareTo(b.Channel(channel));
					});
					if (bucket.Count >= 2)
					{
						outputBuckets.Add(bucket.Take(bucket.Count / 2).ToList());
						outputBuckets.Add(bucket.Skip(bucket.Count / 2).ToList());
					}
					else
					{
						outputBuckets.Add(bucket);
						outputBuckets.Add(bucket);
					}
				}
			}
			return outputBuckets
				.Select(bucket =>
				{
					long R = 0, G = 0, B = 0, A = 0;
					foreach (Color c in bucket)
					{
						R += c.R;
						G += c.G;
						B += c.B;
						A += c.A;
					}
					return new Color((int)Math.Round(R / (double)bucket.Count)
						, (int)Math.Round(G / (double)bucket.Count)
						, (int)Math.Round(B / (double)bucket.Count)
						, (int)Math.Round(A / (double)bucket.Count));
				})
				.OrderBy(a => a)
				.ToList();
		}

		public static int GetBestColorIndex(Color c, List<Color> palette)
		{
			double[] distances = palette
				.Select(paletteColor =>
				{
					return Math.Sqrt(
						Math.Pow(paletteColor.R - c.R, 2)
						+ Math.Pow(paletteColor.G - c.G, 2)
						+ Math.Pow(paletteColor.B - c.B, 2)
						+ Math.Pow(paletteColor.A - c.A, 2)
						);
				})
				.ToArray();
			double lowest = distances[0];
			int idx = 0;
			for (int i = 1; i < distances.Length; i++)
			{
				if (distances[i] < lowest)
				{
					lowest = distances[i];
					idx = i;
				}
			}
			return idx;
		}
	}
}
