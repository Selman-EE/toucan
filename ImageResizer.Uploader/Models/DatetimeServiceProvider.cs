using System;

namespace ImageResizer.Uploader.Models
{
	public class DatetimeServiceProvider : IDatetimeServiceProvider
	{
		public DateTime Now => DateTime.Now;
	}

}
