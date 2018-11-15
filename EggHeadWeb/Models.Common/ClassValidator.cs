using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class ClassValidator : AbstractValidator<Class>
	{
		public ClassValidator()
		{
			base.RuleFor<bool>((Class c) => c.CanRegistOnline).NotNull<Class, bool>().WithMessage<Class, bool>("Regist Online flag is required.");
			base.RuleFor<long>((Class c) => c.InstructorId).NotNull<Class, long>().WithMessage<Class, long>("Main instructor is required.");
			base.RuleFor<bool>((Class c) => c.IsOpen).NotNull<Class, bool>().WithMessage<Class, bool>("Is open flag is required.");
			base.RuleFor<long>((Class c) => c.LocationId).NotNull<Class, long>().WithMessage<Class, long>("Location is required.");
			base.RuleFor<string>((Class c) => c.Name).NotEmpty<Class, string>().WithMessage<Class, string>("Name is required.");
			base.RuleFor<string>((Class c) => c.OnlineName).NotEmpty<Class, string>().WithMessage<Class, string>("Online name is required.");
			base.RuleFor<long?>((Class c) => c.AssistantId).NotEqual<Class, long?>((Class c) => (long?)c.InstructorId, null).WithMessage<Class, long?>("Instructor and assistant must be different.");
			base.RuleFor<string>((Class c) => c.OnlineDescription).NotEmpty<Class, string>().When<Class, string>((Class t) => t.CanRegistOnline, ApplyConditionTo.AllValidators).WithMessage<Class, string>("Online description is required.");
			base.RuleFor<string>((Class c) => c.Dates).NotNull<Class, string>().WithMessage<Class, string>("Dates are required.").When<Class, string>((Class c) => c.Id <= (long)0, ApplyConditionTo.AllValidators);
			// TODO base.RuleFor<List<byte>>((Class c) => c.GradeIds).NotNull<Class, List<byte>>().WithMessage<Class, List<byte>>("Grades are required.").Must<Class, List<byte>>((List<byte> a) => a.Count > 0).WithMessage<Class, List<byte>>("Must select at least one grade");
			base.RuleFor<decimal?>((Class c) => c.NCost).NotNull<Class, decimal?>().WithMessage<Class, decimal?>("Cost is required.").GreaterThan<Class, decimal>(new decimal(0)).WithMessage<Class, decimal?>("Cost must be positive");
			base.RuleFor<DateTime?>((Class c) => c.NDisplayUntil).NotNull<Class, DateTime?>().When<Class, DateTime?>((Class t) => t.CanRegistOnline, ApplyConditionTo.AllValidators).WithMessage<Class, DateTime?>("Display until is required.");
			base.RuleFor<TimeSpan>((Class c) => c.TimeEnd).NotNull<Class, TimeSpan>().WithMessage<Class, TimeSpan>("End time is required.");
			base.RuleFor<TimeSpan>((Class c) => c.TimeEnd).GreaterThanOrEqualTo<Class, TimeSpan>(new TimeSpan(5, 0, 0)).WithMessage<Class, TimeSpan>("End time must be from 5AM to 11PM.").LessThanOrEqualTo<Class, TimeSpan>(new TimeSpan(23, 0, 0)).WithMessage<Class, TimeSpan>("End time must be from 5AM to 11PM.");
			base.RuleFor<TimeSpan>((Class c) => c.TimeStart).NotNull<Class, TimeSpan>().WithMessage<Class, TimeSpan>("Start time is required.");
			base.RuleFor<TimeSpan>((Class c) => c.TimeStart).GreaterThanOrEqualTo<Class, TimeSpan>(new TimeSpan(5, 0, 0)).WithMessage<Class, TimeSpan>("Start time must be from 5AM to 11PM.").LessThanOrEqualTo<Class, TimeSpan>(new TimeSpan(23, 0, 0)).WithMessage<Class, TimeSpan>("Start time must be from 5AM to 11PM.");
			base.RuleFor<TimeSpan>((Class c) => c.TimeEnd).Must<Class, TimeSpan>((TimeSpan c) => false).WithMessage<Class, TimeSpan>("End time must be after start time").When<Class, TimeSpan>((Class x) => TimeSpan.Compare(x.TimeStart, x.TimeEnd) > 0, ApplyConditionTo.AllValidators);
			base.RuleFor<int?>((Class c) => c.NMaxEnroll).NotNull<Class, int?>().WithMessage<Class, int?>("Max enroll is required.").GreaterThan<Class, int>(0).WithMessage<Class, int?>("Max enroll must be positive");
		}
	}
}