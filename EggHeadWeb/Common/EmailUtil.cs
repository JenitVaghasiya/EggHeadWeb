using EggheadWeb.Models.Common;
using Mvc.Mailer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Common
{
	public class EmailUtil
	{
		public const string DYNAMIC_CONTENT_NAME = "[Name]";

		public const string DYNAMIC_CONTENT_NEW_PASSWORD = "[New_Password]";

		public EmailUtil()
		{
		}

		public static SmtpClientWrapper CreateStmtClientWrapperForAdmin(EggheadEntities db, Admin admin)
		{
			SmtpClientWrapper stmpClient = new SmtpClientWrapper(EmailUtil.GetSystemSmtpClient(db));
			stmpClient.InnerSmtpClient.Credentials = new NetworkCredential(admin.Email, CryptoUtil.Decrypt(admin.EmailPassword, admin.Username));
			return stmpClient;
		}

		public static SmtpClient GetSystemSmtpClient(EggheadEntities db)
		{
			List<APVariable> apVariables = db.APVariables.ToList<APVariable>();
			SmtpClient systemSmtpClient = new SmtpClient()
			{
				Host = apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_HOST").Value,
				Port = Convert.ToInt32(apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_PORT").Value),
				EnableSsl = Convert.ToBoolean(apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_ENABLE_SSL").Value),
				Credentials = new NetworkCredential(apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_USER_NAME").Value, apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_PASSWORD").Value)
			};
			return systemSmtpClient;
		}

		public static SmtpClient GetSystemSmtpClient(string host, int port, bool enableSSL, string userName, string pass)
		{
			SmtpClient systemSmtpClient = new SmtpClient()
			{
				Host = host,
				Port = port,
				EnableSsl = enableSSL,
				Credentials = new NetworkCredential(userName, pass)
			};
			return systemSmtpClient;
		}

		public static string ReplaceDynamicContent(string input, string keyword, string dynamicName)
		{
			return input.Replace(keyword, dynamicName).Replace(keyword, dynamicName);
		}
	}
}