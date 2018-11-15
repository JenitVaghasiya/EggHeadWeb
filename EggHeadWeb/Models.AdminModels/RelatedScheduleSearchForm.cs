using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class RelatedScheduleSearchForm
	{
		public long? InstructorId
		{
			get;
			set;
		}

		public long? LocationId
		{
			get;
			set;
		}

		public long? ParentId
		{
			get;
			set;
		}

		public RelatedScheduleSearchForm()
		{
		}
	}
}