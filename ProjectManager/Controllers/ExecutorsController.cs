using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProjectManager.Models;

namespace ProjectManager.Controllers
{
    public class ExecutorsController : Controller
    {
        private ProjectManagerEntities3 db = new ProjectManagerEntities3();

        // GET: Executors
        public ActionResult Index()
        {
            var executors = db.Executors.Include(e => e.Users).Include(e => e.Projects);
            return View(executors.ToList());
        }

        // GET: Executors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Executors executors = db.Executors.Find(id);
            if (executors == null)
            {
                return HttpNotFound();
            }
            return View(executors);
        }

        // GET: Executors/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName");
            ViewBag.ProjectID = new SelectList(db.Projects, "ProjectID", "Name");
            return View();
        }

        // POST: Executors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create([Bind(Include = "ExecutorID,UserID,ProjectID")] Executors executors)
        {
            var res = true;
            if (ModelState.IsValid)
            {
                
                db.Executors.Add(executors);
                int result = db.SaveChanges();
                if (result != 1)
                {
                    res = false;
                }                
            }

            /*ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", executors.UserID);
            ViewBag.ProjectID = new SelectList(db.Projects, "ProjectID", "Name", executors.ProjectID);
            return View(executors);*/
            return Json(new
            {
                result = res, id = executors.ExecutorID
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Executors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Executors executors = db.Executors.Find(id);
            if (executors == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", executors.UserID);
            ViewBag.ProjectID = new SelectList(db.Projects, "ProjectID", "Name", executors.ProjectID);
            return View(executors);
        }

        // POST: Executors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ExecutorID,UserID,ProjectID")] Executors executors)
        {
            if (ModelState.IsValid)
            {
                db.Entry(executors).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", executors.UserID);
            ViewBag.ProjectID = new SelectList(db.Projects, "ProjectID", "Name", executors.ProjectID);
            return View(executors);
        }

        // GET: Executors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Executors executors = db.Executors.Find(id);
            if (executors == null)
            {
                return HttpNotFound();
            }
            return View(executors);
        }

        // POST: Executors/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteConfirmed(int id)
        {
            var res = true;
            Executors executors = db.Executors.Find(id);
            db.Executors.Remove(executors);
            int result = db.SaveChanges();
            if (result != 1)
            {
                res = false;
            }
            return Json(new
            {
                result = res
            }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
