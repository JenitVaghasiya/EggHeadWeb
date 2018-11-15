using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	public class FrontendAttr
	{
		[AllowHtml]
		public string PageContent
		{
			get;
			set;
		}

		public FrontendAttr()
		{
		}
	}
}