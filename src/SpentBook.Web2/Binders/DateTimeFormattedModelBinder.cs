//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Globalization;
//using System.Linq;
//using System.Net;
//using System.Web;
//using System.Web.Mvc;

//namespace SpentBook.Web.Binder
//{
//    public class DateTimeFormattedModelBinder : DefaultModelBinder
//    {
//        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
//        {
//            var displayFormat = bindingContext.ModelMetadata.DisplayFormatString;
//            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

//            if (!string.IsNullOrEmpty(displayFormat) && value != null)
//            {
//                DateTime date;
//                displayFormat = displayFormat.Replace("{0:", string.Empty).Replace("}", string.Empty);
//                // use the format specified in the DisplayFormat attribute to parse the date
//                if (DateTime.TryParseExact(value.AttemptedValue, displayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
//                {
//                    return date;
//                }
//                else
//                {
//                    bindingContext.ModelState.AddModelError(
//                        bindingContext.ModelName,
//                        string.Format("{0} is an invalid date format", value.AttemptedValue)
//                    );
//                }
//            }

//            return base.BindModel(controllerContext, bindingContext);
//        }
//    }
//}