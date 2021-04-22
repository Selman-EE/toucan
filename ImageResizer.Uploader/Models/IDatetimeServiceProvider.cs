using System;

namespace ImageResizer.Uploader.Models
{
	public interface IDatetimeServiceProvider
	{
		DateTime Now { get; }
	}
}