using System;

namespace DahuaPictureOverlay
{
	public class Color : IComparable, IComparable<Color>
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;
		public byte Channel(int index)
		{
			switch (index)
			{
				case 0:
					return R;
				case 1:
					return G;
				case 2:
					return B;
				case 3:
					return A;
				default:
					throw new IndexOutOfRangeException();
			}
		}

		public Color() { }
		public Color(byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public Color(int r, int g, int b, int a)
		{
			R = (byte)r;
			G = (byte)g;
			B = (byte)b;
			A = (byte)a;
		}

		public int CompareTo(object obj)
		{
			if (obj is Color)
				return CompareWith((Color)obj);
			return -1;
		}

		public int CompareTo(Color other)
		{
			return CompareWith((Color)other);
		}
		public int CompareWith(Color other)
		{
			int diff = R.CompareTo(other.R);
			if (diff == 0)
				diff = G.CompareTo(other.G);
			if (diff == 0)
				diff = B.CompareTo(other.B);
			if (diff == 0)
				diff = A.CompareTo(other.A);
			return diff;
		}

		public override bool Equals(object obj)
		{
			return CompareTo(obj) == 0;
		}
		public override int GetHashCode()
		{
			return R | G << 8 | B << 16 | A << 24;
		}
		public override string ToString()
		{
			return R + ", " + G + ", " + B + ", " + A;
		}
	}
}
