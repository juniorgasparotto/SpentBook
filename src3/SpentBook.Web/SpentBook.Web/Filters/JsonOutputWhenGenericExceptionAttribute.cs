using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SpentBook.Web.Helpers;

namespace SpentBook.Web.Filters
{
    public class JsonOutputWhenGenericExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var value = new
                {
                    name = filterContext.Exception.GetType().Name,
                    message = filterContext.Exception.Message,
                    callstack = filterContext.Exception.StackTrace
                };

                filterContext.Result = new JsonResult(value)
                {
                    ContentType = "application/json"
                };

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 500;
                //filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
}