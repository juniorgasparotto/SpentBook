using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SpentBook.Web.Controllers
{
    public static class Helper
    {
        public static string CreateFriendlyURL(string value)
        {
            if (String.IsNullOrEmpty(value)) return "";

            // remove accents
            value = RemoveAccents(value);

            // remove entities
            value = Regex.Replace(value, @"&\w+;", "");
            // remove anything that is not letters, numbers, dash, or space
            value = Regex.Replace(value, @"[^A-Za-z0-9\-\s]", "");
            // remove any leading or trailing spaces left over
            value = value.Trim();
            // replace spaces with single dash
            value = Regex.Replace(value, @"\s+", "-");
            // if we end up with multiple dashes, collapse to single dash            
            value = Regex.Replace(value, @"\-{2,}", "-");
            // make it all lower case
            value = value.ToLower();
            // if it's too long, clip it
            if (value.Length > 80)
                value = value.Substring(0, 79);
            // remove trailing dash, if there is one
            if (value.EndsWith("-"))
                value = value.Substring(0, value.Length - 1);

            return value;
        }

        public static string RemoveAccents(string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        //public static string RenderPartialViewToString(string viewName, object model, Controller controller)
        //{
        //    return "TEMp";
        //    //if (string.IsNullOrEmpty(viewName))
        //    //    viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

        //    //controller.ViewData.Model = model;

        //    //using (StringWriter sw = new StringWriter())
        //    //{
        //    //    ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
        //    //    ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
        //    //    viewResult.View.Render(viewContext, sw);
        //    //    return sw.GetStringBuilder().ToString();
        //    //}
        //}

        public static double UnixTicks(this DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
        }
    }
}