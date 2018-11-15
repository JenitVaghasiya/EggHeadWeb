using EggHeadWeb.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EggheadWeb.Common
{
	public class ApVariableUtil
	{
		public ApVariableUtil()
		{
		}

		public static List<APVariable> GetApVariables()
		{
            using (var db = new EggheadContext())
            {
                return db.APVariables.ToList();
            }
		}
	}
}