using EggheadWeb.Common;
using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class AdminValidator : AbstractValidator<Admin>
	{
		public AdminValidator()
		{
			base.RuleFor<string>((Admin a) => a.FirstName).NotEmpty<Admin, string>().WithMessage<Admin, string>("First name is required.");
			base.RuleFor<string>((Admin a) => a.LastName).NotEmpty<Admin, string>().WithMessage<Admin, string>("Last name is required.");
			base.RuleFor<string>((Admin a) => a.Username).NotEmpty<Admin, string>().WithMessage<Admin, string>("Username is required.").Matches<Admin>("^[A-Za-z0-9_]{3,15}$").WithMessage<Admin, string>("Username is invalid");
			base.RuleFor<string>((Admin a) => a.Password).NotEmpty<Admin, string>().WithMessage<Admin, string>("Password is required.").When<Admin, string>((Admin a) => a.Id <= (long)0, ApplyConditionTo.AllValidators);
			base.RuleFor<string>((Admin a) => a.Password).Matches<Admin>("^[A-Za-z0-9_@#$%]{3,20}$").WithMessage<Admin, string>("Password is invalid").When<Admin, string>((Admin a) => a.Password != Constants.STUB_PASSWORD, ApplyConditionTo.AllValidators);
			base.RuleFor<string>((Admin a) => a.Email).NotEmpty<Admin, string>().WithMessage<Admin, string>("Email address is required.").EmailAddress<Admin>().WithMessage<Admin, string>("Email address is invalid");
			base.RuleFor<string>((Admin a) => a.EmailPassword).NotEmpty<Admin, string>().WithMessage<Admin, string>("Email Password is required.").When<Admin, string>((Admin a) => a.Id <= (long)0, ApplyConditionTo.AllValidators);
		}
	}
}