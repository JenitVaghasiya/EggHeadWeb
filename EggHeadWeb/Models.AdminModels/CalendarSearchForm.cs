using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class CalendarSearchForm
	{
		public long? AssistantId
		{
			get;
			set;
		}

		public long? BirthdayId
		{
			get;
			set;
		}

		public long? CampId
		{
			get;
			set;
		}

		public long? ClassId
		{
			get;
			set;
		}

		public DateTime? Date
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

		public long? ParentId
		{
			get;
			set;
		}

		public List<ServiceType> ServiceTypes
		{
			get
			{
				List<ServiceType> types = new List<ServiceType>();
				if (this.ShowClasses)
				{
					types.Add(ServiceType.Class);
				}
				if (this.ShowCamps)
				{
					types.Add(ServiceType.Camp);
				}
				if (this.ShowBirthdays)
				{
					types.Add(ServiceType.Birthday);
				}
				if (this.ShowWorkshops)
				{
					types.Add(ServiceType.Workshop);
				}
				return types;
			}
		}

		[Display(Name="Birthdays")]
		public bool ShowBirthdays
		{
			get;
			set;
		}

		[Display(Name="Camps")]
		public bool ShowCamps
		{
			get;
			set;
		}

		[Display(Name="Classes")]
		public bool ShowClasses
		{
			get;
			set;
		}

		[Display(Name="Workshops")]
		public bool ShowWorkshops
		{
			get;
			set;
		}

		public long? StudentId
		{
			get;
			set;
		}

		public CalendarViewType ViewBy
		{
			get;
			set;
		}

		public long? WorkshopId
		{
			get;
			set;
		}

		public CalendarSearchForm()
		{
			this.ShowClasses = true;
			this.ShowCamps = true;
			this.ShowBirthdays = true;
			this.ShowWorkshops = true;
			this.ViewBy = CalendarViewType.Month;
		}
	}
}