using EggheadWeb.Models.Common;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Common
{
	public class ScheduleUtil
	{
		public ScheduleUtil()
		{
		}

		public static string GetDateListText(dynamic service)
		{
			ICollection<Assign> assgn = service.Assigns as ICollection<Assign>;
			return string.Join(", ", (
				from t in assgn
				orderby t.Date
				select t into a
				select a.Date.ToString("M/dd")).ToArray<string>());
		}

		public static string GetDaysOfService(dynamic service)
		{
			List<DayOfWeek> days = new List<DayOfWeek>();
			foreach (dynamic assign in (IEnumerable)service.Assigns)
			{
				if (!(dynamic)(!days.Contains(assign.Date.DayOfWeek)))
				{
					continue;
				}
				days.Add(assign.Date.DayOfWeek);
			}
			return string.Join(", ", (
				from t in days
				orderby t
				select t.ToString()).ToArray<string>());
		}

		public static string GetGradeListText(dynamic service)
		{
			ICollection<Grade> grades = service.GradeGroup.Grades as ICollection<Grade>;
			return string.Join(", ", (
				from t in grades
				orderby t.Id
				select t into g
				select g.Name).ToArray<string>());
		}

		public static string GetShortInstructorName(Instructor instructor)
		{
			return string.Format("{0}.{1}", instructor.FirstName.Substring(0, 1).ToUpper(), instructor.LastName);
		}
	}
}