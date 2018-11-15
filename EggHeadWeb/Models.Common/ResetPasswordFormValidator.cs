using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class ResetPasswordFormValidator : AbstractValidator<ResetPasswordForm>
	{
		public ResetPasswordFormValidator()
		{
			base.RuleFor<string>((ResetPasswordForm a) => a.Email).NotEmpty<ResetPasswordForm, string>().WithMessage<ResetPasswordForm, string>("Please enter a valid email address.");
		}
	}
}