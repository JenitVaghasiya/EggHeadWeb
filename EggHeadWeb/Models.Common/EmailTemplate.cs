using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class EmailTemplate
	{
		public int Id
		{
			get;
			set;
		}

		public string MailBody
		{
			get;
			set;
		}

		public string MailSubject
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public int Type
		{
			get;
			set;
		}

		public EmailTemplate()
		{
		}
	}
}