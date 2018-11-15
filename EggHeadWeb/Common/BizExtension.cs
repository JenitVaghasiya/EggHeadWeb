using EggheadWeb.Models.Common;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Common
{
	public static class BizExtension
	{
		public static bool IsRunOnDay(this Class klass, DayOfWeek day)
		{
			return klass.Assigns.Any<Assign>((Assign a) => day == a.Date.DayOfWeek);
		}

		public static bool IsRunOnDay(this Camp camp, DayOfWeek day)
		{
			return camp.Assigns.Any<Assign>((Assign a) => day == a.Date.DayOfWeek);
		}

		public static bool IsRunOnDay(this Birthday birthday, DayOfWeek day)
		{
			return birthday.Assigns.Any<Assign>((Assign a) => day == a.Date.DayOfWeek);
		}

		public static bool IsRunOnDay(this Workshop workshop, DayOfWeek day)
		{
			return workshop.Assigns.Any<Assign>((Assign a) => day == a.Date.DayOfWeek);
		}
	}
}