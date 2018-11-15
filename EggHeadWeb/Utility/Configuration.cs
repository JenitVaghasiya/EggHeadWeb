using PayPal.Api;
using System;
using System.Collections.Generic;

namespace EggheadWeb.Utility
{
	public static class Configuration
	{
		public readonly static string ClientId;

		public readonly static string ClientSecret;

		static Configuration()
		{
			Dictionary<string, string> config = Configuration.GetConfig();
			Configuration.ClientId = config["clientId"];
			Configuration.ClientSecret = config["clientSecret"];
		}

		private static string GetAccessToken()
		{
			string accessToken = (new OAuthTokenCredential(Configuration.ClientId, Configuration.ClientSecret, Configuration.GetConfig())).GetAccessToken();
			return accessToken;
		}

		public static APIContext GetAPIContext(string accessToken = "")
		{
			APIContext apiContext = new APIContext((string.IsNullOrEmpty(accessToken) ? Configuration.GetAccessToken() : accessToken));
			apiContext.Config = Configuration.GetConfig();
			return apiContext;
		}

		public static Dictionary<string, string> GetConfig()
		{
			return ConfigManager.Instance.GetProperties();
		}
	}
}