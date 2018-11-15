using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class ChangePasswordForm
	{
		[RegularExpression("^[!@#$%^&*_+-/a-zA-Z0-9]{1,100}$", ErrorMessage="* Please enter a valid password.")]
		[Required(ErrorMessage="* Please enter new password.")]
		public string NewPassword
		{
			get;
			set;
		}

		[RegularExpression("^[!@#$%^&*_+-/a-zA-Z0-9]{1,100}$", ErrorMessage="* Please enter a valid password.")]
		[Required(ErrorMessage="* Please enter old password.")]
		public string OldPassword
		{
			get;
			set;
		}

		public ChangePasswordForm()
		{
		}
	}
}