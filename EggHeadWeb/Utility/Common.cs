using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace EggheadWeb.Utility
{
	public static class Common
	{
		public static string FormatJsonString(string json)
		{
			if (string.IsNullOrEmpty(json))
			{
				return string.Empty;
			}
			if (!json.StartsWith("["))
			{
				return JObject.Parse(json).ToString();
			}
			json = string.Concat("{\"list\":", json, "}");
			string formattedText = JObject.Parse(json).ToString();
			formattedText = formattedText.Substring(13, formattedText.Length - 14).Replace("\n  ", "\n");
			return formattedText;
		}

		public static string GetRandomInvoiceNumber()
		{
			return (new Random()).Next(999999).ToString();
		}
	}
}