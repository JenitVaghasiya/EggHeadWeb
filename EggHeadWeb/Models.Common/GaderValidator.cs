using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class GaderValidator : AbstractValidator<Grade>
	{
		public GaderValidator()
		{
			base.RuleFor<string>((Grade c) => c.Name).NotEmpty<Grade, string>().WithMessage<Grade, string>("Name is is required.");
		}
	}
}