using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class AssignValidator : AbstractValidator<Assign>
	{
		public AssignValidator()
		{
			base.RuleFor<long>((Assign c) => c.InstructorId).NotEmpty<Assign, long>().WithMessage<Assign, long>("Instructor is required.");
			base.RuleFor<long?>((Assign c) => c.AssistantId).NotEqual<Assign, long?>((Assign c) => (long?)c.InstructorId, null).WithMessage<Assign, long?>("Instructor and assistant must be different");
		}
	}
}