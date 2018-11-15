using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.DAL
{
	public class CampRepository : GenericRepository<Camp>
	{
		public CampRepository(EggheadEntities context) : base(context)
		{
		}

		public List<Camp> GetCampEnrolledByParent(long parentId)
		{
			return (
				from t in this.context.Camps
				where t.Bookings.Any<Booking>((Booking l) => l.Student.ParentId == parentId)
				select t).ToList<Camp>();
		}

		public List<Camp> GetEnrollableCampsOfParent(Parent parent)
		{
			IQueryable<Camp> query = 
				from t in this.context.Camps
				where t.Location.AreaId == parent.Location.AreaId && t.CanRegistOnline && t.DisplayUntil.HasValue && (t.DisplayUntil.Value >= DateTime.Today) && (t.Assigns.Max<Assign, DateTime>((Assign x) => x.Date) >= DateTime.Today)
				select t;
			return (
				from t in query
				where t.LocationId == parent.LocationId || t.IsOpen
				select t).ToList<Camp>();
		}
	}
}