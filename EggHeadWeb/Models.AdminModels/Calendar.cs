using EggheadWeb.Models.Common;
using EggHeadWeb.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class Calendar
	{
		public List<Assign> Events
		{
			get;
			set;
		}

		public DateTime FromDate
		{
			get;
			private set;
		}

		public DateTime NextDate
		{
			get;
			private set;
		}

		public DateTime PreviousDate
		{
			get;
			private set;
		}

		public List<ServiceType> ServiceTypes
		{
			get;
			set;
		}

		public DateTime ToDate
		{
			get;
			private set;
		}

		public CalendarViewType Type
		{
			get;
			private set;
		}

		public Calendar(DateTime fromDay, CalendarViewType type, List<ServiceType> serviceTypes)
		{
			this.Type = type;
			this.ServiceTypes = new List<ServiceType>(serviceTypes);
			switch (this.Type)
			{
				case CalendarViewType.Day:
				{
					this.FromDate = fromDay;
					this.ToDate = this.FromDate;
					this.PreviousDate = this.FromDate.AddDays(-1);
					this.NextDate = this.FromDate.AddDays(1);
					return;
				}
				case CalendarViewType.Week:
				{
					this.FromDate = fromDay.AddDays((double)(-(int)fromDay.DayOfWeek));
					this.ToDate = this.FromDate.AddDays(6);
					this.PreviousDate = this.FromDate.AddDays(-7);
					this.NextDate = this.FromDate.AddDays(7);
					return;
				}
				case CalendarViewType.Month:
				{
					this.FromDate = new DateTime(fromDay.Year, fromDay.Month, 1);
					DateTime dateTime = this.FromDate.AddMonths(1);
					this.ToDate = dateTime.AddDays(-1);
					this.PreviousDate = this.FromDate.AddMonths(-1);
					this.NextDate = this.FromDate.AddMonths(1);
					return;
				}
				default:
				{
					return;
				}
			}
		}
	}
}