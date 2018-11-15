using System;

namespace EggheadWeb.Common
{
	public class MisUtil
	{
		public MisUtil()
		{
		}

		public static int CalAge(DateTime birthDate)
		{
			int age = DateTime.Today.Year - birthDate.Year;
			if (DateTime.Today < birthDate.AddYears(age))
			{
				age--;
			}
			return age;
		}

		public static long GetJavascriptTimestamp(DateTime input)
		{
			TimeSpan span = new TimeSpan(DateTime.Parse("1/1/1970").Ticks);
			return input.Subtract(span).Ticks / (long)10000;
		}
	}
}