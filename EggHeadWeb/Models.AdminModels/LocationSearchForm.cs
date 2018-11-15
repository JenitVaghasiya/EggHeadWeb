using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class LocationSearchForm
	{
		public bool? CanEnrollOnline
		{
			get;
			set;
		}

		public bool? IsActive
		{
			get;
			set;
		}

		public string QuickSearch
		{
			get;
			set;
		}

		public LocationSearchForm()
		{
		}
	}
}