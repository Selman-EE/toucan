using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ImageResizer.Uploader
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
					.ConfigureKestrel((context, options) =>
					{
						// Handle requests up to 1GB
						options.Limits.MaxRequestBodySize = 1024 * 1024 * 1024;
					})
					.UseStartup<Startup>();
				});
	}
}
