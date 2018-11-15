using EggheadWeb.Models.AdminModels;
using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class InstructorItemValidator : AbstractValidator<InstructorItem1>
	{
		public InstructorItemValidator()
		{
			base.RuleFor<string>((InstructorItem1 i) => i.Address).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("Address is required.");
			base.RuleFor<long>((InstructorItem1 i) => i.AreaId).NotEmpty<InstructorItem1, long>().WithMessage<InstructorItem1, long>("Area is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.City).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("City is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.Email).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("Email is required.").EmailAddress<InstructorItem1>().WithMessage<InstructorItem1, string>("Email is not valid");
			base.RuleFor<string>((InstructorItem1 i) => i.FirstName).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("First name is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.Gender).NotNull<InstructorItem1, string>().WithMessage<InstructorItem1, string>("Gender is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.LastName).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("Last name is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.Pay).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("Pay is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.PhoneNumber).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("Phone number is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.State).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("State is required.");
			base.RuleFor<string>((InstructorItem1 i) => i.Zip).NotEmpty<InstructorItem1, string>().WithMessage<InstructorItem1, string>("Zip is required.");
		}
	}
}