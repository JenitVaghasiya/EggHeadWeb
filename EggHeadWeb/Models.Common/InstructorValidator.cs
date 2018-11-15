using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class InstructorValidator : AbstractValidator<Instructor>
	{
		public InstructorValidator()
		{
			base.RuleFor<string>((Instructor i) => i.Address).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("Address is required.");
			base.RuleFor<long>((Instructor i) => i.AreaId).NotEmpty<Instructor, long>().WithMessage<Instructor, long>("Area is required.");
			base.RuleFor<string>((Instructor i) => i.City).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("City is required.");
			base.RuleFor<string>((Instructor i) => i.Email).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("Email is required.").EmailAddress<Instructor>().WithMessage<Instructor, string>("Email is not valid");
			base.RuleFor<string>((Instructor i) => i.FirstName).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("First name is required.");
			base.RuleFor<string>((Instructor i) => i.LastName).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("Last name is required.");
			base.RuleFor<string>((Instructor i) => i.Pay).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("Pay is required.");
			base.RuleFor<string>((Instructor i) => i.PhoneNumber).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("Phone number is required.");
			base.RuleFor<string>((Instructor i) => i.State).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("State is required.");
			base.RuleFor<string>((Instructor i) => i.Zip).NotEmpty<Instructor, string>().WithMessage<Instructor, string>("Zip is required.");
		}
	}
}