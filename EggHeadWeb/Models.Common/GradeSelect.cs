using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class GradeSelect
	{
		public long GradeId
		{
			get;
			set;
		}

		public string GradeName
		{
			get;
			set;
		}

		public bool Value
		{
			get;
			set;
		}

		public GradeSelect()
		{
		}
	}
}