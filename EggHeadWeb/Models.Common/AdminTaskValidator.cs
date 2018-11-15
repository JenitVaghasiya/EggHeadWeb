using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class AdminTaskValidator : AbstractValidator<AdminTask>
	{
		public AdminTaskValidator()
		{
			base.RuleFor<string>((AdminTask a) => a.Name).NotEmpty<AdminTask, string>().WithMessage<AdminTask, string>("Name is required.");
			base.RuleFor<byte>((AdminTask a) => a.Priority).NotNull<AdminTask, byte>().WithMessage<AdminTask, byte>("Priority is required.");
		}
	}
}