using EggheadWeb.Models.Common;
using EggheadWeb.Models.UserModels;
using System;
using System.Net.Mail;

namespace EggheadWeb.Mailers
{
	public interface IUserMailer
	{
		MailMessage PasswordReset(Parent parent, string newPassword);

		MailMessage UserAdminPasswordReset(Admin parent, string newPassword);

		MailMessage WelcomeToNewUser(ParentItem parent);
	}
}