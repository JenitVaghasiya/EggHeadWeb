using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(AdminEmailTemplateAttrs))]
	[ValidateInput(false)]
	[Validator(typeof(AdminEmailTemplateValidator))]
	public class AdminEmailTemplate
	{
		public virtual EggheadWeb.Models.Common.Admin Admin
		{
			get;
			set;
		}

		public long AdminId
		{
			get;
			set;
		}

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

		public AdminEmailTemplate()
		{
		}
	}
}