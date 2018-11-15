using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(UpdateEmailPasswordFormValidator))]
	public class UpdateEmailPasswordForm
	{
		public string ConfirmNewPassword
		{
			get;
			set;
		}

		public string NewPassword
		{
			get;
			set;
		}

		public UpdateEmailPasswordForm()
		{
		}
	}
}