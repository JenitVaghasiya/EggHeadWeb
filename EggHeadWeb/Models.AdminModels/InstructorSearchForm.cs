using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class InstructorSearchForm
	{
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

		public InstructorSearchForm()
		{
		}
	}
}