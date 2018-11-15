using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.UserModels
{
	public class BookingShortInfo
	{
		public decimal Cost
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

		public ServiceType Type
		{
			get;
			set;
		}

		public BookingShortInfo()
		{
		}
	}
}