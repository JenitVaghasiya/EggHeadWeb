using EggHeadWeb.DatabaseContext;
using FluentValidation;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class AreaValidator : AbstractValidator<Area>
	{
		public AreaValidator()
		{
			base.RuleFor<string>((Area a) => a.DisplayName).NotEmpty<Area, string>().WithMessage<Area, string>("Name is required.");
			base.RuleFor<string>((Area a) => a.Name).NotEmpty<Area, string>().WithMessage<Area, string>("URL Name is required.");
			base.RuleFor<string>((Area a) => a.Name).Must<Area, string>((string k) => !k.Contains(" ")).WithMessage<Area, string>("URL Name cant not contains space character.");
			base.RuleFor<string>((Area a) => a.State).NotEmpty<Area, string>().WithMessage<Area, string>("State is required.");
		}
	}
}