using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(SendEmailFormValidatior))]
	public class SendEmailForm
	{
		public string AttachedFilePaths
		{
			get;
			set;
		}

		[AllowHtml]
		public string Body
		{
			get;
			set;
		}

		public string FolderId
		{
			get;
			set;
		}

		public string From
		{
			get;
			set;
		}

		public bool OnePerAddress
		{
			get;
			set;
		}

		public string PreAttachFile
		{
			get;
			set;
		}

		public string Subject
		{
			get;
			set;
		}

		public string ToAddress
		{
			get;
			set;
		}

		public string ToNames
		{
			get;
			set;
		}

		public EmailType Type
		{
			get;
			set;
		}

		public SendEmailForm()
		{
		}
	}
}