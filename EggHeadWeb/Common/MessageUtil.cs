using System;

namespace EggheadWeb.Common
{
	public static class MessageUtil
	{
		public static string GetMessage(string message, params object[] parameters)
		{
			return string.Format(message, parameters);
		}
	}
}