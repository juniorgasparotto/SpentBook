using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpentBook.Web.Controllers
{
    [Authorize]
    public class DBUpdateController : Controller
    {
        public ActionResult Index()
        {
            //var databaseFile = Helper.GetUserDataBase(HttpContext, HttpContext.User);
            //FileDataBase.Refresh(databaseFile);
            return null;
        }
    }
}
