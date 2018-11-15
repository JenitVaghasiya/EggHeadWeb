using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class UserSearchForm
	{
		public bool? IsSuperAdmin
		{
			get;
			set;
		}

		public string QuickSearch
		{
			get;
			set;
		}

		public UserSearchForm()
		{
		}
	}
}