using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Utility
{
	public class PayPalOrder
	{
		public string Address
		{
			get;
			set;
		}

		public decimal Amount
		{
			get;
			set;
		}

		public string CancelUrl
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public bool OverrideAddress
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public string PayerID
		{
			get;
			set;
		}

		public string PhoneNumer
		{
			get;
			set;
		}

		public string ReturnUrl
		{
			get;
			set;
		}

		public string Signature
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public string Token
		{
			get;
			set;
		}

		public string UserName
		{
			get;
			set;
		}

		public string Zip
		{
			get;
			set;
		}

		public PayPalOrder()
		{
		}
	}
}