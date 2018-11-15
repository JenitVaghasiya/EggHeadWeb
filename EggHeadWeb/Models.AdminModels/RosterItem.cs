using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class RosterItem
	{
		public string ClassName
		{
			get;
			set;
		}

		public string ClassNo
		{
			get;
			set;
		}

		public int Enrolls
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

		public string InstructorEmail
		{
			get;
			set;
		}

		public string Location
		{
			get;
			set;
		}

		public DateTime StartDate
		{
			get;
			set;
		}

		public ServiceType Type
		{
			get;
			set;
		}

		public RosterItem()
		{
		}
	}
}