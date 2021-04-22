using ImageResizer.Uploader.Helper;

namespace ImageResizer.Uploader.Models
{
	public class FileUploadResult
	{
		public bool Uploaded { get; init; }
		public string FileName { get; init; }
		public string NewFileName { get; init; }
		public string OriginalFullPath { get; init; }
		public int Width { get; init; }
		public int Height { get; init; }
		public long Length { get; init; }
		public string FileSize => Length > 0 ? $"{Length.ToSize(SizeUnits.MB)} {SizeUnits.MB}" : $"You must set to file {nameof(Length).ToLower()} first";

		public string ResizedFullPath { get; init; }
		public int ResizedWidth { get; set; }
		public int ResizedHeight { get; set; }
		public long ResizedLength { get; set; }
		public string ResizedFileSize => ResizedLength > 0
					? $"{ResizedLength.ToSize(SizeUnits.MB)} {SizeUnits.MB}"
					: $"You must set to file {nameof(ResizedLength).ToLower()} first";

	}
}
