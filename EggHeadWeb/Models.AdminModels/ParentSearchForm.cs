using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class ParentSearchForm
	{
		public long? LocationId
		{
			get;
			set;
		}

		public string QuickSearch
		{
			get;
			set;
		}

		public ParentSearchForm()
		{
		}
	}
}