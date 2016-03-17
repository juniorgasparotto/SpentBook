using SpentBook.Web.Binder;
using SpentBook.Web.Filters;
using System.Web;
using System.Web.Mvc;

namespace SpentBook.Web
{
    public class BinderConfig
    {
        public static void RegisterBinders()
        {
            ModelBinders.Binders.DefaultBinder = new CustomModelBinder();
        }
    }
}
