using EggheadWeb.Models.Common;
using EggHeadWeb.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class RosterDetailPdf : List<string[]>
	{
		public string Address
		{
			get;
			set;
		}

		public HashSet<Booking> Bookings
		{
			get;
			set;
		}

		public string ClassName
		{
			get;
			set;
		}

		public string[] Dates
		{
			get;
			set;
		}

		public string Days
		{
			get;
			set;
		}

		public string ExportFileName
		{
			get;
			set;
		}

		public List<string> Headers
		{
			get;
			set;
		}

		public string LocationName
		{
			get;
			set;
		}

		public string LogoUrl
		{
			get;
			set;
		}

		public string Printed
		{
			get;
			set;
		}

		public string Time
		{
			get;
			set;
		}

		public ServiceType Type
		{
			get;
			set;
		}

		public RosterDetailPdf()
		{
		}
	}
}