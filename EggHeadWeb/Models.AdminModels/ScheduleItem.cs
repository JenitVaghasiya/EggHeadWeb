using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class ScheduleItem
	{
		public string Class
		{
			get;
			set;
		}

		public int Cost
		{
			get;
			set;
		}

		public string Dates
		{
			get;
			set;
		}

		public int Enrolls
		{
			get;
			set;
		}

		public string Grades
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public string Instructor
		{
			get;
			set;
		}

		public string Location
		{
			get;
			set;
		}

		public string TimeEast
		{
			get;
			set;
		}

		public string TimeSouth
		{
			get;
			set;
		}

		public ServiceType Type
		{
			get;
			set;
		}

		public ScheduleItem()
		{
		}
	}
}