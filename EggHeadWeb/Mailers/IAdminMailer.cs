using EggheadWeb.Models.Common;
using System.Collections.Generic;
using System.Net.Mail;

namespace EggheadWeb.Mailers
{
	public interface IAdminMailer
	{
		MailMessage MailOne(SendEmailForm form);

		List<MailMessage> MailWithSeperateOnePerAddress(SendEmailForm form);
	}
}