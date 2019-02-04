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
    public class ProjectsController : Controller
    {
        private ProjectManagerEntities3 db = new ProjectManagerEntities3();

        // GET: Projects
        public ActionResult Index()
        {
            var projects = db.Projects.Include(p => p.Users);
            return View(projects.ToList());
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projects projects = db.Projects.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }

            List<int> ID = db.Executors.Where(u => u.ProjectID == projects.ProjectID).Select(s => s.UserID).ToList();
            List<Users> usersToExcept = db.Users.ToList();
            List<Users> usersExcepted = db.Users.ToList();
            foreach (int i in ID)
            {
                usersToExcept.Remove(usersExcepted.Where(u => u.UserID == i).FirstOrDefault());
            }            
           
            SelectList users = new SelectList(usersToExcept, "UserId", "FullName");
            ViewBag.Users = users;
            return View(projects);
        }

        // GET: Projects/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FullName");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProjectID,Name,Customer,Executor,UserID,BeginDate,EndDate,Priority,Text")] Projects projects)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(projects);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", projects.UserID);
            return View(projects);
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projects projects = db.Projects.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", projects.UserID);
            return View(projects);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProjectID,Name,Customer,Executor,UserID,BeginDate,EndDate,Priority,Text")] Projects projects)
        {
            if (ModelState.IsValid)
            {
                db.Entry(projects).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", projects.UserID);
            return View(projects);
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projects projects = db.Projects.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }
            return View(projects);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Projects projects = db.Projects.Find(id);
            db.Projects.Remove(projects);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GetItems(int projectId)
        {
            List<int> ID = db.Executors.Where(u => u.ProjectID == projectId).Select(s => s.UserID).ToList();
            List<Users> usersToExcept = db.Users.ToList();
            List<Users> usersExcepted = db.Users.ToList();
            foreach (int i in ID)
            {
                usersToExcept.Remove(usersExcepted.Where(u => u.UserID == i).FirstOrDefault());
            }          
            return PartialView("GetItems", usersToExcept);
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
