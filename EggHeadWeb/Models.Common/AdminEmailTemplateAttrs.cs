using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	public class AdminEmailTemplateAttrs
	{
		[AllowHtml]
		public string MailBody
		{
			get;
			set;
		}

		public AdminEmailTemplateAttrs()
		{
		}
	}
}