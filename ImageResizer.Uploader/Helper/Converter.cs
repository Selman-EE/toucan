using System;

namespace ImageResizer.Uploader.Helper
{
	public static class Converter
	{
		public static string ToSize(this long value, SizeUnits unit)
		{
			return (value / (double)Math.Pow(1024, (long)unit)).ToString("0.00");
		}
	}

	public enum SizeUnits
	{
		Byte, KB, MB, GB, TB, PB, EB, ZB, YB
	}


}
