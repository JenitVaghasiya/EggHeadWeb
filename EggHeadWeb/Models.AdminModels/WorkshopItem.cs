using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class WorkshopItem
	{
		public long AssistantId
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

		public int GradeGroupId
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

		public WorkshopItem()
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