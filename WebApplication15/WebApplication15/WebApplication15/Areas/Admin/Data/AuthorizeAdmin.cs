using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication15.Areas.Admin.Data
{
    public class AuthorizeAdmin : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var role = httpContext.Session["Role"];
            return role != null && role.ToString() == "Admin";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/User/Login");
        }
    }
}