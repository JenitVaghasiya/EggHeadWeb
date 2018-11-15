using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public class LocationValidator : AbstractValidator<Location>
	{
		public LocationValidator()
		{
			base.RuleFor<string>((Location l) => l.Address).NotEmpty<Location, string>().WithMessage<Location, string>("Address is required.");
			base.RuleFor<long>((Location l) => l.AreaId).NotEmpty<Location, long>().WithMessage<Location, long>("Area is required.");
			base.RuleFor<bool>((Location l) => l.CanRegistOnline).NotNull<Location, bool>();
			base.RuleFor<string>((Location l) => l.ContactPerson).NotEmpty<Location, string>().WithMessage<Location, string>("Contact person is required.");
			base.RuleFor<string>((Location l) => l.DisplayName).NotEmpty<Location, string>().WithMessage<Location, string>("Display name is required.");
			base.RuleFor<string>((Location l) => l.Email).NotEmpty<Location, string>().WithMessage<Location, string>("Email is required.").EmailAddress<Location>().WithMessage<Location, string>("Email is not valid");
			base.RuleFor<bool>((Location l) => l.IsActive).NotNull<Location, bool>();
			base.RuleFor<string>((Location l) => l.Name).NotEmpty<Location, string>().WithMessage<Location, string>("Name is required.");
			base.RuleFor<string>((Location l) => l.PhoneNumber).NotEmpty<Location, string>().WithMessage<Location, string>("Phone number is required.");
			base.RuleFor<string>((Location l) => l.City).NotEmpty<Location, string>().WithMessage<Location, string>("City is required.");
			base.RuleFor<string>((Location l) => l.Zip).NotEmpty<Location, string>().WithMessage<Location, string>("Zip is required.");
			base.RuleFor<string>((Location l) => l.State).NotEmpty<Location, string>().WithMessage<Location, string>("State is required.");
		}
	}
}