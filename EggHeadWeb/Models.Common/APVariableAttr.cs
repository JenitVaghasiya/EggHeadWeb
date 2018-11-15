using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	public class APVariableAttr
	{
		[AllowHtml]
		public string Value
		{
			get;
			set;
		}

		public APVariableAttr()
		{
		}
	}
}