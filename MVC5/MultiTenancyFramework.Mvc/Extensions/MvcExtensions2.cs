﻿using System.Web.Routing;

namespace System.Web.Mvc
{
    public static class MvcExtensions2
    {
        /// <summary>
        /// This incorporates the institution code in the generated Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string MyAction(this UrlHelper url)
        {
            return url.MyAction(null, null, null, null);
        }

        /// <summary>
        /// This incorporates the institution code in the generated Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static string MyAction(this UrlHelper url, string actionName)
        {
            return url.MyAction(actionName, null, null, null);
        }

        /// <summary>
        /// This incorporates the institution code in the generated Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static string MyAction(this UrlHelper url, string actionName, string controllerName)
        {
            return url.MyAction(actionName, controllerName, null, null);
        }

        /// <summary>
        /// This incorporates the institution code in the generated Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="areaName"></param>
        /// <returns></returns>
        public static string MyAction(this UrlHelper url, string actionName, string controllerName, string areaName)
        {
            return url.MyAction(actionName, controllerName, areaName, null);
        }

        /// <summary>
        /// This incorporates the institution code in the generated Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="areaName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string MyAction(this UrlHelper url, string actionName, string controllerName, string areaName, object routeValues)
        {
            RouteValueDictionary routes;
            if (routeValues == null)
            {
                routes = new RouteValueDictionary();
            }
            else
            {
                routes = new RouteValueDictionary(routeValues);
            }
            routes["institution"] = HttpContext.Current.Request.RequestContext.RouteData.Values["institution"];
            if (areaName != null)
            {
                routes["area"] = areaName;
            }
            if (string.IsNullOrWhiteSpace(actionName))
            {
                actionName = Convert.ToString(HttpContext.Current.Request.RequestContext.RouteData.Values["action"]);
            }
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                return url.Action(actionName, routes);
            }
            return url.Action(actionName, controllerName, routes);
        }

    }
}
