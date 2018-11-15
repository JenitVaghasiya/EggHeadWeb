using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class LoginForm
	{
		[Display(Name="Password")]
		[Required(ErrorMessage="Please enter a valid password")]
		public string Password
		{
			get;
			set;
		}

		public string ReturnUrl
		{
			get;
			set;
		}

		[Display(Name="Remember me")]
		public bool SaveLogin
		{
			get;
			set;
		}

		[Display(Name="User name")]
		[Required(ErrorMessage="Please enter a valid username")]
		public string Username
		{
			get;
			set;
		}

		public LoginForm()
		{
		}
	}
}