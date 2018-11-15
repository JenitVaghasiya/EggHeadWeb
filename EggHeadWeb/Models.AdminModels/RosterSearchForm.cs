using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class RosterSearchForm
	{
		public int? BirthdayPage
		{
			get;
			set;
		}

		public int? CampPage
		{
			get;
			set;
		}

		public string ClassName
		{
			get;
			set;
		}

		public int? ClassPage
		{
			get;
			set;
		}

		public int? InstructorId
		{
			get;
			set;
		}

		public int? LocationId
		{
			get;
			set;
		}

		public DateTime? StartDate
		{
			get;
			set;
		}

		public ServiceType? Type
		{
			get;
			set;
		}

		public int? WorkshopPage
		{
			get;
			set;
		}

		public RosterSearchForm()
		{
		}
	}
}