using CsvHelper;
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
    public class PanelController : Controller
    {
        private const string CREATE_OR_EDIT_TEMPLATE = "CreateOrEdit";
        
        [HttpGet]
        public ActionResult Details(Guid dashboardId, Guid panelId)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).First();
            var panel = dashboard.Panels.First(f => f.Id == panelId);
            var model = this.ConvertObjectDomainToModel(panel, dashboard);
            
            if (Request.IsAjaxRequest())
                return PartialView(model);

            return View(model);
        }

        [HttpGet]
        public ActionResult Create(Guid dashboardId)
        {
            ViewBag.IsEdit = false;

            var model = new PanelModel();

            if (Request.IsAjaxRequest())
                return PartialView(CREATE_OR_EDIT_TEMPLATE, model);

            return View(CREATE_OR_EDIT_TEMPLATE, model);
        }

        [HttpPost]
        public ActionResult Create(Guid dashboardId, PanelModel model)
        {
            ViewBag.IsEdit = false;

            if (ModelState.IsValid)
            {
                var uow = Helper.GetUnitOfWorkByCurrentUser();
                var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();

                var panel = this.ConvertModelToObjectDomain(model);
                panel.Id = Guid.NewGuid();
                panel.CreateDate = DateTime.Now;
                
                if (dashboard.Panels == null)
                    dashboard.Panels = new List<Panel>();

                if (dashboard.Panels.Count > 0)
                    panel.PanelOrder = dashboard.Panels.Max(f=>f.PanelOrder) + 1;
                else
                    panel.PanelOrder = 1;

                dashboard.Panels.Add(panel);
                uow.Dashboards.Update(dashboard);

                if (Request.IsAjaxRequest())
                    return Json(new { Success = true });

                return RedirectToDashboard(dashboard.FriendlyUrl);
            }
            else
            {
                if (Request.IsAjaxRequest())
                    return PartialView(CREATE_OR_EDIT_TEMPLATE, model);

                return View(CREATE_OR_EDIT_TEMPLATE, model);
            }
        }

        [HttpGet]
        public ActionResult Edit(Guid dashboardId, Guid panelId)
        {
            ViewBag.IsEdit = true;

            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).First();
            var panel = dashboard.Panels.First(f => f.Id == panelId);
            var model = this.ConvertObjectDomainToModel(panel, dashboard);

            if (Request.IsAjaxRequest())
                return PartialView(CREATE_OR_EDIT_TEMPLATE, model);

            return View(CREATE_OR_EDIT_TEMPLATE, model);
        }
        
        [HttpPost]
        public ActionResult Edit(Guid dashboardId, Guid panelId, PanelModel model)
        {
            ViewBag.IsEdit = true;

            if (ModelState.IsValid)
            {
                var uow = Helper.GetUnitOfWorkByCurrentUser();
                var panelReplaced = this.ConvertModelToObjectDomain(model);
                
                var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
                var panelExists = dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
                var panelPosition = dashboard.Panels.IndexOf(panelExists);

                panelReplaced.Id = panelExists.Id;
                panelReplaced.CreateDate = panelExists.CreateDate;
                panelReplaced.LastUpdateDate = DateTime.Now;
                panelReplaced.PanelOrder = panelExists.PanelOrder;

                dashboard.Panels.Remove(panelExists);
                dashboard.Panels.Insert(panelPosition, panelReplaced);
                uow.Dashboards.Update(dashboard);

                if (Request.IsAjaxRequest())
                    return Json(new { Success = true });

                return RedirectToDashboard(dashboard.FriendlyUrl);
            }
            else
            {
                if (Request.IsAjaxRequest())
                    return PartialView(CREATE_OR_EDIT_TEMPLATE, model);

                return View(CREATE_OR_EDIT_TEMPLATE, model);
            }
        }
        
        [HttpGet]
        public ActionResult Delete(Guid dashboardId, Guid panelId)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            dashboard.Panels.RemoveAll(f => f.Id == panelId);
            uow.Dashboards.Update(dashboard);

            if (Request.IsAjaxRequest())
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            else
                return RedirectToDashboard(dashboard.FriendlyUrl);
        }

        [HttpGet]
        public JsonResult PanelsUpdated(Guid dashboardId, Guid? panelId)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var model = new DashboardModel();
            model.Dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            return Json(model.Dashboard.Panels, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PanelsUpdated(Guid dashboardId, List<Panel> panelsExistsInInterface)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            var panelsExistsInDB = dashboard.Panels;

            panelsExistsInDB = panelsExistsInDB ?? new List<Panel>();
            panelsExistsInInterface = panelsExistsInInterface ?? new List<Panel>();

            // exists in DB but not exists in interface
            var news = (
                from panelDB in panelsExistsInDB
                from panelInterface in panelsExistsInInterface.Where(f => f.Id == panelDB.Id).DefaultIfEmpty()
                where panelInterface == null
                orderby panelDB.PanelOrder, panelDB.Title ascending
                select panelDB
            ).ToList();

            // exists in interface but not exists in DB
            var deleteds = (
                from panelInterface in panelsExistsInInterface
                from panelDB in panelsExistsInDB.Where(f => f.Id == panelInterface.Id).DefaultIfEmpty()
                where panelDB == null
                orderby panelInterface.PanelOrder, panelInterface.Title ascending
                select panelInterface
            ).ToList();

            // exists in both, but the update date in DB is more than interface
            var updateds = (
                from panelDB in panelsExistsInDB
                join panelInterface in panelsExistsInInterface on panelDB.Id equals panelInterface.Id
                where panelDB.LastUpdateDate > panelInterface.LastUpdateDate
                orderby panelDB.PanelOrder, panelDB.Title ascending
                select panelDB
            ).ToList();

            var changes = new
            {
                News = news,
                Deleteds = deleteds,
                Updateds = updateds
            };

            return Json(changes, JsonRequestBehavior.AllowGet);
        }  

        private PanelModel ConvertObjectDomainToModel(Panel panel, Dashboard dashboard)
        {
            var model = new PanelModel();
            
            if (panel != null)
            {
                model.Title = panel.Title;
                model.Id = panel.Id;
                model.Dashboard = dashboard;
                model.PanelOrder = panel.PanelOrder;
                model.PanelType = panel.PanelType;
                model.GroupBy = panel.GroupBy;
                model.GroupBy2 = panel.GroupBy2;
                model.OrderBy = panel.OrderBy;
                model.OrderByClassification = panel.OrderByClassification;
                model.FilterTransactionType = panel.Filter.TransactionType;
                model.FilterDateStart = panel.Filter.DateStart;
                model.FilterDateEnd = panel.Filter.DateEnd;
                model.FilterValueStart = panel.Filter.ValueStart;
                model.FilterValueEnd = panel.Filter.ValueEnd;

                if (panel.Filter.Categories != null)
                    model.FilterCategories = string.Join(",", panel.Filter.Categories);

                if (panel.Filter.SubCategories != null)
                    model.FilterSubCategories = string.Join(",", panel.Filter.SubCategories);

                if (panel.Filter.Names != null)
                    model.FilterTransactionNames = string.Join(",", panel.Filter.Names);
            }

            return model;
        }

        private Panel ConvertModelToObjectDomain(PanelModel model)
        {
            var panel = new Panel();
            panel.Title = model.Title;
            panel.PanelType = model.PanelType;
            panel.PanelOrder = model.PanelOrder;
            panel.GroupBy = model.GroupBy;
            panel.GroupBy2 = model.GroupBy2;
            panel.OrderBy = model.OrderBy;
            panel.OrderByClassification = model.OrderByClassification;
            panel.Filter = new TransactionFilter();
            panel.Filter.TransactionType = model.FilterTransactionType;
            panel.Filter.DateStart = model.FilterDateStart;
            panel.Filter.DateEnd = model.FilterDateEnd;
            panel.Filter.ValueStart = model.FilterValueStart;
            panel.Filter.ValueEnd = model.FilterValueEnd;
            
            if (!string.IsNullOrWhiteSpace(model.FilterCategories))
            { 
                panel.Filter.Categories = new List<string>();
                panel.Filter.Categories = model.FilterCategories.Split(',').ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.FilterSubCategories))
            {
                panel.Filter.SubCategories = new List<string>();
                panel.Filter.SubCategories = model.FilterSubCategories.Split(',').ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.FilterTransactionNames))
            {
                panel.Filter.Names = new List<string>();
                panel.Filter.Names = model.FilterTransactionNames.Split(',').ToList(); string.Join(",", panel.Filter.Names);
            }

            return panel;
        }

        private ActionResult RedirectToDashboard(string url)
        {
            return RedirectToAction("View", "Dashboard", new { id = url });
        }
    }
}