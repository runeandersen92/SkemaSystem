﻿using SkemaSystem.Models;
using SkemaSystem.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SkemaSystem.Controllers
{

    [RouteArea("Admin", AreaPrefix = "admin")]
    [RoutePrefix("optionalSubjects")]
    [Route("{action=index}/{id?}")]
    public class OptionalSubjectsController : BaseController
    {

        [Route("")]
        public ActionResult Index()
        {
            return View(db.Schemes.Where(x => x.ClassModel == null)); // Show a list of schemes without classes => that means it is optional subjects! (valgfag in danish)
        }

        public ActionResult Create()
        {
            HashSet<string> years = new HashSet<string>();
            foreach (Scheme scheme in db.Schemes)
            {
                years.Add(scheme.YearString);
            }

            ViewBag.Years = years;

            ViewBag.Education = (from e in db.Educations
                                 where e.Name.Equals("DMU")
                                 select e).SingleOrDefault();

            ViewBag.Subjects = from s in db.Subjects
                               where s.OptionalSubject
                               select s;

            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            if (string.IsNullOrEmpty(form["semester"]) || string.IsNullOrEmpty(form["name"]) || string.IsNullOrEmpty(form["year"]))
            {
                // If any of the required fields are empty!
            }

            string[] subjectBlocksCountArray = form["subjectBlockCount"].Split(',');
            string[] subjectIdArray = form["subjectId"].Split(',');
            string[] subjectUses = form["subjectUse"].Split(',');

            for (int i = 0; i < subjectBlocksCountArray.Count(); i++)
            {
                if (subjectBlocksCountArray[i].Equals("") && subjectUses.Any(x => x.Equals(subjectIdArray[i])))
                {
                    // A chosen subject has no block count value!
                }
            }

            // If no errors occurs - convert the arrays to int arrays, now we know that every index has correct values!
            int[] subjectIds = ConvertStringArraytoInt(subjectIdArray);
            int[] subjectBlockCounts = ConvertStringArraytoInt(subjectBlocksCountArray);

            int semesterId = Int32.Parse(form["semester"]);
            string name = form["name"];
            string year = form["year"];

            Semester semester = db.Semesters.Where(x=> x.Id == semesterId).SingleOrDefault();


            // Get all the subjects chosen along with it's values!
            List<SemesterSubjectBlock> optionalSubjectBlocks = new List<SemesterSubjectBlock>();
            for (int i = 0; i < subjectIds.Count(); i++)
            {
                int subjectId = subjectIds[i];
                if (subjectUses.Contains("" + subjectId))
                {
                    Subject su = db.Subjects.Where(x=>x.Id == subjectId).SingleOrDefault();
                    int blocksCount = subjectBlockCounts[i];
                    // The subject has been chosen from the list, and has to be created!
                    optionalSubjectBlocks.Add(new SemesterSubjectBlock { BlocksCount = blocksCount, Subject = su });
                }
            }

            // Get all schemes from the conflict list, and make sure this new scheme has those schemes in it and that the other schemes has this one in their conflicts-list as well!
            int[] conflictSchemeIds = ConvertStringArraytoInt(form["conflictScheme"].Split(','));
            List<Scheme> conflictSchemes = new List<Scheme>();
            foreach (int schemeId in conflictSchemeIds) {
                Scheme s = db.Schemes.Where(x => x.Id == schemeId).SingleOrDefault();
                conflictSchemes.Add(s);
            }

            // random scheme to get the startdate and enddate of this particular semester!
            // If the randomScheme is empty, then we aren't able to start an optionalSubject! Error message has to be shown!
            Scheme randomScheme = db.Schemes.Where(x => x.YearString.Equals(year) && x.Semester.Id == semester.Id).FirstOrDefault();
            if (randomScheme != null)
            {
                //List<ConflictScheme> cSchemeList = new List<ConflictScheme>();

                    //cSchemeList.Add(new ConflictScheme { scheme = conflictSchemes });

                    Scheme newScheme = new Scheme { Name = name, OptionalSubjectBlockList = optionalSubjectBlocks, ConflictSchemes = conflictSchemes, Semester = semester, SemesterStart = randomScheme.SemesterStart, SemesterFinish = randomScheme.SemesterFinish, YearString = "" };

                //List<Scheme> temp = new List<Scheme>();
                //temp.Add(newScheme);

                foreach (Scheme otherScheme in conflictSchemes)
                {
                    otherScheme.ConflictSchemes.Add(newScheme);
                }
                
                db.Schemes.Add(newScheme);
                db.SaveChanges();
            }
            else
            {
                // ERROR: No start and end date!
            }

            return RedirectToAction("Index");
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

        public ActionResult UpdateConflictsWith(string year, string semester)
        {
            int semesterId = Int32.Parse(semester);
            var schemes = from s in db.Schemes
                          where s.Semester.Id == semesterId && s.YearString.Equals(year)
                          select s;

            return PartialView("_ConflictSchemes", schemes);
        }

        public ActionResult CreateOptionalSubject(string name)
        {

            Subject su = new Subject { Name = name, OptionalSubject = true, Education = db.Educations.Where(x => x.Name.Equals("DMU")).SingleOrDefault() };
            db.Subjects.Add(su);
            db.SaveChanges();

            ViewBag.Subjects = from s in db.Subjects
                               where s.OptionalSubject
                               select s;

            return PartialView("_OptionalSubjectsList");
        }
       
        public ActionResult Delete(int id)
        {
            var scheme = (from s in db.Schemes
                          where s.Id == id
                          select s).FirstOrDefault();

            if (scheme != null)
            {
                scheme.OptionalSubjectBlockList.Clear();
                scheme.SubjectDistBlocks.Clear();
                foreach (var item in scheme.ConflictSchemes)
                {
                    item.ConflictSchemes.Remove(scheme);
                }
                scheme.ConflictSchemes = null;
                scheme.Semester = null;
                db.Schemes.Remove(scheme);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Scheme scheme = db.Schemes.Find(id);
            if (scheme == null)
            {
                return HttpNotFound();
            }

            ViewBag.Schemes = from s in db.Schemes
                          where s.Semester.Id == scheme.Semester.Id && s.YearString.Equals(scheme.YearString)
                          select s;

            ViewBag.Subjects = from s in db.Subjects
                               where s.OptionalSubject
                               select s;

            return View(scheme);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection form)
        {
            int schemeId = Int32.Parse(form["schemeId"]);
            Scheme scheme = db.Schemes.SingleOrDefault(x => x.Id == schemeId);
            Semester semester = scheme.Semester; // ONLY to get the value from the database!
            List<SubjectDistBlock> s2 = scheme.SubjectDistBlocks; // ONLY to get the value from the database!
            if (string.IsNullOrEmpty(form["name"]))
            {
                // If any of the required fields are empty!
            }

            scheme.Name = form["name"];

            string[] subjectBlocksCountArray = form["subjectBlockCount"].Split(',');
            string[] subjectIdArray = form["subjectId"].Split(',');
            string[] subjectUses = form["subjectUse"].Split(',');

            for (int i = 0; i < subjectBlocksCountArray.Count(); i++)
            {
                if (subjectBlocksCountArray[i].Equals("") && subjectUses.Any(x => x.Equals(subjectIdArray[i])))
                {
                    // A chosen subject has no block count value!
                }
            }

            // If no errors occurs - convert the arrays to int arrays, now we know that every index has correct values!
            int[] subjectIds = ConvertStringArraytoInt(subjectIdArray);
            int[] subjectBlockCounts = ConvertStringArraytoInt(subjectBlocksCountArray);

            // Get all the subjects chosen along with it's values!
            List<SemesterSubjectBlock> optionalSubjectBlocks = new List<SemesterSubjectBlock>();
            for (int i = 0; i < subjectIds.Count(); i++)
            {
                int subjectId = subjectIds[i];
                if (subjectUses.Contains("" + subjectId))
                {
                    Subject su = db.Subjects.Where(x => x.Id == subjectId).SingleOrDefault();

                    int blocksCount = subjectBlockCounts[i];
                    // The subject has been chosen from the list, and has to be created!
                    optionalSubjectBlocks.Add(new SemesterSubjectBlock { BlocksCount = blocksCount, Subject = su });
                }
            }

            // Get all schemes from the conflict list, and make sure this new scheme has those schemes in it and that the other schemes has this one in their conflicts-list as well!
            int[] conflictSchemeIds = ConvertStringArraytoInt(form["conflictScheme"].Split(','));
            List<Scheme> conflictSchemes = new List<Scheme>();
            foreach (int sId in conflictSchemeIds)
            {
                Scheme s = db.Schemes.Where(x => x.Id == sId).SingleOrDefault();
                conflictSchemes.Add(s);
            }

            
            // Update schemes optionalSubject list!
            List<SemesterSubjectBlock> newOptionalSubjectList = new List<SemesterSubjectBlock>();
            foreach (SemesterSubjectBlock ssb in optionalSubjectBlocks)
            {
                if (scheme.OptionalSubjectBlockList.Any(x => x.Subject.Id == ssb.Subject.Id))
                {
                    ssb.Id = scheme.OptionalSubjectBlockList.FirstOrDefault(x => x.Subject.Id == ssb.Subject.Id).Id;
                }
                newOptionalSubjectList.Add(ssb);
            }
            scheme.OptionalSubjectBlockList = newOptionalSubjectList;

            scheme.SubjectDistBlocks = (scheme.SubjectDistBlocks.Where(x => (scheme.OptionalSubjectBlockList.Any(q => q.Subject.Id == x.Subject.Id)))).ToList();


            // Update schemes conflictSchemes list
            List<Scheme> newConflictList = new List<Scheme>();
            foreach (Scheme s in scheme.ConflictSchemes)
            {
                if (!conflictSchemes.Any(x => x.Id == s.Id))
                {
                    // This particular scheme is in the old list of schemes, but not in the new! Remove it from both this and this from the other schemes list!
                    s.ConflictSchemes.Remove(scheme);
                }
                else
                {
                    newConflictList.Add(s);
                }
            }
            scheme.ConflictSchemes = newConflictList;

            foreach (Scheme s in conflictSchemes)
            {
                if (!scheme.ConflictSchemes.Any(x=> x.Id == s.Id)) {
                    // If this scheme doesn't already has the loop-scheme in its conflict list - add it now!
                    scheme.ConflictSchemes.Add(s);
                }
                if (!s.ConflictSchemes.Any(x => x.Id == scheme.Id))
                {
                    // If the other scheme doesn't already has this scheme in its conflict list - add it now!
                    s.ConflictSchemes.Add(scheme);
                }
            }

            

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult SubjectDistribution(int id)
        {
            Scheme scheme = db.Schemes.Single(x => x.Id == id);

            Education edu = db.Educations.Where(e=> e.Semesters.Any(s=> s.Id == scheme.Semester.Id)).FirstOrDefault();
            ViewBag.Teachers = db.Teachers.Where(x => x.Educations.Any(e => e.Id == edu.Id)).ToList();

            return View(scheme);
        }

        [HttpPost]
        public PartialViewResult AddSubjectDistBlock(int scheme, int add_subject, int add_teacher, int add_blockscount)
        {
            Scheme theScheme = db.Schemes.Single(x => x.Id == scheme);

            Education edu = db.Educations.Where(e => e.Semesters.Any(s => s.Id == theScheme.Semester.Id)).FirstOrDefault();
            ViewBag.Teachers = db.Teachers.Where(x => x.Educations.Any(e => e.Id == edu.Id)).ToList();

            if (add_blockscount > 0 && add_blockscount < 100 && theScheme.AddLessonBlock(db.Teachers.SingleOrDefault(x => x.Id == add_teacher), db.Subjects.SingleOrDefault(x => x.Id == add_subject), add_blockscount))
            {
                db.SaveChanges();
            }
            else
            {
                // Didn't succeed!
                ViewBag.add_subject = add_subject;
                ViewBag.add_teacher = add_teacher;
                ViewBag.add_blockscount = add_blockscount;
                if (add_blockscount < 0 || add_blockscount > 100)
                {
                    ViewBag.Error = "- Antal blokke skal være mellem 0 og 100.";
                }
                else
                {
                    ViewBag.Error = "- Der er ikke nok ledige blokke på semestret til, at udføre denne handling.";
                }
            }
            return PartialView("_SchemeSubjectDistribution", theScheme);
        }
	}
}