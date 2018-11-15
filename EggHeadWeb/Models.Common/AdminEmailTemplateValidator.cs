using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class AdminEmailTemplateValidator : AbstractValidator<AdminEmailTemplate>
	{
		public AdminEmailTemplateValidator()
		{
			base.RuleFor<string>((AdminEmailTemplate c) => c.Name).NotEmpty<AdminEmailTemplate, string>().WithMessage<AdminEmailTemplate, string>("Name is is required.");
			base.RuleFor<string>((AdminEmailTemplate c) => c.MailSubject).NotEmpty<AdminEmailTemplate, string>().WithMessage<AdminEmailTemplate, string>("Subject is is required.");
			base.RuleFor<string>((AdminEmailTemplate c) => c.MailBody).NotEmpty<AdminEmailTemplate, string>().WithMessage<AdminEmailTemplate, string>("Body is is required.");
		}
	}
}