using MVCControlsToolkit.Core;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace EggheadWeb.Models.Common
{
	public static class ChoiceLists
	{
		public static ChoiceList<Grade, int, string> Grades(EggheadEntities db)
		{
			return new ChoiceList<Grade, int, string>(
				from g in db.Grades
				orderby g.Id
				select g, (Grade g) => g.Id, (Grade g) => g.Name, null, null, true, null);
		}
	}
}