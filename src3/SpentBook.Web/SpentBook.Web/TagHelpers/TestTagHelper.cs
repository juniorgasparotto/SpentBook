using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SpentBook.Web.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("test")]
    public class TestTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Content.SetContent("TESTE");
        }
    }
}
