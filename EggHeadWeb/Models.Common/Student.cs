using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(StudentAttrs))]
	[Validator(typeof(StudentValidator))]
	public class Student
	{
		public DateTime BirthDate
		{
			get;
			set;
		}

		public virtual ICollection<Booking> Bookings
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string Gender
		{
			get;
			set;
		}

		public string GenderText
		{
			get
			{
				string gender = this.Gender;
				string str = gender;
				if (gender != null)
				{
					if (str == "M")
					{
						return "Male";
					}
					if (str == "F")
					{
						return "Female";
					}
				}
				return "[Unknown]";
			}
		}

		public virtual EggheadWeb.Models.Common.Grade Grade
		{
			get;
			set;
		}

		public byte GradeId
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public string Notes
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Parent Parent
		{
			get;
			set;
		}

		public long ParentId
		{
			get;
			set;
		}

		public Student()
		{
			this.Bookings = new HashSet<Booking>();
		}
	}
}