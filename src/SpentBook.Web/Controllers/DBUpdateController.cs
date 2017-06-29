using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using SpentBook.Web.Models;
using SpentBook.Data.FileSystem;

namespace SpentBook.Web.Controllers
{
    public class DBUpdateController : Controller
    {
        public ActionResult Index()
        {
            var databaseFile = Helper.GetUserDataBase(HttpContext.User);
            FileDataBase.Refresh(databaseFile);
            return null;
        }
    }
}
