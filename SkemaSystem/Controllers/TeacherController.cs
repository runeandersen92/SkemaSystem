﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SkemaSystem.Models;
using System.Diagnostics;
using System.Text;

namespace SkemaSystem.Controllers
{
    //[Authorize(Roles="teacher")]
    [RouteArea("admin")]
    [RoutePrefix("teachers")]
    [Route("{action=index}")]
    public class TeacherController : BaseController
    {
        private SkeamSystemDb db = new SkeamSystemDb();

        // GET: /admin/teachers/
        [Route("")]
        public ActionResult Index()
        {
            return View(db.Teachers.ToList());
        }

        // GET: /admin/teachers/details/5
        [Route("details")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // GET: /admin/teachers/create
        [Route("create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /admin/teachers/create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("create")]
        public ActionResult Create([Bind(Include="Id,Name")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                db.Teachers.Add(teacher);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(teacher);
        }

        // GET: /admin/teachers/edit/5
        [Route("edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            //List<SelectListItem> educations = new List<SelectListItem>();
            //educations.Add(new SelectListItem { Text = "DMU", Value = "1", Selected = false });

            //teacher.Educations.Add(db.Educations.First());
            //ViewBag.Educations = db.Educations.ToList();
            return View(GetFruitsInitialModel(teacher));
            //return View(new Test(teacher, educations));
        }

        // POST: /admin/teachers/edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Route("edit")]
        public ActionResult Edit(TeacherViewModel result)
        {
            Debug.WriteLine(result);

            Teacher teacher = result.Teacher;

            if (result.PostedEducations != null && result.PostedEducations.EducationIds.Any())
            {
                IEnumerable<Education> educations = db.Educations;
                teacher.Educations = educations
                 .Where(x => result.PostedEducations.EducationIds.Any(s => x.EducationId.ToString().Equals(s)))
                 .ToList();
            }
            else
            {
                teacher.Educations = new List<Education>();
            }
            Debug.WriteLine(db.Educations.First());

            if (ModelState.IsValid)
            {
                Education education = teacher.Educations.FirstOrDefault<Education>();

                teacher.Educations.Remove(education);

                db.Entry(teacher).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                // TODO redirect
            }

            return View(GetFruitsModel(teacher, result.PostedEducations));
        }

        // GET: /admin/teachers/delete/5
        [Route("delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // POST: /admin/teachers/delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Teacher teacher = db.Teachers.Find(id);
            db.Teachers.Remove(teacher);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /*public static class FruitRepository
        {
            /// <summary>
            /// for get fruit for specific id
            /// </summary>
            public static Education Get(SkeamSystemDb db, int id)
            {
                return GetAll(db).FirstOrDefault(x => x.Id.Equals(id));
            }

            /// <summary>
            /// for get all fruits
            /// </summary>
            public static IEnumerable<Education> GetAll(SkeamSystemDb db)
            {
                return db.Educations;
                //    new List<Education> {
                //    new Education {Name = "Apple", Id = 1 },
                //    new Education {Name = "Banana", Id = 2},
                //    new Education {Name = "Cherry", Id = 3},
                //    new Education {Name = "Pineapple", Id = 4},
                //    new Education {Name = "Grape", Id = 5},
                //    new Education {Name = "Guava", Id = 6},
                //    new Education {Name = "Mango", Id = 7}
                //};
            }
        }*/
    }
}
