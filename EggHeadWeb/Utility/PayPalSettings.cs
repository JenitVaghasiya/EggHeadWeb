using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;

namespace EggheadWeb.Utility
{
	public static class PayPalSettings
	{
		public static string ApiDomain
		{
			get
			{
				if (!PayPalSettings.Setting<bool>("PayPal:Sandbox"))
				{
					return "api-3t.paypal.com";
				}
				return "api-3t.sandbox.paypal.com";
			}
		}

		public static string BaseReturnUrl
		{
			get
			{
				return PayPalSettings.Setting<string>("PayPal:BaseReturnUrl");
			}
		}

		public static string CancelUrl
		{
			get
			{
				return PayPalSettings.Setting<string>("PayPal:CancelUrl");
			}
		}

		public static string CgiDomain
		{
			get
			{
				if (!PayPalSettings.Setting<bool>("PayPal:Sandbox"))
				{
					return "www.paypal.com";
				}
				return "www.sandbox.paypal.com";
			}
		}

		public static string Password
		{
			get
			{
				return PayPalSettings.Setting<string>("PayPal:Password");
			}
		}

		public static string ReturnUrl
		{
			get
			{
				return PayPalSettings.Setting<string>("PayPal:ReturnUrl");
			}
		}

		public static string Signature
		{
			get
			{
				return PayPalSettings.Setting<string>("PayPal:Signature");
			}
		}

		public static string Username
		{
			get
			{
				return PayPalSettings.Setting<string>("PayPal:Username");
			}
		}

		private static T Setting<T>(string name)
		{
			string value = ConfigurationManager.AppSettings[name];
			if (value == null)
			{
				throw new Exception(string.Format("Could not find setting '{0}',", name));
			}
			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}
	}
}