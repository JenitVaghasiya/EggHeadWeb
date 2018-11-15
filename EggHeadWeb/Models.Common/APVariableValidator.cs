using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class APVariableValidator : AbstractValidator<APVariable>
	{
		public APVariableValidator()
		{
			base.RuleFor<string>((APVariable a) => a.Name).NotNull<APVariable, string>().WithMessage<APVariable, string>("Name is required.");
			base.RuleFor<string>((APVariable a) => a.Value).NotNull<APVariable, string>().WithMessage<APVariable, string>("Value is required.");
		}
	}
}