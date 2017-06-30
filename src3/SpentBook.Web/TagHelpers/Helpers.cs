using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;

namespace SpentBook.Web.TagHelpers
{
    public static class TagHelperUtils
    {
        public static IEnumerable<SelectListItem> EnumToSelectList(ModelExpression _source)
        {
            var source = (Enum)_source.Model;
            return Enum.GetValues(source.GetType())
                .OfType<Enum>()
                .Select(e =>
                    new SelectListItem()
                    {
                        Text = GetEnumValueDescription(e),
                        Value = e.ToString(),
                        Selected = source.HasFlag(e)
                    }
                ).ToList();
        }

        public static string GetEnumValueDescription(Enum en)
        {
            Type type = en.GetType();
            System.Reflection.MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute),
                                                                false);

                if (attrs != null && attrs.Length > 0)
                    return ((System.ComponentModel.DataAnnotations.DisplayAttribute)attrs[0]).GetName();
            }

            return en.ToString();
        }
    }
}