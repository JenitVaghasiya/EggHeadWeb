using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(AdminPaymentInfoValidatior))]
	public class AdminPaymentInfo
	{
		public virtual EggheadWeb.Models.Common.Admin Admin
		{
			get;
			set;
		}

		public long AdminId
		{
			get;
			set;
		}

		public string APILoginID
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public DateTime? LastUpdateDate
		{
			get;
			set;
		}

		public string MD5HashPhrase
		{
			get;
			set;
		}

		public string TransactionKey
		{
			get;
			set;
		}

		public AdminPaymentInfo()
		{
		}
	}
}