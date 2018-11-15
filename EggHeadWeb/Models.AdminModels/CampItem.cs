using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class CampItem
	{
		public long AssistantId
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

		public DateTime DisplayUntil
		{
			get;
			set;
		}

		public long GradeGroupId
		{
			get;
			set;
		}

		public List<GradeSelect> GradeSelects
		{
			get;
			set;
		}

		public long Id
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

		public CampItem()
		{
			this.GradeSelects = (
				from k in (new EggheadEntities()).Grades
				select new GradeSelect()
				{
					GradeId = (long)k.Id,
					GradeName = k.Name,
					Value = false
				}).ToList<GradeSelect>();
		}
	}
}