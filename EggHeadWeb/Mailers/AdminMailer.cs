using EggheadWeb.Models.Common;
using Mvc.Mailer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Web;

namespace EggheadWeb.Mailers
{
	public class AdminMailer : MailerBase, IAdminMailer
	{
		public AdminMailer()
		{
			this.MasterName = "_Layout";
		}

		public virtual MailMessage MailOne(SendEmailForm form)
		{
			string toAddress = form.ToAddress;
			char[] chrArray = new char[] { ';' };
			List<string> addresses = toAddress.Split(chrArray).ToList<string>();
			MailMessage mailMessage = new MailMessage()
			{
				IsBodyHtml = true,
				From = new MailAddress(form.From),
				Subject = form.Subject,
				Body = form.Body
			};
			MailMessage mailMessage1 = mailMessage;
			addresses.ForEach((string t) => mailMessage1.To.Add(t));
			if (!string.IsNullOrEmpty(form.AttachedFilePaths))
			{
				string[] strArrays = form.AttachedFilePaths.Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string filePath = strArrays[i];
					string fileLocation = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Upload"), filePath);
					Attachment data = new Attachment(fileLocation, "application/octet-stream");
					mailMessage1.Attachments.Add(data);
				}
			}
			return mailMessage1;
		}

		public virtual List<MailMessage> MailWithSeperateOnePerAddress(SendEmailForm form)
		{
			List<MailMessage> mailMesssages = new List<MailMessage>();
			string toAddress = form.ToAddress;
			char[] chrArray = new char[] { ';' };
			List<string> addresses = toAddress.Split(chrArray).ToList<string>();
			string toNames = form.ToNames;
			char[] chrArray1 = new char[] { ';' };
			List<string> names = toNames.Split(chrArray1).ToList<string>();
			string[] filePaths = new string[0];
			if (!string.IsNullOrEmpty(form.AttachedFilePaths))
			{
				filePaths = form.AttachedFilePaths.Split(new char[] { ';' });
			}
			for (int i = 0; i < addresses.Count<string>(); i++)
			{
				MailMessage mailMessage = new MailMessage()
				{
					IsBodyHtml = true,
					From = new MailAddress(form.From),
					Subject = form.Subject.Replace("[Name]", names[i]).Replace("[Name]".ToLower(), names[i]),
					Body = form.Body.Replace("[Name]", names[i]).Replace("[Name]".ToLower(), names[i])
				};
				MailMessage msg = mailMessage;
				msg.To.Add(addresses[i]);
				string[] strArrays = filePaths;
				for (int j = 0; j < (int)strArrays.Length; j++)
				{
					string filePath = strArrays[j];
					string fileLocation = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Upload"), filePath);
					Attachment data = new Attachment(fileLocation, "application/octet-stream");
					msg.Attachments.Add(data);
				}
				mailMesssages.Add(msg);
			}
			return mailMesssages;
		}
	}
}