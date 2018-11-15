using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class UpdateEmailPasswordFormValidator : AbstractValidator<UpdateEmailPasswordForm>
	{
		public UpdateEmailPasswordFormValidator()
		{
			base.RuleFor<string>((UpdateEmailPasswordForm a) => a.NewPassword).NotEmpty<UpdateEmailPasswordForm, string>().WithMessage<UpdateEmailPasswordForm, string>("* Please enter new password.");
			base.RuleFor<string>((UpdateEmailPasswordForm a) => a.ConfirmNewPassword).NotEmpty<UpdateEmailPasswordForm, string>().WithMessage<UpdateEmailPasswordForm, string>("* Please enter a confirm password.").Equal<UpdateEmailPasswordForm, string>((UpdateEmailPasswordForm a) => a.NewPassword, null).WithMessage<UpdateEmailPasswordForm, string>("Confirm Password and password not match");
		}
	}
}