using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ImageResizer.Uploader.Extensions
{
	public static class FileExtension
	{
		public static Task<byte[]> ReadFileAsBytesAsync(this string path)
		{
			if (!File.Exists(path))
				return Task.FromResult(Array.Empty<byte>());

			return File.ReadAllBytesAsync(path);
		}

		public static Task<long> GetFileLength(this string path)
		{
			if (!File.Exists(path))
				return Task.FromResult(default(long));

			FileInfo fi = new(path);
			return Task.FromResult(fi.Length);
		}

		public static Task WriteFileAsBytesAsync(this string path, byte[] msSteram)
		{
			return File.WriteAllBytesAsync(path, msSteram);
		}

		public static (int height, int width, long length) GetImageSize(this string path)
		{
			if (!File.Exists(path))
				return (0, 0, 0);

			using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var image = Image.FromStream(fileStream, false, false);
			return (height: image.Height, width: image.Width, length: fileStream.Length);
		}
	}
}
