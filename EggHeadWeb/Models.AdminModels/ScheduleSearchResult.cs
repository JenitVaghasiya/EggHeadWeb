using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class ScheduleSearchResult
	{
		public SearchResult<ScheduleSearchForm, Birthday> Birthdays
		{
			get;
			set;
		}

		public SearchResult<ScheduleSearchForm, Camp> Camps
		{
			get;
			set;
		}

		public SearchResult<ScheduleSearchForm, Class> Classes
		{
			get;
			set;
		}

		public SearchResult<ScheduleSearchForm, Workshop> Workshops
		{
			get;
			set;
		}

		public ScheduleSearchResult()
		{
		}
	}
}