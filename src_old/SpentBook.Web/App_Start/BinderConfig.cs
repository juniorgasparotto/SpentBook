using SpentBook.Web.Binder;
using SpentBook.Web.Filters;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SpentBook.Web
{
    public class BinderConfig
    {
        public static void RegisterBinders()
        {
            ModelBinders.Binders.DefaultBinder = new EnumModelBinder();
            //var binder = new KeyValuePair<Type,IModelBinder>(typeof(DateTimeFormattedModelBinder), new DateTimeFormattedModelBinder());
            //ModelBinders.Binders.Add(binder);
        }
    }
}
