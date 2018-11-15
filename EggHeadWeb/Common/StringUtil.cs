using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Common
{
	public class StringUtil
	{
		public StringUtil()
		{
		}

		public static string GetFullAddress(string address, string city, string state, string zip)
		{
			object[] objArray = new object[] { address, city, state, zip };
			return string.Format("{0} {1}, {2} {3}", objArray);
		}

		public static string GetFullDate(DateTime date)
		{
			return date.ToString("M/dd/yyyy");
		}

		public static string GetFullDateList(List<DateTime> dates)
		{
			return string.Join(", ", (
				from a in dates
				select a.ToString("M/dd/yyyy")).ToArray<string>());
		}

		public static string GetFullName(string firstName, string lastName)
		{
			return string.Format("{0} {1}", firstName, lastName);
		}

		public static string GetShortDateList(List<DateTime> dates)
		{
			return string.Join(", ", (
				from a in dates
				select a.ToString("MM/dd")).ToArray<string>());
		}
	}
}