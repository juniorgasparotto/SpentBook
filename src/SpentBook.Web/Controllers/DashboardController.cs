using CsvHelper;
using DotNet.Highcharts.Options;
using SpentBook.Data.FileSystem;
using SpentBook.Domain;
using SpentBook.Web.Filters;
using SpentBook.Web.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpentBook.Web.Controllers
{
    [JsonOutputWhenModelInvalid]
    [JsonOutputWhenGenericException]
    [Authorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {   
            return View();
        }

        public PartialViewResult Dashboards()
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var model = new DashboardsModel();
            model.Dashboards = uow.Dashboards.GetAll().ToList();

            return PartialView(model);
        }

        [ActionName("View")]
        public ActionResult Dashboard(string id)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var model = new DashboardModel();
            model.Dashboard = uow.Dashboards.Get(f => f.FriendlyUrl == id).FirstOrDefault();
            return View("Dashboard", model);
        }

        [HttpPost]
        public JsonResult Create(DashboardModel model)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var name = model.Name.Trim();
            var dashboard = new Dashboard()
            {
                Name = name,
                FriendlyUrl = Helper.CreateFriendlyURL(name)
            };

            var dashboardExists = uow.Dashboards.Get(f => f.FriendlyUrl == dashboard.FriendlyUrl).FirstOrDefault();
            if (dashboardExists != null)
                throw new Exception("Já existe um dashboard cadastrado com esse mesmo nome, tente outro.");

            uow.Dashboards.Insert(dashboard);

            return Json(new { Success = true });
        }

        [HttpGet]
        public JsonResult Delete(Guid id)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            uow.Dashboards.Delete(id);

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}