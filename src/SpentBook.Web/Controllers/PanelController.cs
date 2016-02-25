﻿using CsvHelper;
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
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            
            var model = new PanelModel();

            if (dashboard.Panels == null || dashboard.Panels.Count == 0)
                model.PanelOrder = 1;
            else
                model.PanelOrder = dashboard.Panels.Max(f => f.PanelOrder) + 1;

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
                panel.LastUpdateDate = panel.CreateDate;
                
                if (dashboard.Panels == null)
                    dashboard.Panels = new List<Panel>();

                dashboard.Panels.Add(panel);

                // reorder panels
                dashboard.ReorderPanel(panel, panel.PanelOrder);

                // save dashboard
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
                var panelUpdate = this.ConvertModelToObjectDomain(model);
                
                var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
                var panelOld = dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
                var panelPosition = dashboard.Panels.IndexOf(panelOld);

                panelUpdate.Id = panelOld.Id;
                panelUpdate.CreateDate = panelOld.CreateDate;
                panelUpdate.LastUpdateDate = DateTime.Now;

                dashboard.Panels.Remove(panelOld);
                dashboard.Panels.Insert(panelPosition, panelUpdate);

                // reorder panels
                dashboard.ReorderPanel(panelUpdate, panelUpdate.PanelOrder);

                //var addAfterIfOccurConflict = panelUpdate.PanelOrder > panelOld.PanelOrder;
                //dashboard.ReorderPanels(addAfterIfOccurConflict);

                // save dashboard
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

            // reorder panels
            dashboard.ReorderPanel();

            // save dashboard
            uow.Dashboards.Update(dashboard);

            if (Request.IsAjaxRequest())
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            else
                return RedirectToDashboard(dashboard.FriendlyUrl);
        }

        [HttpGet]
        public JsonResult ChangePanelOrder(Guid dashboardId, Guid panelId, int newOrder)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            var panel = dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
            dashboard.ReorderPanel(panel, newOrder);
            uow.Dashboards.Update(dashboard);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
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
                model.PanelWidth = panel.PanelWidth;
                model.PanelType = panel.PanelType;
                model.GroupBy = panel.GroupBy;
                model.GroupBy2 = panel.GroupBy2;
                model.GroupBy3 = panel.GroupBy3;
                model.OrderBy = panel.OrderBy;
                model.OrderByClassification = panel.OrderByClassification;
                model.FilterTransactionType = panel.Filter.TransactionType;
                model.FilterDateStart = panel.Filter.DateStart;
                model.FilterDateEnd = panel.Filter.DateEnd;
                model.FilterValueStart = panel.Filter.ValueStart;
                model.FilterValueEnd = panel.Filter.ValueEnd;
                model.ViewName = panel.ViewName;

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
            panel.PanelWidth = model.PanelWidth;
            panel.GroupBy = model.GroupBy;
            panel.GroupBy2 = model.GroupBy2;
            panel.GroupBy3 = model.GroupBy3;
            panel.OrderBy = model.OrderBy;
            panel.OrderByClassification = model.OrderByClassification;
            panel.Filter = new TransactionFilter();
            panel.Filter.TransactionType = model.FilterTransactionType;
            panel.Filter.DateStart = model.FilterDateStart;
            panel.Filter.DateEnd = model.FilterDateEnd;
            panel.Filter.ValueStart = model.FilterValueStart;
            panel.Filter.ValueEnd = model.FilterValueEnd;
            panel.ViewName = model.ViewName;

            if (!string.IsNullOrWhiteSpace(model.FilterCategories))
            { 
                panel.Filter.Categories = new List<string>();
                panel.Filter.Categories = model.FilterCategories.Split(',').Select(s => s.Trim()).ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.FilterSubCategories))
            {
                panel.Filter.SubCategories = new List<string>();
                panel.Filter.SubCategories = model.FilterSubCategories.Split(',').Select(s => s.Trim()).ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.FilterTransactionNames))
            {
                panel.Filter.Names = new List<string>();
                panel.Filter.Names = model.FilterTransactionNames.Split(',').Select(s => s.Trim()).ToList(); 
            }

            return panel;
        }

        private ActionResult RedirectToDashboard(string url)
        {
            return RedirectToAction("View", "Dashboard", new { id = url });
        }
    }
}