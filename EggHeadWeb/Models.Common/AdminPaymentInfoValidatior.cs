using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class AdminPaymentInfoValidatior : AbstractValidator<AdminPaymentInfo>
	{
		public AdminPaymentInfoValidatior()
		{
			base.RuleFor<string>((AdminPaymentInfo c) => c.APILoginID).NotEmpty<AdminPaymentInfo, string>().WithMessage<AdminPaymentInfo, string>("User name is required.");
			base.RuleFor<string>((AdminPaymentInfo c) => c.TransactionKey).NotEmpty<AdminPaymentInfo, string>().WithMessage<AdminPaymentInfo, string>("Password is required.");
			base.RuleFor<string>((AdminPaymentInfo c) => c.MD5HashPhrase).NotEmpty<AdminPaymentInfo, string>().WithMessage<AdminPaymentInfo, string>("Signature is required.");
		}
	}
}