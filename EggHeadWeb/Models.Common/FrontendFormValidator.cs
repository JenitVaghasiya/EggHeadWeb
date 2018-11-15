using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class FrontendFormValidator : AbstractValidator<Frontend>
	{
		public FrontendFormValidator()
		{
			base.RuleFor<string>((Frontend a) => a.Name).NotEmpty<Frontend, string>().WithMessage<Frontend, string>("Name is required.");
			base.RuleFor<string>((Frontend a) => a.MenuName).NotEmpty<Frontend, string>().WithMessage<Frontend, string>("Menu text is required.");
			base.RuleFor<string>((Frontend a) => a.PageContent).NotEmpty<Frontend, string>().WithMessage<Frontend, string>("Page content is required.");
		}
	}
}