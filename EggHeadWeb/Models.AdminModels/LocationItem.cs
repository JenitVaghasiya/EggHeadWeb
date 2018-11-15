using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class LocationItem
	{
		[Required(ErrorMessage="Address is required.")]
		public string Address
		{
			get;
			set;
		}

		public long AreaId
		{
			get;
			set;
		}

		public bool CanRegistOnline
		{
			get;
			set;
		}

		[Required(ErrorMessage="Contact person is required.")]
		public string ContactPerson
		{
			get;
			set;
		}

		[Required(ErrorMessage="Display is required.")]
		public string DisplayName
		{
			get;
			set;
		}

		[Required(ErrorMessage="Email is required.")]
		public string Email
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		[Required(ErrorMessage="Name is required.")]
		public string Name
		{
			get;
			set;
		}

		public string Note
		{
			get;
			set;
		}

		[Required(ErrorMessage="PhoneNumber is required.")]
		public string PhoneNumber
		{
			get;
			set;
		}

		public LocationItem()
		{
		}
	}
}