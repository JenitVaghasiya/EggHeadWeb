using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.DAL
{
	public class ClassRepository : GenericRepository<Class>
	{
		public ClassRepository(EggheadEntities context) : base(context)
		{
		}

		public List<Class> GetClassEnrolledByParent(long parentId)
		{
			return (
				from t in this.context.Classes
				where t.Bookings.Any<Booking>((Booking l) => l.Student.ParentId == parentId)
				select t).ToList<Class>();
		}

		public List<Class> GetEnrollableClassesOfParent(Parent parent)
		{
			IQueryable<Class> query = 
				from t in this.context.Classes
				where t.Location.AreaId == parent.Location.AreaId && t.CanRegistOnline && t.DisplayUntil.HasValue && (t.DisplayUntil.Value >= DateTime.Today) && (t.Assigns.Max<Assign, DateTime>((Assign x) => x.Date) >= DateTime.Today)
				select t;
			return (
				from t in query
				where t.LocationId == parent.LocationId || t.IsOpen
				select t).ToList<Class>();
		}
	}
}