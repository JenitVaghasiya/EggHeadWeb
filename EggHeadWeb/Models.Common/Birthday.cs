using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(BirtdayAttrs))]
	[Validator(typeof(BirthdayValidator))]
	public class Birthday
	{
		public string Address
		{
			get;
			set;
		}

		public int Age
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

		public long? AssistantId
		{
			get;
			set;
		}

		public virtual ICollection<Booking> Bookings
		{
			get;
			set;
		}

		public string ChildName
		{
			get;
			set;
		}

		public string ContactNumber
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

		public DateTime? InputDate
		{
			get;
			set;
		}

		public long InstructorId
		{
			get;
			set;
		}

		public string Notes
		{
			get;
			set;
		}

		public string ParentName
		{
			get;
			set;
		}

		public DateTime PartyDate
		{
			get;
			set;
		}

		public TimeSpan PartyTime
		{
			get;
			set;
		}

		public Birthday()
		{
			this.Assigns = new HashSet<Assign>();
			this.Bookings = new HashSet<Booking>();
		}

		public void UpdateCustomProperties()
		{
			this.InstructorId = this.Assigns.First<Assign>().InstructorId;
			this.AssistantId = this.Assigns.First<Assign>().AssistantId;
		}
	}
}