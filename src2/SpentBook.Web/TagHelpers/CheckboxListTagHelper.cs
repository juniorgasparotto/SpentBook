using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace SpentBook.Web.TagHelpers
{
    [HtmlTargetElement("checkbox-list")]
    public class CheckboxListTagHelper : TagHelper
    {
        public Enum Value { get; set; }

        public override void Init(TagHelperContext context)
        {
            base.Init(context);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var listItems = Enum.GetValues(Value.GetType())
                .OfType<Enum>()
                .Select(e =>
                    new SelectListItem()
                    {
                        Text = GetDescription(e),
                        Value = e.ToString(),
                        Selected = Value.HasFlag(e)
                    }
                );

            //string prefix = ViewData.TemplateInfo.HtmlFieldPrefix;
            var prefix = "";
            int index = 0;
            //ViewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

            output.TagName = "div";
            foreach (var li in listItems)
            {
                string fieldName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", prefix, index++);
                var content = @"<label class=""checkbox - inline"">";
                content += $@"<input type=""checkbox"" value=""{li.Value}"" name=""{prefix}"" id=""{fieldName}"" {(li.Selected ? "checked=\"checked\"" : "")}>";
                output.Content.SetContent(content);
            }
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            return base.ProcessAsync(context, output);
        }

        private string GetDescription(Enum en)
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
