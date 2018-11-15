using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class ScheduleSearchForm
	{
		public bool AllDays
		{
			get
			{
				if (!this.Monday || !this.Tuesday || !this.Wednesday || !this.Thursday || !this.Friday || !this.Saturday)
				{
					return false;
				}
				return this.Sunday;
			}
		}

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

		public int? ClassPage
		{
			get;
			set;
		}

		public DateTime? DateFrom
		{
			get;
			set;
		}

		public DateTime? DateTo
		{
			get;
			set;
		}

		public bool Friday
		{
			get;
			set;
		}

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

		public bool Monday
		{
			get;
			set;
		}

		public bool Saturday
		{
			get;
			set;
		}

		public List<int> SelectedDays
		{
			get
			{
				List<int> days = new List<int>();
				if (this.Monday)
				{
					days.Add(1);
				}
				if (this.Tuesday)
				{
					days.Add(2);
				}
				if (this.Wednesday)
				{
					days.Add(3);
				}
				if (this.Thursday)
				{
					days.Add(4);
				}
				if (this.Friday)
				{
					days.Add(5);
				}
				if (this.Saturday)
				{
					days.Add(6);
				}
				if (this.Sunday)
				{
					days.Add(0);
				}
				return days;
			}
		}

		public bool Sunday
		{
			get;
			set;
		}

		public bool Thursday
		{
			get;
			set;
		}

		public bool Tuesday
		{
			get;
			set;
		}

		public ServiceType? Type
		{
			get;
			set;
		}

		public bool Wednesday
		{
			get;
			set;
		}

		public int? WorkshopPage
		{
			get;
			set;
		}

		public ScheduleSearchForm()
		{
			this.Monday = true;
			this.Tuesday = true;
			this.Wednesday = true;
			this.Thursday = true;
			this.Friday = true;
			this.Saturday = true;
			this.Sunday = true;
		}
	}
}