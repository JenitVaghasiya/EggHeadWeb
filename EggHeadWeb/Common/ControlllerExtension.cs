using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Common
{
	public static class ControlllerExtension
	{
		private const string SUCCESS_MAIN = "SuccessMain";

		private const string SUCCESS_SUB = "SuccessSub";

		private const string ERROR_MAIN = "ErrorMain";

		private const string ERROR_SUB = "ErrorSub";

		public static void SetErrorMessages(this Controller controller, string mainMessage, string subMessage = "")
		{
			controller.TempData.Add("ErrorMain", mainMessage);
		    controller.TempData.Add( "ErrorSub", subMessage);
		}

		public static void SetSuccessMessages(this Controller controller, string mainMessage, string subMessage = "")
		{
		    controller.TempData.Add( "SuccessMain", mainMessage);
		    controller.TempData.Add( "SuccessSub", subMessage);
		}
	}
}