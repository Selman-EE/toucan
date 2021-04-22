using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ImageResizer.Uploader.Models
{
	public class FileUploadModel
	{
		public IReadOnlyList<IFormFile> Files { get; set; }
		public int Witdh { get; set; }
		public int Height { get; set; }

	}
}
