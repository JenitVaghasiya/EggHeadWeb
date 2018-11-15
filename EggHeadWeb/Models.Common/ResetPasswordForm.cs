using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(ResetPasswordFormValidator))]
	public class ResetPasswordForm
	{
		public string Email
		{
			get;
			set;
		}

		public ResetPasswordForm()
		{
		}
	}
}