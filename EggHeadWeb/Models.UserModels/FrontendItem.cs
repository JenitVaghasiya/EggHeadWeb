using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.UserModels
{
	public class FrontendItem
	{
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