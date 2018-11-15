using System;
using System.Web.Mvc;

namespace EggheadWeb.Security
{
	public class UserAuthorizeFilter : AuthorizeAttribute
	{
		public UserAuthorizeFilter()
		{
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if ((filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) ? false : !filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)))
			{
				base.OnAuthorization(filterContext);
			}
		}
	}
}