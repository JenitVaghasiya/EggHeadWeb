using EggheadWeb.Models.Common;
using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	[Validator(typeof(InstructorItemValidator))]
	public class InstructorItem1
	{
		public string Address
		{
			get;
			set;
		}

		public long AreaId
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public long CreatedBy
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string Gender
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public string Note
		{
			get;
			set;
		}

		public string Pay
		{
			get;
			set;
		}

		public string PhoneNumber
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public string Zip
		{
			get;
			set;
		}

		public InstructorItem1()
		{
		}
	}
}