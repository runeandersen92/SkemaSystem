﻿using SkemaSystem.Models;
using SkemaSystem.Models.ViewModels;
using SkemaSystem.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SkemaSystem.Controllers
{
    [Authorize(Roles="Teacher,Admin,Master")]
    [RouteArea("Admin", AreaPrefix = "admin")]
    [RoutePrefix("{education}/scheduling")]
    [Route("{action=index}")]
    public class SchedulingController : BaseController
    {
        [Route("~/admin/scheduling")]
        public ActionResult Redirect()
        {
            Teacher teacher = db.Teachers.SingleOrDefault(t => t.Id.Equals(User.Id));

            Education education = teacher.Educations.FirstOrDefault();

            return RedirectToAction("Index", new { education = education.Name.ToLower() });
        }

        //[Route("~/admin/{education?}/scheduling", Name = "Schedule")]
        public ActionResult Index(string education)
        {
            Education _education = db.Educations.FirstOrDefault(e => e.Name.Equals(education));

            if (!IsTeacher())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            IEnumerable<SelectListItem> schemes = from s in db.Schemes
                                                  select new SelectListItem { Text = s.ClassModel.ClassName + " " + SqlFunctions.StringConvert((double)s.Semester.Number).Trim() + ". semester", Value = SqlFunctions.StringConvert((double)s.Id).Trim() };
            ViewBag.schemes = schemes;

            IEnumerable<SelectListItem> educations = from e in db.Educations
                                                     select new SelectListItem { Text = e.Name, Value = SqlFunctions.StringConvert((double)e.Id).Trim() };
            ViewBag.educations = educations;

            IEnumerable<SelectListItem> rooms = from r in db.Rooms
                                                     select new SelectListItem { Text = r.RoomName, Value = SqlFunctions.StringConvert((double)r.Id).Trim() };
            ViewBag.rooms = rooms;

            ViewBag.SubjectDistBlocks = new List<SubjectDistBlock>();

            IEnumerable<SelectListItem> teachers = from e in db.Teachers
                                                   select new SelectListItem { Text = e.Name, Value = SqlFunctions.StringConvert((double)e.Id).Trim() };
            ViewBag.teachers = teachers;

            return View();
        }


        public ActionResult ChangeSubjectDropDown(int scheme)
        {
            if (!IsTeacher())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            ViewBag.SubjectDistBlocks = db.Schemes.Single(x => x.Id == scheme).SubjectDistBlocks;

            return PartialView("_SubjectDropDown");
        }

        [Route("lesson"), HttpPost]
        public ActionResult ScheduleLesson(int schemeId, int subjectId, int roomId, DateTime date, int blockNumber)
        {
            if (!IsTeacher())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            LessonBlock lesson;

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                Timeout = TransactionManager.DefaultTimeout
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                try
                {
                    lesson = SchedulingService.ScheduleLesson(schemeId, subjectId, roomId, date, blockNumber, db.Schemes, db.Rooms);
                    
                    // if no exception thrown
                    db.SaveChanges();
                    scope.Complete();
                }
                catch (Exception e)
                {
                    scope.Dispose();
                    return Json(new { message = e.Message });
                }
            }
            return PartialView("_LessonBlockPartial", lesson);
        }

        [Route("lesson/delete"), HttpPost]
        public ActionResult DeleteLessons(int schemeId, string lessonIds)
        {
            if (!IsTeacher())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            // TODO check permissions

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                Timeout = TransactionManager.DefaultTimeout
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                if (SchedulingService.DeleteLessons(schemeId, lessonIds, db.Schemes))
                {
                    db.SaveChanges();
                    scope.Complete();
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [Route("lesson/relocate"), HttpPost]
        public ActionResult RelocateLesson(int schemeId, string lessonIds, int roomId)
        {
            if (!IsTeacher())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            // TODO check permissions

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                Timeout = TransactionManager.DefaultTimeout
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                if (SchedulingService.RelocateLesson(schemeId, lessonIds, roomId, db.Schemes, db.Rooms))
                {
                    db.SaveChanges();
                    scope.Complete();
                }
                else
                {
                    scope.Dispose();
                    return Json(new { message = "Lokalet er ikke ledigt på det pågældende tidspunkt." });
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult ChangeScheme(int scheme)
        {
            if (!IsTeacher())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            Scheme _scheme = db.Schemes.Single(x => x.Id == scheme);
            
            DateTime startDate = new DateTime(2014, 5, 25);

            Scheme data = _scheme;

            TableViewModel tvm = new TableViewModel() { ClassName = _scheme.ClassModel.ClassName, StartDate = SchedulingService.CalculateStartDate(startDate), TableCells = SchedulingService.buildScheme(startDate, data) };

            SchemeViewModel model = new SchemeViewModel();
            model.Classname = _scheme.ClassModel.ClassName;
            model.Schemes.Add(tvm);
            if (scheme == 2)
            {
                model.Schemes.Add(tvm);
            }
            return PartialView("_SchemePartial", model);
        }

        public ActionResult FindAHoleInScheme(string blockIdsString)
        {
            string[] block = blockIdsString.Split(',');
            int[] blockIds = ConvertStringArraytoInt(block);

            //******************************
            // Get the lessonblocks of the id's
            var choosenBlocks = db.LessonBlocks.Where(lb => blockIds.Contains(lb.Id));
            //******************************

            //******************************
            // Get the mainScheme
            var mainScheme = db.Schemes.Where(sc => sc.LessonBlocks.FirstOrDefault().Id == choosenBlocks.FirstOrDefault().Id).SingleOrDefault();
            //******************************

            //******************************
            // Gets the lessonblocks which may conflict
            List<LessonBlock> conflictLessons = new List<LessonBlock>();
            foreach (var item in mainScheme.ConflictSchemes)
            {
                conflictLessons.AddRange(item.LessonBlocks);
            }
            var teacherConflicts = (from x in db.LessonBlocks
                                    where choosenBlocks.FirstOrDefault().Teacher.Id == x.Teacher.Id
                                    select x).ToList();
            foreach (var item in teacherConflicts)
            {
                if (conflictLessons.Any(x => x.Id == item.Id) == false && mainScheme.LessonBlocks.Any(x => x.Id == item.Id) == false)
                    conflictLessons.Add(item);
            }
            //******************************

            ConflictService service = new ConflictService();

            //Original, out commented for testing
            //List<DateTime> availableDates = service.FindAHoleInScheme(mainScheme, conflictLessons, choosenBlocks.ToList(), DateTime.Today < mainScheme.SemesterStart ? mainScheme.SemesterStart : DateTime.Today);
            List<DateTime> availableDates = service.FindAHoleInScheme(mainScheme, conflictLessons, choosenBlocks.ToList(), new DateTime(2014,5,26));

            //******************************
            // Do something with the availableDates

            //******************************

            return null;
        }

        public ActionResult SetLessonBehindOwnLesson(string blockIdsString)
        {
            string[] block = blockIdsString.Split(',');
            int[] blockIds = ConvertStringArraytoInt(block);

            //******************************
            // Get the lessonblocks of the id's
            var choosenBlocks = db.LessonBlocks.Where(lb => blockIds.Contains(lb.Id));
            //******************************

            //******************************
            // Get the mainScheme
            var mainScheme = db.Schemes.Where(sc => sc.LessonBlocks.FirstOrDefault().Id == choosenBlocks.FirstOrDefault().Id).SingleOrDefault();
            //Scheme mainScheme = (from s in db.Schemes
            //                    where s.LessonBlocks.Contains(choosenBlocks[0])
            //                    select s).FirstOrDefault();
            //******************************

            //******************************
            // Gets the lessonblocks which may conflict
            List<LessonBlock> conflictLessons = (from x in db.LessonBlocks
                                                 where choosenBlocks.FirstOrDefault().Teacher.Id == x.Teacher.Id
                                                select x).ToList();
            //******************************

            ConflictService service = new ConflictService();

            List<DateTime> availableDates = service.setLessonBehindOwnLesson(mainScheme, conflictLessons, choosenBlocks.ToList(), new DateTime(2014, 5, 26));

            //******************************
            // Do something with the availableDates

            //******************************

            return null;
        }

        public ActionResult SwitchWithOtherTeacher(string blockIdsString, string choosenTeacherId)
        {
            string[] block = blockIdsString.Split(',');
            int[] blockIds = ConvertStringArraytoInt(block);

            //******************************
            // Get the lessonblocks of the id's
            var choosenBlocks = db.LessonBlocks.Where(lb => blockIds.Contains(lb.Id));
            //******************************

            //******************************
            // Get the mainScheme
            var mainScheme = db.Schemes.Where(sc => sc.LessonBlocks.FirstOrDefault().Id == choosenBlocks.FirstOrDefault().Id).SingleOrDefault();
            //Scheme mainScheme = (from s in db.Schemes
            //                    where s.LessonBlocks.Contains(choosenBlocks[0])
            //                    select s).FirstOrDefault();
            //******************************

            //******************************
            // Get the teacher object
            int id = Convert.ToInt32(choosenTeacherId);
            var otherTeacher = db.Teachers.Where(x => x.Id == id).SingleOrDefault();
            //var otherTeacher = (from t in db.Teachers
            //                        where t.Id == Convert.ToInt16(choosenTeacherId)
            //                       select t).FirstOrDefault();
            //******************************

            // Gets the lessonblocks which may conflict
            List<LessonBlock> conflictLessons = (from x in db.LessonBlocks
                                                 where choosenBlocks.FirstOrDefault().Teacher.Id == x.Teacher.Id
                                                 //where otherTeacher.Id == x.Teacher.Id
                                                select x).ToList();
            //******************************

            ConflictService service = new ConflictService();

            List<DateTime> availableDates = service.switchWithOtherTeacher(mainScheme, conflictLessons, choosenBlocks.ToList(), otherTeacher, DateTime.Today);

            //******************************
            // Do something with the availableDates

            //******************************

            return null;
        }

        private int[] ConvertStringArraytoInt(string[] item)
        {
            int[] result = new int[item.Length];
            for (int i = 0; i < item.Length; i++)
            {
                result[i] = (item[i].Equals("")) ? 0 : Int32.Parse(item[i]);
            }
            return result;
        }
	}
}