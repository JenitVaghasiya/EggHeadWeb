using EggheadWeb.Models.Common;
using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	[Validator(typeof(BirthdayValidator))]
	public class BirthdayItem
	{
		public string Address
		{
			get;
			set;
		}

		public int Age
		{
			get;
			set;
		}

		public long AssistantId
		{
			get;
			set;
		}

		public string ChildName
		{
			get;
			set;
		}

		public string ContactNumber
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public long InstructorId
		{
			get;
			set;
		}

		public string Notes
		{
			get;
			set;
		}

		public string ParentName
		{
			get;
			set;
		}

		public DateTime PartyDate
		{
			get;
			set;
		}

		public TimeSpan PartyTime
		{
			get;
			set;
		}

		public BirthdayItem()
		{
		}
	}
}