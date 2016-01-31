using SpentBook.Web.Filters;
using System.Web;
using System.Web.Mvc;

namespace SpentBook.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new JsonNetActionFilter());
        }
    }
}
