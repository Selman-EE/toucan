using PhotoSauce.MagicScaler;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageResizer.Uploader.Extensions
{
	public static class ImageResizerFactory
	{
		// todo: will be parameters width, height , size 
		const int _size = 600;
		const long _quality = 100;

		//https://devblogs.microsoft.com/dotnet/net-core-image-processing/#corecompat-system-drawing
		public static (int height, int width, long length) DrawingBitmap(string inputPath, string outputPath)
		{
			if (!File.Exists(inputPath))
				return (0, 0, 0);

			using var fileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var image = new Bitmap(Image.FromStream(fileStream, false, false));
			CalculateImageWidthAndHeight(image, out int width, out int height);

			try
			{
				var resized = new Bitmap(width, height);
				using (var graphics = Graphics.FromImage(resized))
				{
					graphics.CompositingQuality = CompositingQuality.HighSpeed;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.DrawImage(image, 0, 0, width, height);
					using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						var encoderParameters = new EncoderParameters(1);
						encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, _quality);
						var codec = ImageCodecInfo
							.GetImageDecoders()
							.Where(codec => codec.FormatID == ImageFormat.Jpeg.Guid)
							.FirstOrDefault();
						resized.Save(output, codec, encoderParameters);
						return (resized.Height, resized.Width, output.Length);
					}
				}
			}
			catch
			{
				return (0, 0, 0);
			}


		}
		public static long DrawingBitmap(string inputPath, string outputPath, int height, int width)
		{
			if (!File.Exists(inputPath))
				return 0;

			using var fileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var image = new Bitmap(Image.FromStream(fileStream, false, false));

			CalculateImageSize(ref height, ref width, image);

			try
			{
				var resized = new Bitmap(width, height);
				using (var graphics = Graphics.FromImage(resized))
				{
					graphics.CompositingQuality = CompositingQuality.HighSpeed;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.DrawImage(image, 0, 0, width, height);
					using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						var encoderParameters = new EncoderParameters(1);
						encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, _quality);
						var codec = ImageCodecInfo
							.GetImageDecoders()
							.Where(codec => codec.FormatID == ImageFormat.Jpeg.Guid)
							.FirstOrDefault();
						resized.Save(output, codec, encoderParameters);
						return output.Length;
					}
				}
			}
			catch
			{
				return 0;
			}


		}

		private static void CalculateImageSize(ref int height, ref int width, Bitmap image)
		{
			if (image.Width > image.Height)
			{
				height = Convert.ToInt32(image.Height * height / (double)image.Width);
			}
			else
			{
				width = Convert.ToInt32(image.Width * width / (double)image.Height);
			}
		}

		private static void CalculateImageWidthAndHeight(Bitmap image, out int width, out int height)
		{
			if (image.Width > image.Height)
			{
				width = _size;
				height = Convert.ToInt32(image.Height * _size / (double)image.Width);
			}
			else
			{
				width = Convert.ToInt32(image.Width * _size / (double)image.Height);
				height = _size;
			}
		}

		//https://devblogs.microsoft.com/dotnet/net-core-image-processing/#magicscaler		
		public static (int height, int width, long length) MagicScaler(string inputPath, string outputPath)
		{
			if (!File.Exists(inputPath))
				return (0, 0, 0);

			using var fileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var image = new Bitmap(Image.FromStream(fileStream, false, false));
			CalculateImageWidthAndHeight(image, out int width, out int height);

			var settings = new ProcessImageSettings()
			{
				Width = width,
				Height = height,
				ResizeMode = CropScaleMode.Max,
				SaveFormat = FileFormat.Jpeg,
				JpegQuality = 100,
				JpegSubsampleMode = ChromaSubsampleMode.Subsample420
			};

			try
			{
				using var output = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				_ = MagicImageProcessor.ProcessImage(inputPath, output, settings);
				return (height, width, output.Length);
			}
			catch
			{
				return (0, 0, 0);
			}
		}

		//Magick.NET : Cross-platform support winner
		//ImageSharp		
		//SkiaSharp
		//FreeImage
	}
}
