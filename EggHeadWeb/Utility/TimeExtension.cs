using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Utility
{
	public static class TimeExtension
	{
		public static string To12HoursString(this TimeSpan time)
		{
			return (new DateTime(time.Ticks)).ToString("h:mm tt");
		}
	}
}