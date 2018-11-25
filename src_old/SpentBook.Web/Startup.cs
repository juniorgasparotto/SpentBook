using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SpentBook.Web.Startup))]
namespace SpentBook.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
