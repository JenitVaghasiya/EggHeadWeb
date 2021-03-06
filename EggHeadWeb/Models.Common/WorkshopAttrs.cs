using EggheadWeb.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	public class WorkshopAttrs
	{
		[AllowHtml]
		public string Notes
		{
			get;
			set;
		}

		[TrippleDDLTimeSpan(ErrorMessage="Please select valid time.")]
		public TimeSpan TimeEnd
		{
			get;
			set;
		}

		[TrippleDDLTimeSpan(ErrorMessage="Please select valid time.")]
		public TimeSpan TimeStart
		{
			get;
			set;
		}

		public WorkshopAttrs()
		{
		}
	}
}