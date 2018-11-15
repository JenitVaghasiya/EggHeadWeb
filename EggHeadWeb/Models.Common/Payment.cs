using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class Payment
	{
		public string AdminAmount
		{
			get;
			set;
		}

		public long? AdminId
		{
			get;
			set;
		}

		public long? AdminParentId
		{
			get;
			set;
		}

		public decimal Amount
		{
			get;
			set;
		}

		public string AuthCode
		{
			get;
			set;
		}

		public string Bill_Address
		{
			get;
			set;
		}

		public string Bill_City
		{
			get;
			set;
		}

		public string Bill_Email
		{
			get;
			set;
		}

		public string Bill_FirstName
		{
			get;
			set;
		}

		public string Bill_LastName
		{
			get;
			set;
		}

		public string Bill_State
		{
			get;
			set;
		}

		public string Bill_Zip
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public long ParentId
		{
			get;
			set;
		}

		public DateTime? PaymentDate
		{
			get;
			set;
		}

		public string PaymentMessage
		{
			get;
			set;
		}

		public string ServiceName
		{
			get;
			set;
		}

		public string TransactionID
		{
			get;
			set;
		}

		public Payment()
		{
		}
	}
}