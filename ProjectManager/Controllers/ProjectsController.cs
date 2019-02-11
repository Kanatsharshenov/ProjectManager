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

        [HttpPost]
        public ActionResult Index(string filter)
        {
            ViewBag.filter = filter;
            return View();
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
            // Убираем добавленных сотрудников из списка выбора, для избежания повторного добавления в проект сраницы "Детали"
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
            ViewBag.Priority = 9;
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
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FullName", projects.UserID);
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
        // Заполняем DropDownList сотрудниками при добавлении в проект
        public ActionResult GetItems(int projectId)
        {
            List<int> ID = db.Executors.Where(u => u.ProjectID == projectId).Select(s => s.UserID).ToList();
            // Убираем добавленных сотрудников из списка выбора, для избежания повторного добавления в проект сраницы "Детали"
            List<Users> usersToExcept = db.Users.ToList();
            List<Users> usersExcepted = db.Users.ToList();
            foreach (int i in ID)
            {
                usersToExcept.Remove(usersExcepted.Where(u => u.UserID == i).FirstOrDefault());
            }
            if (usersToExcept.Count != 0)
            {
                return PartialView("GetItems", usersToExcept);
            }else
            {
                return new EmptyResult();
            }
        }

        // Сортировка и поиск по полям
        public ActionResult GetProjects(string filter = null, string searchBy = null, string filterBeginDate = null, string filterEndDate = null, string sortParam = null)
        {
            List<Projects> projects = new List<Projects>();
            string NameSort = String.IsNullOrEmpty(sortParam) ? "name_desc" : "";
            string CustomerSort = sortParam == "Customer" ? "customer_desc" : "Customer";
            switch (searchBy)
            {
                case "Наименование":
                   projects = db.Projects.Where(x => x.Name.Contains(filter)).ToList();
                    break;
                case "Заказчик":
                    projects = db.Projects.Where(x => x.Customer.Contains(filter)).ToList();
                    break;
                case "Исполнитель":
                    projects = db.Projects.Where(x => x.Executor.Contains(filter)).ToList();
                    break;
                case "Дата начала":
                    if(filterBeginDate =="" || filterEndDate == "")
                    {
                        projects = db.Projects.ToList();
                    } else
                    {
                        DateTime beginDate = filterBeginDate == null ? new DateTime() : Convert.ToDateTime(filterBeginDate);
                        DateTime endDate = Convert.ToDateTime(filterEndDate);
                        projects = db.Projects.Where(x => x.BeginDate >= beginDate && x.BeginDate <= endDate).ToList();
                    }
                    break;

                case "Дата окончания":
                    if (filterBeginDate == "" || filterEndDate == "")
                    {
                        projects = db.Projects.ToList();
                    } else
                    {
                        DateTime beginDate = Convert.ToDateTime(filterBeginDate);
                        DateTime endDate = Convert.ToDateTime(filterEndDate);
                        projects = db.Projects.Where(x => x.EndDate >= beginDate && x.EndDate <= endDate).ToList();
                    }
                        
                    break;
                default:
                    projects = db.Projects.ToList();
                    break;
            }

            return PartialView("_TableProjects", OrderEntity(projects, sortParam));
        }
        // Метод дл сортировки полей. 
        public List<Projects> OrderEntity(List<Projects> pr, string srt)
        {
            List<Projects> projects;
            switch (srt)
            {
                case "name_desc":
                    projects = pr.OrderByDescending(s => s.Name).ToList();
                    break;
                case "customer_asc":
                    projects = pr.OrderBy(s => s.Customer).ToList();
                    break;
                case "customer_desc":
                    projects = pr.OrderByDescending(s => s.Customer).ToList();
                    break;
                case "executor_asc":
                    projects = pr.OrderBy(s => s.Executor).ToList();
                    break;
                case "executor_desc":
                    projects = pr.OrderByDescending(s => s.Executor).ToList();
                    break;
                case "date_begin_asc":
                    projects = pr.OrderBy(s => s.BeginDate).ToList();
                    break;
                case "date_begin_desc":
                    projects = pr.OrderByDescending(s => s.BeginDate).ToList();
                    break;
                case "date_end_asc":
                    projects = pr.OrderBy(s => s.EndDate).ToList();
                    break;
                case "date_end_desc":
                    projects = pr.OrderByDescending(s => s.EndDate).ToList();
                    break;
                case "priority_asc":
                    projects = pr.OrderBy(s => s.Priority).ToList();
                    break;
                case "priority_desc":
                    projects = pr.OrderByDescending(s => s.Priority).ToList();
                    break;
                case "head_asc":
                    projects = pr.OrderBy(s => s.Users.FullName).ToList();
                    break;
                case "head_desc":
                    projects = pr.OrderByDescending(s => s.Users.FullName).ToList();
                    break;
                default:
                    projects = pr.OrderBy(s => s.Name).ToList();
                    break;
            }
            return projects;
        }
        // Проверяем поле "Приоритет", чтобыоно не было отрицательным или не равен нулю
        public JsonResult IsPositive(int priority)
        {            
            if(priority <= 0)
                return Json("Введите положительное число!", JsonRequestBehavior.AllowGet);
            return Json( true, JsonRequestBehavior.AllowGet);
        }

        // Находим максимальное значение поля "Приоритет".
        public JsonResult GetMaxPriority()
        {
            int maxPriority = 0;
            if (db.Projects.Count() != 0)
            {
                maxPriority = db.Projects.Max(c => c.Priority);
            }
            
            
                return Json(++maxPriority, JsonRequestBehavior.AllowGet);            
        }
        // Отслеживаем, чтобы введенное на страницах "Создание" и "Редактирование" проектов, поле "Приоритет" был последовательным.
        // Сделано для правильной логики уменьшения или увеличивания приоритета.
        public JsonResult CheckPriority(int pr)
        {
            if (db.Projects.Count() != 0)
            {
                int maxPriority = db.Projects.Max(c => c.Priority);
                if (pr - maxPriority > 1)
                {
                    return Json(new { res = false, max = maxPriority }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
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
