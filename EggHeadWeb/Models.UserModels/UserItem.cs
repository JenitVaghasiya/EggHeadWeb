using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.UserModels
{
	public class UserItem
	{
		public ParentItem Parent
		{
			get;
			set;
		}

		public List<StudentItem> Students
		{
			get;
			set;
		}

		public UserItem()
		{
		}
	}
}