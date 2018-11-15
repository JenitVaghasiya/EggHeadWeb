using EggheadWeb.Common;
using EggheadWeb.Models.UserModels;
using EggHeadWeb.DatabaseContext;
using Mvc.Mailer;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;

namespace EggheadWeb.Mailers
{
	public class UserMailer : MailerBase, IUserMailer
	{
		private EggheadContext db;

		public UserMailer(EggheadContext db)
		{
			this.db = new EggheadContext();
			db.APVariables.ToList<APVariable>();
		}

		public virtual MailMessage EnrollService(Parent parent)
		{
			EmailTemplate welcomeNewUser = this.db.EmailTemplates.FirstOrDefault<EmailTemplate>((EmailTemplate t) => t.Id == 1);
			MailMessage mailMessage1 = new MailMessage()
			{
				IsBodyHtml = true,
				From = new MailAddress(this.db.APVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_USER_NAME").Value),
				Subject = welcomeNewUser.MailSubject,
				Body = welcomeNewUser.MailBody
			};
			MailMessage mailMessage = mailMessage1;
			mailMessage.To.Add(parent.Email);
			return mailMessage;
		}

		public virtual MailMessage PasswordReset(Parent parent, string newPassword)
		{
			EmailTemplate passwordReset = this.db.EmailTemplates.FirstOrDefault<EmailTemplate>((EmailTemplate t) => t.Type == 2);
			MailMessage mailMessage1 = new MailMessage()
			{
				IsBodyHtml = true,
				From = new MailAddress(this.db.APVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_USER_NAME").Value),
				Subject = EmailUtil.ReplaceDynamicContent(passwordReset.MailSubject, "[Name]", StringUtil.GetFullName(parent.FirstName, parent.LastName)),
				Body = EmailUtil.ReplaceDynamicContent(EmailUtil.ReplaceDynamicContent(passwordReset.MailBody, "[Name]", StringUtil.GetFullName(parent.FirstName, parent.LastName)), "[New_Password]", newPassword)
			};
			MailMessage mailMessage = mailMessage1;
			mailMessage.To.Add(parent.Email);
			return mailMessage;
		}

		public virtual MailMessage UserAdminPasswordReset(Admin parent, string newPassword)
		{
			EmailTemplate passwordReset = this.db.EmailTemplates.FirstOrDefault<EmailTemplate>((EmailTemplate t) => t.Type == 2);
			MailMessage mailMessage1 = new MailMessage()
			{
				IsBodyHtml = true,
				From = new MailAddress(this.db.APVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_USER_NAME").Value),
				Subject = EmailUtil.ReplaceDynamicContent(passwordReset.MailSubject, "[Name]", StringUtil.GetFullName(parent.FirstName, parent.LastName)),
				Body = EmailUtil.ReplaceDynamicContent(EmailUtil.ReplaceDynamicContent(passwordReset.MailBody, "[Name]", StringUtil.GetFullName(parent.FirstName, parent.LastName)), "[New_Password]", newPassword)
			};
			MailMessage mailMessage = mailMessage1;
			mailMessage.To.Add(parent.Email);
			return mailMessage;
		}

		public virtual MailMessage WelcomeToNewUser(ParentItem parent)
		{
			EmailTemplate welcomeNewUser = this.db.EmailTemplates.FirstOrDefault<EmailTemplate>((EmailTemplate t) => t.Id == 1);
			MailMessage mailMessage1 = new MailMessage()
			{
				IsBodyHtml = true,
				From = new MailAddress(this.db.APVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_USER_NAME").Value),
				Subject = EmailUtil.ReplaceDynamicContent(welcomeNewUser.MailSubject, "[Name]", StringUtil.GetFullName(parent.FirstName, parent.LastName)),
				Body = EmailUtil.ReplaceDynamicContent(welcomeNewUser.MailBody, "[Name]", StringUtil.GetFullName(parent.FirstName, parent.LastName))
			};
			MailMessage mailMessage = mailMessage1;
			mailMessage.To.Add(parent.Email);
			return mailMessage;
		}
	}
}