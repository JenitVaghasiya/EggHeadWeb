using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class CouponBookingInfo
	{
		public DateTime BookDate
		{
			get;
			set;
		}

		public string Camps
		{
			get;
			set;
		}

		public string ClassCamps
		{
			get;
			set;
		}

		public string Classes
		{
			get;
			set;
		}

		public EggheadWeb.Models.Common.Parent Parent
		{
			get;
			set;
		}

		public CouponBookingInfo()
		{
		}
	}
}