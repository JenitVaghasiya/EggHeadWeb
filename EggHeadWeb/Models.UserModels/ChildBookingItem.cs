using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.UserModels
{
	public class ChildBookingItem
	{
		public List<BookingShortInfo> Booking
		{
			get;
			set;
		}

		public Student Child
		{
			get;
			set;
		}

		public ChildBookingItem()
		{
		}
	}
}