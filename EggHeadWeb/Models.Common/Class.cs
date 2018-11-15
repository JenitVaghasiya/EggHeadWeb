using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(ClassCampAttrs))]
	[Validator(typeof(ClassValidator))]
	public class Class
	{
		public List<Assign> AssignList
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

		public bool CanRegistOnline
		{
			get;
			set;
		}

		public decimal Cost
		{
			get;
			set;
		}

		public string Dates
		{
			get;
			set;
		}

		public DateTime? DisplayUntil
		{
			get;
			set;
		}

		public int? Enrolled
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.GradeGroup GradeGroup
		{
			get;
			set;
		}

		public long GradeGroupId
		{
			get;
			set;
		}

		public List<int> GradeIds
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

		public virtual EggheadWeb.Models.Common.Instructor Instructor
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Instructor Instructor1
		{
			get;
			set;
		}

		public long InstructorId
		{
			get;
			set;
		}

		public bool IsOpen
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

		public int MaxEnroll
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public decimal? NCost
		{
			get;
			set;
		}

		public DateTime? NDisplayUntil
		{
			get;
			set;
		}

		public int? NMaxEnroll
		{
			get;
			set;
		}

		public string Notes
		{
			get;
			set;
		}

		public string OnlineDescription
		{
			get;
			set;
		}

		public string OnlineName
		{
			get;
			set;
		}

		public TimeSpan TimeEnd
		{
			get;
			set;
		}

		public TimeSpan TimeStart
		{
			get;
			set;
		}

		public Class()
		{
			this.Assigns = new HashSet<Assign>();
			this.Bookings = new HashSet<Booking>();
		}

		public void UpdateCustomProperties()
		{
			this.AssignList = (
				from t in this.Assigns
				orderby t.Date
				select t).ToList<Assign>();
			this.AssignList.ForEach((Assign t) => t.NDate = new DateTime?(t.Date));
			this.GradeIds = (
				from g in this.GradeGroup.Grades
				select g.Id).ToList<int>();
			this.Dates = string.Join(", ", (
				from a in this.Assigns
				select a.Date.ToShortDateString()).ToArray<string>());
			this.NDisplayUntil = this.DisplayUntil;
			this.NMaxEnroll = new int?(this.MaxEnroll);
			this.NCost = new decimal?(this.Cost);
		}
	}
}