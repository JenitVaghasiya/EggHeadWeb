using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class CampValidator : AbstractValidator<Camp>
	{
		public CampValidator()
		{
			base.RuleFor<bool>((Camp c) => c.CanRegistOnline).NotNull<Camp, bool>().WithMessage<Camp, bool>("Regist Online flag is required.");
			base.RuleFor<long>((Camp c) => c.InstructorId).NotNull<Camp, long>().WithMessage<Camp, long>("Main instructor is required.");
			base.RuleFor<bool>((Camp c) => c.IsOpen).NotNull<Camp, bool>().WithMessage<Camp, bool>("Is open flag is required.");
			base.RuleFor<long>((Camp c) => c.LocationId).NotNull<Camp, long>().WithMessage<Camp, long>("Location is required.");
			base.RuleFor<string>((Camp c) => c.Name).NotEmpty<Camp, string>().WithMessage<Camp, string>("Name is required.");
			base.RuleFor<string>((Camp c) => c.OnlineName).NotEmpty<Camp, string>().WithMessage<Camp, string>("Online name is required.");
			base.RuleFor<long?>((Camp c) => c.AssistantId).NotEqual<Camp, long?>((Camp c) => (long?)c.InstructorId, null).WithMessage<Camp, long?>("Instructor and assistant must be different.");
			base.RuleFor<string>((Camp c) => c.OnlineDescription).NotEmpty<Camp, string>().When<Camp, string>((Camp t) => t.CanRegistOnline, ApplyConditionTo.AllValidators).WithMessage<Camp, string>("Online description is required.");
			base.RuleFor<string>((Camp c) => c.Dates).NotNull<Camp, string>().WithMessage<Camp, string>("Dates are required.").When<Camp, string>((Camp c) => c.Id <= (long)0, ApplyConditionTo.AllValidators);
			// TODO base.RuleFor<List<int>>((Camp c) => c.GradeIds).NotNull<Camp, List<int>>().WithMessage<Camp, List<int>("Grades are required.").Must<Camp, List<byte>>((List<byte> a) => a.Count > 0).WithMessage<Camp, List<byte>>("Must select at least one grade");
			base.RuleFor<decimal?>((Camp c) => c.NCost).NotNull<Camp, decimal?>().WithMessage<Camp, decimal?>("Cost is required.").GreaterThan<Camp, decimal>(new decimal(0)).WithMessage<Camp, decimal?>("Cost must be positive");
			base.RuleFor<DateTime?>((Camp c) => c.NDisplayUntil).NotNull<Camp, DateTime?>().When<Camp, DateTime?>((Camp t) => t.CanRegistOnline, ApplyConditionTo.AllValidators).WithMessage<Camp, DateTime?>("Display until is required.");
			base.RuleFor<TimeSpan>((Camp c) => c.TimeEnd).NotNull<Camp, TimeSpan>().WithMessage<Camp, TimeSpan>("End time is required.");
			base.RuleFor<TimeSpan>((Camp c) => c.TimeEnd).GreaterThanOrEqualTo<Camp, TimeSpan>(new TimeSpan(5, 0, 0)).WithMessage<Camp, TimeSpan>("End time must be from 5AM to 11PM.").LessThanOrEqualTo<Camp, TimeSpan>(new TimeSpan(23, 0, 0)).WithMessage<Camp, TimeSpan>("End time must be from 5AM to 11PM.");
			base.RuleFor<TimeSpan>((Camp c) => c.TimeStart).NotNull<Camp, TimeSpan>().WithMessage<Camp, TimeSpan>("Start time is required.");
			base.RuleFor<TimeSpan>((Camp c) => c.TimeStart).GreaterThanOrEqualTo<Camp, TimeSpan>(new TimeSpan(5, 0, 0)).WithMessage<Camp, TimeSpan>("Start time must be from 5AM to 11PM.").LessThanOrEqualTo<Camp, TimeSpan>(new TimeSpan(23, 0, 0)).WithMessage<Camp, TimeSpan>("Start time must be from 5AM to 11PM.");
			base.RuleFor<TimeSpan>((Camp c) => c.TimeEnd).Must<Camp, TimeSpan>((TimeSpan c) => false).WithMessage<Camp, TimeSpan>("End time must be after start time").When<Camp, TimeSpan>((Camp x) => TimeSpan.Compare(x.TimeStart, x.TimeEnd) > 0, ApplyConditionTo.AllValidators);
			base.RuleFor<int?>((Camp c) => c.NMaxEnroll).NotNull<Camp, int?>().WithMessage<Camp, int?>("Max enroll is required.").GreaterThan<Camp, int>(0).WithMessage<Camp, int?>("Max enroll must be positive");
		}
	}
}