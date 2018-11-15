using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class GradeGroup
	{
		public virtual ICollection<Camp> Camps
		{
			get;
			set;
		}

		public virtual ICollection<Class> Classes
		{
			get;
			set;
		}

		public virtual ICollection<Grade> Grades
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

		public virtual ICollection<Workshop> Workshops
		{
			get;
			set;
		}

		public GradeGroup()
		{
			this.Grades = new HashSet<Grade>();
			this.Workshops = new HashSet<Workshop>();
			this.Camps = new HashSet<Camp>();
			this.Classes = new HashSet<Class>();
		}
	}
}