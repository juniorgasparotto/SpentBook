using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SpentBook.Web.Filters
{
    public class JsonOutputWhenGenericExceptionAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult()
                {
                    ContentType = "application/json",
                    Data = new
                    {
                        name = filterContext.Exception.GetType().Name,
                        message = filterContext.Exception.Message,
                        callstack = filterContext.Exception.StackTrace
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
}