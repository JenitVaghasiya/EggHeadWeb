using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.UserModels
{
	public class EnrollStudentItem
	{
		public DateTime BookDate
		{
			get;
			set;
		}

		public long ClassCampId
		{
			get;
			set;
		}

		public string ClassCampName
		{
			get;
			set;
		}

		public long StudentId
		{
			get;
			set;
		}

		public string StudentName
		{
			get;
			set;
		}

		public EnrollStudentItem()
		{
		}
	}
}