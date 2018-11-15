using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class BirthdayValidator : AbstractValidator<Birthday>
	{
		public BirthdayValidator()
		{
			base.RuleFor<string>((Birthday c) => c.ParentName).NotEmpty<Birthday, string>().WithMessage<Birthday, string>("Parent name is is required.");
			base.RuleFor<string>((Birthday c) => c.ContactNumber).NotEmpty<Birthday, string>().WithMessage<Birthday, string>("Contact number is is required.");
			base.RuleFor<string>((Birthday c) => c.Email).NotEmpty<Birthday, string>().WithMessage<Birthday, string>("Email address is is required.").EmailAddress<Birthday>().WithMessage<Birthday, string>("Email address is not valid");
			base.RuleFor<string>((Birthday c) => c.Address).NotEmpty<Birthday, string>().WithMessage<Birthday, string>("Address is is required.");
			base.RuleFor<string>((Birthday c) => c.ChildName).NotEmpty<Birthday, string>().WithMessage<Birthday, string>("Child name is is required.");
			base.RuleFor<int>((Birthday c) => c.Age).NotEmpty<Birthday, int>().WithMessage<Birthday, int>("Age is is required.").GreaterThan<Birthday, int>(0).WithMessage<Birthday, int>("Age must be an positive.").LessThan<Birthday, int>(100).WithMessage<Birthday, int>("Age is too high.");
			base.RuleFor<DateTime>((Birthday c) => c.PartyDate).NotNull<Birthday, DateTime>().WithMessage<Birthday, DateTime>("Party date is is required.");
			base.RuleFor<long>((Birthday c) => c.InstructorId).NotNull<Birthday, long>().WithMessage<Birthday, long>("Instructor is is required.");
			base.RuleFor<long?>((Birthday c) => c.AssistantId).NotEqual<Birthday, long?>((Birthday c) => (long?)c.InstructorId, null).WithMessage<Birthday, long?>("Instructor and assistant must be different.");
			base.RuleFor<TimeSpan>((Birthday c) => c.PartyTime).NotNull<Birthday, TimeSpan>().WithMessage<Birthday, TimeSpan>("Party time is is required.");
			base.RuleFor<TimeSpan>((Birthday c) => c.PartyTime).GreaterThanOrEqualTo<Birthday, TimeSpan>(new TimeSpan(5, 0, 0)).WithMessage<Birthday, TimeSpan>("Party time must be from 5AM to 11PM.").LessThanOrEqualTo<Birthday, TimeSpan>(new TimeSpan(23, 0, 0)).WithMessage<Birthday, TimeSpan>("Party time must be from 5AM to 11PM.");
		}
	}
}