using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(ParentValidator))]
	public class Parent
	{
		public string Address
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public long? CreatedBy
		{
			get;
			set;
		}

		public DateTime? CreatedDate
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string FullName
		{
			get
			{
				return string.Concat(this.FirstName, " ", this.LastName);
			}
		}

		public long Id
		{
			get;
			set;
		}

		public DateTime? LastLoginDateTime
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Location Location
		{
			get;
			set;
		}

		public long LocationId
		{
			get;
			set;
		}

		public string Note
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public string PhoneNumer
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public virtual ICollection<Student> Students
		{
			get;
			set;
		}

		public string Zip
		{
			get;
			set;
		}

		public Parent()
		{
			this.Students = new HashSet<Student>();
		}
	}
}