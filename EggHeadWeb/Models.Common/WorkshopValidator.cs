using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class WorkshopValidator : AbstractValidator<Workshop>
	{
		public WorkshopValidator()
		{
			base.RuleFor<long>((Workshop c) => c.InstructorId).NotNull<Workshop, long>().WithMessage<Workshop, long>("Main instructor is required.");
			base.RuleFor<long>((Workshop c) => c.LocationId).NotNull<Workshop, long>().WithMessage<Workshop, long>("Location is required.");
			base.RuleFor<string>((Workshop c) => c.Name).NotEmpty<Workshop, string>().WithMessage<Workshop, string>("Name is required.");
			base.RuleFor<long?>((Workshop c) => c.AssistantId).NotEqual<Workshop, long?>((Workshop c) => (long?)c.InstructorId, null).WithMessage<Workshop, long?>("Instructor and assistant must be different.");
			base.RuleFor<string>((Workshop c) => c.Dates).NotNull<Workshop, string>().WithMessage<Workshop, string>("Dates are required.").When<Workshop, string>((Workshop c) => c.Id <= (long)0, ApplyConditionTo.AllValidators);
			// TODO base.RuleFor<List<byte>>((Workshop c) => c.GradeIds).NotNull<Workshop, List<byte>>().WithMessage<Workshop, List<byte>>("Grades are required.").Must<Workshop, List<byte>>((List<byte> a) => a.Count > 0).WithMessage<Workshop, List<byte>>("Must select at least one grade.");
			base.RuleFor<decimal?>((Workshop c) => c.NCost).NotNull<Workshop, decimal?>().WithMessage<Workshop, decimal?>("Cost is required.").GreaterThan<Workshop, decimal>(new decimal(0)).WithMessage<Workshop, decimal?>("Cost must be positive");
			base.RuleFor<TimeSpan>((Workshop c) => c.TimeEnd).NotNull<Workshop, TimeSpan>().WithMessage<Workshop, TimeSpan>("End time is required.");
			base.RuleFor<TimeSpan>((Workshop c) => c.TimeStart).NotNull<Workshop, TimeSpan>().WithMessage<Workshop, TimeSpan>("Start time is required.");
			base.RuleFor<TimeSpan>((Workshop c) => c.TimeEnd).GreaterThanOrEqualTo<Workshop, TimeSpan>(new TimeSpan(5, 0, 0)).WithMessage<Workshop, TimeSpan>("End time must be from 5AM to 11PM.").LessThanOrEqualTo<Workshop, TimeSpan>(new TimeSpan(23, 0, 0)).WithMessage<Workshop, TimeSpan>("End time must be from 5AM to 11PM.");
			base.RuleFor<TimeSpan>((Workshop c) => c.TimeStart).GreaterThanOrEqualTo<Workshop, TimeSpan>(new TimeSpan(5, 0, 0)).WithMessage<Workshop, TimeSpan>("Start time must be from 5AM to 11PM.").LessThanOrEqualTo<Workshop, TimeSpan>(new TimeSpan(23, 0, 0)).WithMessage<Workshop, TimeSpan>("Start time must be from 5AM to 11PM.");
			base.RuleFor<TimeSpan>((Workshop c) => c.TimeEnd).Must<Workshop, TimeSpan>((TimeSpan c) => false).WithMessage<Workshop, TimeSpan>("End time must be after start time.").When<Workshop, TimeSpan>((Workshop x) => TimeSpan.Compare(x.TimeStart, x.TimeEnd) > 0, ApplyConditionTo.AllValidators);
		}
	}
}