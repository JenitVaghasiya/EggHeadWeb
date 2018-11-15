using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	public class AdminFrontendAttr
	{
		[AllowHtml]
		public string OverridePageContent
		{
			get;
			set;
		}

		public AdminFrontendAttr()
		{
		}
	}
}