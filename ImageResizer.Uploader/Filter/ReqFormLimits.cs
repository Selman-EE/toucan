using Microsoft.AspNetCore.Mvc;

namespace ImageResizer.Uploader.Filter
{
	public class ReqFormLimits : RequestFormLimitsAttribute
	{
		// todo : Customize request limit 

		/*
		 *  Error Message will be :
		 *  HTTP 404.13 - Not Found
		 *  The request filtering is configured to deny a request that exceeds the request form content length.
		 */

	}

}
