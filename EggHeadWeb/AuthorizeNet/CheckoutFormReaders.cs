using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace AuthorizeNet
{
	public static class CheckoutFormReaders
	{
		public static IGatewayRequest BuildAuthAndCaptureFromPost(NameValueCollection post)
		{
			AuthorizationRequest request = new AuthorizationRequest(post);
			CheckoutFormReaders.SerializeForm(request, post);
			return request;
		}

		public static IGatewayRequest BuildAuthAndCaptureFromPost()
		{
			return CheckoutFormReaders.BuildAuthAndCaptureFromPost(HttpContext.Current.Request.Form);
		}

		private static void SerializeForm(IGatewayRequest request, NameValueCollection collection)
		{
			ApiFields apiField = new ApiFields();
			foreach (string item in collection.Keys)
			{
				request.Queue(item, collection[item]);
			}
		}
	}
}