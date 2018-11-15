using EggheadWeb.Models.Common;
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
			return (new EggheadEntities()).APVariables.ToList<APVariable>();
		}
	}
}