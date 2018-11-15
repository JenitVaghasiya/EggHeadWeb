using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(InstructorValidator))]
	public class Instructor
	{
		public string Address
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Area Area
		{
			get;
			set;
		}

		public long AreaId
		{
			get;
			set;
		}

		public virtual ICollection<Assign> Assigns
		{
			get;
			set;
		}

		public virtual ICollection<Assign> Assigns1
		{
			get;
			set;
		}

		public virtual ICollection<Camp> Camps
		{
			get;
			set;
		}

		public virtual ICollection<Camp> Camps1
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public virtual ICollection<Class> Classes
		{
			get;
			set;
		}

		public virtual ICollection<Class> Classes1
		{
			get;
			set;
		}

		public long CreatedBy
		{
			get;
			set;
		}

		public DateTime CreatedDate
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

		public long Id
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public string Note
		{
			get;
			set;
		}

		public string Pay
		{
			get;
			set;
		}

		public string PhoneNumber
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public virtual ICollection<Workshop> Workshops
		{
			get;
			set;
		}

		public virtual ICollection<Workshop> Workshops1
		{
			get;
			set;
		}

		public string Zip
		{
			get;
			set;
		}

		public Instructor()
		{
			this.Assigns = new HashSet<Assign>();
			this.Assigns1 = new HashSet<Assign>();
			this.Workshops = new HashSet<Workshop>();
			this.Workshops1 = new HashSet<Workshop>();
			this.Camps = new HashSet<Camp>();
			this.Camps1 = new HashSet<Camp>();
			this.Classes = new HashSet<Class>();
			this.Classes1 = new HashSet<Class>();
		}
	}
}