using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class CouponValidator : AbstractValidator<Coupon>
	{
		public CouponValidator()
		{
			base.RuleFor<string>((Coupon c) => c.Code).NotEmpty<Coupon, string>().WithMessage<Coupon, string>("Coupon code is is required.");
			base.RuleFor<string>((Coupon c) => c.Description).NotEmpty<Coupon, string>().WithMessage<Coupon, string>("Description is is required.");
			base.RuleFor<byte>((Coupon c) => c.Type).NotNull<Coupon, byte>().WithMessage<Coupon, byte>("Type is is required.");
			base.RuleFor<DateTime?>((Coupon c) => c.NExpDate).NotNull<Coupon, DateTime?>().WithMessage<Coupon, DateTime?>("Exp Date is is required.");
			base.RuleFor<decimal?>((Coupon c) => c.DiscountAmount).NotNull<Coupon, decimal?>().WithMessage<Coupon, decimal?>("Amount is is required.").GreaterThan<Coupon, decimal>(new decimal(0)).WithMessage<Coupon, decimal?>("Amount must greater than 0.").LessThanOrEqualTo<Coupon, decimal>(new decimal(100)).When<Coupon, decimal?>((Coupon c) => c.Type == 2, ApplyConditionTo.AllValidators).WithMessage<Coupon, decimal?>("Amount must not be greater than 100.");
			base.RuleFor<int>((Coupon c) => c.MaxAvailable).NotNull<Coupon, int>().WithMessage<Coupon, int>("Availabe number is is required.").GreaterThan<Coupon, int>(0).WithMessage<Coupon, int>("Availabe number must greater than 0.");
			base.RuleFor<int>((Coupon c) => c.MaxUsesPerCustomer).NotNull<Coupon, int>().WithMessage<Coupon, int>("Max uses per customer is is required.").GreaterThan<Coupon, int>(0).WithMessage<Coupon, int>("Max uses per customer must greater than 0").LessThanOrEqualTo<Coupon, int>((Coupon c) => c.MaxAvailable).WithMessage<Coupon, int>("Max uses per customer must be less than or equal to availabe number");
		}
	}
}