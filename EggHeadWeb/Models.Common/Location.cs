using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(LocationValidator))]
	public class Location
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

		public virtual ICollection<Camp> Camps
		{
			get;
			set;
		}

		public bool CanRegistOnline
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

		public string ContactPerson
		{
			get;
			set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public string Email
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

		public string Name
		{
			get;
			set;
		}

		public string Note
		{
			get;
			set;
		}

		public virtual ICollection<Parent> Parents
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

		public long UpdatedBy
		{
			get;
			set;
		}

		public DateTime UpdatedDate
		{
			get;
			set;
		}

		public virtual ICollection<Workshop> Workshops
		{
			get;
			set;
		}

		public string Zip
		{
			get;
			set;
		}

		public Location()
		{
			this.Parents = new HashSet<Parent>();
			this.Workshops = new HashSet<Workshop>();
			this.Camps = new HashSet<Camp>();
			this.Classes = new HashSet<Class>();
		}
	}
}