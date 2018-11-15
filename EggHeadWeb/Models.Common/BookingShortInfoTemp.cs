using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class BookingShortInfoTemp
	{
		public long? classId
		{
			get;
			set;
		}

		public decimal? Cost
		{
			get;
			set;
		}

		public long? CouponId
		{
			get;
			set;
		}

		public string Dates
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string ServiceType
		{
			get;
			set;
		}

		public long? StudentId
		{
			get;
			set;
		}

		public BookingShortInfoTemp()
		{
		}
	}
}