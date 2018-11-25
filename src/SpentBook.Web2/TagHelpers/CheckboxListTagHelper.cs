using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SpentBook.Web.TagHelpers
{
    public class CheckboxListTagHelper : TagHelper
    {
        public ModelExpression Source { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var listItems = TagHelperUtils.EnumToSelectList(Source);
            string prefix = Source.Name;
            int index = 0;

            output.TagName = "div";
            var content = "";
            foreach (var li in listItems)
            {
                string fieldName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", prefix, index++);
                content += @"<label class=""checkbox-inline"">";
                content += $@"<input type=""checkbox"" value=""{li.Value}"" name=""{prefix}"" id=""{fieldName}"" {(li.Selected ? "checked=\"checked\"" : "")}>";
                content += li.Text;
                content += "</label>";
            }

            output.Content.AppendHtml(content);
        }
    }
}