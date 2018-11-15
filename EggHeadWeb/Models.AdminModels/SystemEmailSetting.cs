using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class SystemEmailSetting
	{
		public string Email
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public string SmtpHost
		{
			get;
			set;
		}

		public int SmtpPort
		{
			get;
			set;
		}

		public bool UseSSL
		{
			get;
			set;
		}

		public SystemEmailSetting()
		{
		}
	}
}