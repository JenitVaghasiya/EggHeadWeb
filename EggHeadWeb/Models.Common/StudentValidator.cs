using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class StudentValidator : AbstractValidator<Student>
	{
		public StudentValidator()
		{
			base.RuleFor<string>((Student s) => s.Gender).NotEmpty<Student, string>().WithMessage<Student, string>("Gender is required.");
			base.RuleFor<byte>((Student s) => s.GradeId).NotNull<Student, byte>().WithMessage<Student, byte>("Grade is required.");
			base.RuleFor<string>((Student s) => s.FirstName).NotEmpty<Student, string>().WithMessage<Student, string>("First name is required.");
			base.RuleFor<string>((Student s) => s.LastName).NotEmpty<Student, string>().WithMessage<Student, string>("Last name is required.");
		}
	}
}