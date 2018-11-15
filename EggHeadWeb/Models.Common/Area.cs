using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(AreaValidator))]
	public class Area
	{
		public virtual ICollection<Admin> Admins
		{
			get;
			set;
		}

		public virtual ICollection<Birthday> Birthdays
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public virtual ICollection<Instructor> Instructors
		{
			get;
			set;
		}

		public virtual ICollection<Location> Locations
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public Area()
		{
			this.Admins = new HashSet<Admin>();
			this.Instructors = new HashSet<Instructor>();
			this.Locations = new HashSet<Location>();
			this.Birthdays = new HashSet<Birthday>();
		}
	}
}