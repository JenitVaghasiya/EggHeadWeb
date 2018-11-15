using EggheadWeb.Models.Common;
using EggheadWeb.Models.UserModels;
using EggHeadWeb.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EggheadWeb.DAL
{
	public class AdminFrontendRepository : GenericRepository<AdminFrontend>
	{
		public AdminFrontendRepository(EggheadContext context) : base(context)
		{
		}

		public FrontendItem GetAdminFrontendItem(int id, long areaId)
		{
			Admin admin = (
				from t in this.context.Admins
				where t.AreaId == (long?)areaId
				select t).FirstOrDefault<Admin>();
			if (admin == null)
			{
				return null;
			}
			AdminFrontend frontend = (
				from t in this.context.AdminFrontends
				where t.Id == id && t.AdminId == admin.Id
				select t).FirstOrDefault<AdminFrontend>();
			FrontendItem frontendItem = new FrontendItem()
			{
				Id = frontend.Id,
				Name = frontend.Name,
				MenuName = frontend.MenuName,
				IsActive = frontend.IsActive,
				PageContent = frontend.OverridePageContent
			};
			return frontendItem;
		}

		public List<FrontendItem> GetAdminFrontendItems(long areaId)
		{
			(
				from t in this.context.Admins
				where t.AreaId == (long?)areaId
				select t).FirstOrDefault<Admin>();
			List<FrontendItem> frontendItems = new List<FrontendItem>();
			List<AdminFrontend> adminFrontends = (
				from t in this.context.AdminFrontends
				where t.Admin.AreaId == (long?)areaId
				select t).ToList<AdminFrontend>();
			adminFrontends.ForEach((AdminFrontend item) => frontendItems.Add(new FrontendItem()
			{
				Id = item.Id,
				Name = item.Name,
				MenuName = item.MenuName,
				IsActive = item.IsActive,
				PageContent = item.OverridePageContent
			}));
			return frontendItems;
		}
	}
}