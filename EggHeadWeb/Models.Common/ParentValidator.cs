using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class ParentValidator : AbstractValidator<Parent>
	{
		public ParentValidator()
		{
			base.RuleFor<string>((Parent p) => p.Address).NotEmpty<Parent, string>().WithMessage<Parent, string>("Address is required.");
			base.RuleFor<string>((Parent p) => p.City).NotEmpty<Parent, string>().WithMessage<Parent, string>("City is required.");
			base.RuleFor<string>((Parent p) => p.Email).NotEmpty<Parent, string>().WithMessage<Parent, string>("Email address is required.").EmailAddress<Parent>().WithMessage<Parent, string>("Email address is not valid");
			base.RuleFor<string>((Parent p) => p.FirstName).NotEmpty<Parent, string>().WithMessage<Parent, string>("First name is required.");
			base.RuleFor<string>((Parent p) => p.LastName).NotEmpty<Parent, string>().WithMessage<Parent, string>("Last name is required.");
			base.RuleFor<long>((Parent p) => p.LocationId).NotNull<Parent, long>().WithMessage<Parent, long>("Location is required.");
			base.RuleFor<string>((Parent p) => p.Password).Matches<Parent>("^[A-Za-z0-9_@#$%]{3,20}$").WithMessage<Parent, string>("Password is not valid").When<Parent, string>((Parent p) => p.Id <= (long)0, ApplyConditionTo.AllValidators);
			base.RuleFor<string>((Parent p) => p.PhoneNumer).NotEmpty<Parent, string>().WithMessage<Parent, string>("Phone number is required.");
			base.RuleFor<string>((Parent p) => p.State).NotEmpty<Parent, string>().WithMessage<Parent, string>("State is required.");
			base.RuleFor<string>((Parent p) => p.Zip).NotEmpty<Parent, string>().WithMessage<Parent, string>("Zip is required.");
		}
	}
}