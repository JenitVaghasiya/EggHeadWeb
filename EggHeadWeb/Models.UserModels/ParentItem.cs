using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.UserModels
{
	public class ParentItem
	{
		[Display(Name="Address")]
		[Required(ErrorMessage="* Please input address.")]
		public string Address
		{
			get;
			set;
		}

		public List<StudentItem> Children
		{
			get;
			set;
		}

		[Display(Name="City")]
		[Required(ErrorMessage="* Please input city.")]
		public string City
		{
			get;
			set;
		}

		[RegularExpression("^([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$", ErrorMessage="* Please enter a valid email address")]
		[Required(ErrorMessage="* Please enter an email address")]
		public string Email
		{
			get;
			set;
		}

		[Display(Name="First Name")]
		[Required(ErrorMessage="* Please input first name.")]
		public string FirstName
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		[Display(Name="Last Name")]
		[Required(ErrorMessage="* Please input last name.")]
		public string LastName
		{
			get;
			set;
		}

		[Required(ErrorMessage="* Please select location")]
		public long LocationId
		{
			get;
			set;
		}

		public string LocationName
		{
			get;
			set;
		}

		public string Note
		{
			get;
			set;
		}

		[DataType(DataType.Password)]
		[RegularExpression("^[!@#$%^&*_+-/a-zA-Z0-9]{6,18}$", ErrorMessage="* Please enter a valid password")]
		[Required(ErrorMessage="* Please enter password")]
		public string Password
		{
			get;
			set;
		}

		[Display(Name="Phone Number")]
		[Required(ErrorMessage="* Please input phone number.")]
		public string PhoneNumer
		{
			get;
			set;
		}

		[Display(Name="State")]
		[Required(ErrorMessage="* Please input state.")]
		public string State
		{
			get;
			set;
		}

		[Display(Name="Zip")]
		[Required(ErrorMessage="* Please input zip.")]
		public string Zip
		{
			get;
			set;
		}

		public ParentItem()
		{
		}
	}
}