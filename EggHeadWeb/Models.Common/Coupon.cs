using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(CouponValidator))]
	public class Coupon
	{
		public virtual EggheadWeb.Models.Common.Admin Admin
		{
			get;
			set;
		}

		public long? AdminId
		{
			get;
			set;
		}

		public virtual ICollection<Booking> Bookings
		{
			get;
			set;
		}

		public string Code
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public decimal? DiscountAmount
		{
			get;
			set;
		}

		public DateTime ExpDate
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public DateTime? LastUsedDate
		{
			get;
			set;
		}

		public int MaxAvailable
		{
			get;
			set;
		}

		public int MaxUsesPerCustomer
		{
			get;
			set;
		}

		public DateTime? NExpDate
		{
			get;
			set;
		}

		public byte Type
		{
			get;
			set;
		}

		public Coupon()
		{
			this.Bookings = new HashSet<Booking>();
		}
	}
}