using EggHeadWeb.DatabaseContext;
using System.Collections.Generic;
using System.Linq;

namespace EggheadWeb.Common
{
    public class GradeUtil
    {
        public static IEnumerable<object> Grades
        {
            get
            {
                var grades = (
                    from t in (new EggheadContext()).Grades
                    select new { ID = (int)t.Id, Name = t.Name }).ToList();
                grades.Insert(0, new { ID = 0, Name = string.Empty });
                return grades;
            }
        }

        public GradeUtil()
        {
        }
    }
}