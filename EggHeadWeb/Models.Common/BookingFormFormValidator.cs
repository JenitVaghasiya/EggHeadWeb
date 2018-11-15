using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class BookingFormFormValidator : AbstractValidator<Booking>
	{
		public BookingFormFormValidator()
		{
			base.RuleFor<long>((Booking a) => a.StudentId).GreaterThan<Booking, long>((long)0).WithMessage<Booking, long>("Student is required.");
		}
	}
}