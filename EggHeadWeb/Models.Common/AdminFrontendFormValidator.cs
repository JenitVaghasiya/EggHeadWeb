using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class AdminFrontendFormValidator : AbstractValidator<AdminFrontend>
	{
		public AdminFrontendFormValidator()
		{
			base.RuleFor<string>((AdminFrontend a) => a.Name).NotEmpty<AdminFrontend, string>().WithMessage<AdminFrontend, string>("Name is required.");
			base.RuleFor<string>((AdminFrontend a) => a.MenuName).NotEmpty<AdminFrontend, string>().WithMessage<AdminFrontend, string>("Menu text is required.");
			base.RuleFor<string>((AdminFrontend a) => a.OverridePageContent).NotEmpty<AdminFrontend, string>().WithMessage<AdminFrontend, string>("Page content is required.");
		}
	}
}