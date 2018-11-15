using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class PrivateMessageValidatior : AbstractValidator<PrivateMessage>
	{
		public PrivateMessageValidatior()
		{
			base.RuleFor<long?>((PrivateMessage c) => c.FromAdminId).NotNull<PrivateMessage, long?>().WithMessage<PrivateMessage, long?>("Sender is required.");
			base.RuleFor<long?>((PrivateMessage c) => c.ToAdminId).NotNull<PrivateMessage, long?>().WithMessage<PrivateMessage, long?>("Receiver is required.");
			base.RuleFor<string>((PrivateMessage c) => c.MessageSubject).NotEmpty<PrivateMessage, string>().WithMessage<PrivateMessage, string>("Message subject is required.");
			base.RuleFor<string>((PrivateMessage c) => c.MessageContent).NotEmpty<PrivateMessage, string>().WithMessage<PrivateMessage, string>("Message content is required.");
		}
	}
}