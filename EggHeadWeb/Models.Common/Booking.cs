using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(BookingFormFormValidator))]
	public class Booking
	{
		public virtual EggheadWeb.Models.Common.Birthday Birthday
		{
			get;
			set;
		}

		public long? BirthdayId
		{
			get;
			set;
		}

		public DateTime BookDate
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Camp Camp
		{
			get;
			set;
		}

		public long? CampId
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Class Class
		{
			get;
			set;
		}

		public long? ClassId
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Coupon Coupon
		{
			get;
			set;
		}

		public long? CouponId
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Student Student
		{
			get;
			set;
		}

		public long StudentId
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Workshop Workshop
		{
			get;
			set;
		}

		public long? WorkshopId
		{
			get;
			set;
		}

		public Booking()
		{
		}
	}
}