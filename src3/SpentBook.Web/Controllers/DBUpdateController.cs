using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpentBook.Web.Controllers
{
    [Authorize]
    public class DBUpdateController : Controller
    {
        public ActionResult Index()
        {
            PocDatabaseUoW.PocFile = null;
            return new EmptyResult();
        }
    }
}
