using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(GaderValidator))]
	public class Grade
	{
		public virtual ICollection<GradeGroup> GradeGroups
		{
			get;
			set;
		}

		public int Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public virtual ICollection<Student> Students
		{
			get;
			set;
		}

		public Grade()
		{
			this.Students = new HashSet<Student>();
			this.GradeGroups = new HashSet<GradeGroup>();
		}
	}
}