using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(WorkshopAttrs))]
	[Validator(typeof(WorkshopValidator))]
	public class Workshop
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

		public string Notes
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

		public Workshop()
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
			this.NCost = new decimal?(this.Cost);
		}
	}
}