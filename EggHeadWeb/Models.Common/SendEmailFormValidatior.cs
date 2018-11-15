using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class SendEmailFormValidatior : AbstractValidator<SendEmailForm>
	{
		public SendEmailFormValidatior()
		{
			base.RuleFor<string>((SendEmailForm c) => c.From).NotEmpty<SendEmailForm, string>().WithMessage<SendEmailForm, string>("From is is required.");
			base.RuleFor<string>((SendEmailForm c) => c.ToAddress).NotEmpty<SendEmailForm, string>().WithMessage<SendEmailForm, string>("To is is required.");
			base.RuleFor<string>((SendEmailForm c) => c.Subject).NotEmpty<SendEmailForm, string>().WithMessage<SendEmailForm, string>("From is is required.");
			base.RuleFor<string>((SendEmailForm c) => c.Body).NotEmpty<SendEmailForm, string>().WithMessage<SendEmailForm, string>("Body is is required.");
		}
	}
}