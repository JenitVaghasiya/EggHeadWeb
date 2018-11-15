using EggheadWeb.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.UserModels
{
	public class StudentItem
	{
		[Display(Name="BirthDate")]
		[TrippleDDLDateTime(ErrorMessage="* Please input valid Birth Date.")]
		public DateTime? BirthDate
		{
			get;
			set;
		}

		[Display(Name="First Name")]
		[Required(ErrorMessage="* Please input first name.")]
		public string FirstName
		{
			get;
			set;
		}

		[Display(Name="Gender")]
		public string Gender
		{
			get;
			set;
		}

		[Display(Name="Grade")]
		[Required(ErrorMessage="* Please select grade.")]
		[UIHint("GradeDropDown")]
		public byte GradeId
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		[Display(Name="Last Name")]
		[Required(ErrorMessage="* Please input last name.")]
		public string LastName
		{
			get;
			set;
		}

		[Display(Name="Special Notes")]
		public string Notes
		{
			get;
			set;
		}

		public long ParentId
		{
			get;
			set;
		}

		public StudentItem()
		{
		}
	}
}