using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Utility
{
	public class PayPalRedirect
	{
		public string Token
		{
			get;
			set;
		}

		public string Url
		{
			get;
			set;
		}

		public PayPalRedirect()
		{
		}
	}
}