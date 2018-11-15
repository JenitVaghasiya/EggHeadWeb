using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.AdminModels
{
	public class FrontendItem
	{
		public int FrontendId
		{
			get;
			set;
		}

		public int Id
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		public string MenuName
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		[AllowHtml]
		public string PageContent
		{
			get;
			set;
		}

		public FrontendItem()
		{
		}
	}
}