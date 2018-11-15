using EggheadWeb.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class StudentAttrs
	{
		[TrippleDDLDateTime(ErrorMessage="* Please input BirthDate.")]
		public DateTime BirthDate
		{
			get;
			set;
		}

		public StudentAttrs()
		{
		}
	}
}