using ImageResizer.Uploader.Extensions;
using ImageResizer.Uploader.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizer.Uploader.Controllers
{
	//https://devblogs.microsoft.com/dotnet/net-core-image-processing/#sample-code
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IDatetimeServiceProvider _datetimeService;
		private readonly IWebHostEnvironment _env;
		private const long _maxFileSize = 1024 * 1024 * 128;
		private const long _multipartBodyLengthLimit = 1024 * 1024 * 128;
		public HomeController(ILogger<HomeController> logger, IDatetimeServiceProvider datetimeService, IWebHostEnvironment env)
		{
			_logger = logger;
			_datetimeService = datetimeService;
			_env = env;
		}

		public IActionResult Index()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequestFormLimits(MultipartBodyLengthLimit = _multipartBodyLengthLimit)]// Set the limit to 128 MB
		[RequestSizeLimit(_multipartBodyLengthLimit * 2)]// Set the limit to 256 MB
		public async Task<IActionResult> Upload([FromForm] FileUploadModel model)
		{
			if (model.Files is null || model.Files.Any() == false)
				return NotFound();

			if (!IsMultipartContentType(Request.ContentType))
			{
				ModelState.AddModelError("File", $"The request couldn't be processed (Error 1).");
				// todo: Log error
				return BadRequest(ModelState);
			}

			var result = await SaveFiles(model);
			return Ok(result);
		}


		internal async Task<List<FileUploadResult>> SaveFiles(FileUploadModel model)
		{
			var list = new List<FileUploadResult>();
			foreach (var file in model.Files)
			{
				if (file.Length > _maxFileSize)
					continue;

				if (!CheckHasValidFileType(file.FileName))
					continue;

				// todo: other conditions
				list.Add(await SaveFile(file, _datetimeService, _env));
			}

			foreach (var item in list)
			{
				// its working very fast - high performance
				//(int height, int width, long length) = ImageResizerFactory.DrawingBitmap(item.OriginalFullPath, item.ResizedFullPath);

				var fileLength = ImageResizerFactory.DrawingBitmap(item.OriginalFullPath, item.ResizedFullPath, model.Height, model.Witdh);

				// high quality is your top-priority, run on Windows only
				(int height1, int width1, long length1) = ImageResizerFactory.MagicScaler(item.OriginalFullPath, item.ResizedFullPath);
				if (!System.IO.File.Exists(item.ResizedFullPath) || length1 <= 0)
					continue;

				item.ResizedWidth = width1;
				item.ResizedHeight = height1;
				item.ResizedLength = length1;
			}
			return list;
		}

		private static async Task<FileUploadResult> SaveFile(IFormFile file, IDatetimeServiceProvider datetimeService, IWebHostEnvironment environment)
		{
			string originalFilesPath = OriginalFileSavedPath(environment);
			string newFileName = CreateUniqueFileName(file.FileName, datetimeService);
			var filePathOriginal = Path.Combine(originalFilesPath, newFileName);

			try
			{
				using (Stream stream = file.OpenReadStream())
				{
					using var mstream = new MemoryStream();
					await stream.CopyToAsync(mstream);
					await filePathOriginal
							.WriteFileAsBytesAsync(mstream.ToArray())
							.ConfigureAwait(false);
				}
			}
			catch
			{
				// todo: log error				
				throw;
			}

			// get file properties
			(int height, int width, long length) = filePathOriginal.GetImageSize();

			string resizerFilesPath = DestinationResizerImagePath(environment);
			var filePathResize = Path.Combine(resizerFilesPath, newFileName);
			FileUploadResult uploadResult = new()
			{
				Uploaded = true,
				FileName = file.FileName,
				NewFileName = newFileName,
				OriginalFullPath = filePathOriginal,
				ResizedFullPath = filePathResize,
				Length = file.Length,
				Width = width,
				Height = height
			};
			return uploadResult;
		}

		private static bool CheckHasValidFileType(string uploadedFileName)
		{
			string ext = GetImageFileExtension(uploadedFileName);
			if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
				return false; // The extension is invalid ...

			return true;
		}
		private static string CreateUniqueFileName(string fileName, IDatetimeServiceProvider datetimeServiceProvider)
		{
			return $"{GetRandomFileNameAsGuid}_{datetimeServiceProvider.Now.Year}{datetimeServiceProvider.Now.Month}{datetimeServiceProvider.Now.Day}{GetImageFileExtension(fileName)}";
		}
		private static bool IsMultipartContentType(string contentType)
		{
			return !string.IsNullOrEmpty(contentType)
				   && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
		}
		private static string GetImageFileExtension(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				return string.Empty;

			var ext = Path.GetExtension(fileName).ToLowerInvariant();
			if (string.IsNullOrEmpty(ext))
				return string.Empty;

			return ext;
		}
		private static string OriginalFileSavedPath(IWebHostEnvironment environment)
		{
			var originalFilesPath = Path.Combine(environment.WebRootPath, "images", "original-uploads");
			if (!Directory.Exists(originalFilesPath))
				Directory.CreateDirectory(originalFilesPath);
			return originalFilesPath;
		}
		private static string DestinationResizerImagePath(IWebHostEnvironment environment)
		{
			var resizerFilesPath = Path.Combine(environment.WebRootPath, "images", "resizer-uploads");
			if (!Directory.Exists(resizerFilesPath))
				Directory.CreateDirectory(resizerFilesPath);
			return resizerFilesPath;
		}

		private static readonly List<string> _permittedExtensions = new() { ".png", ".jpeg", ".jpg" };
		private static string GetRandomFileNameAsGuid => Guid.NewGuid().ToString();
	}
}
