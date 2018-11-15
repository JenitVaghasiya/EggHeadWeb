using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Utility
{
	public static class DateTimeExtension
	{
		private const string M_DD_YYYY = "M/dd/yyyy";

		private const string M_DD_YYYY_H_mm_SS = "M/dd/yyyy H:mm:ss";

		public static string ToOneDigitMonth(this DateTime date)
		{
			return date.ToString("M/dd/yyyy");
		}

		public static string ToOneDigitMonthWithTime(this DateTime date)
		{
			return date.ToString("M/dd/yyyy H:mm:ss");
		}
	}
}