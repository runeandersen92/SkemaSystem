﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SkemaSystem.Models
{
    public class ClassModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string ClassName { get; set; }

        public virtual Education Education { get; set; }

        public virtual List<Scheme> ActiveSchemes { get; set; }

        public ClassModel()
        {
            ActiveSchemes = new List<Scheme>();
        }

        public bool CreateNewSemester() {
            Semester nextSemester = NextSemester();
            if (nextSemester != null)
            {
                ActiveSchemes.Add(new Scheme { Semester = nextSemester, ClassModel = this });
                return true;
            }
            return false;
        }

        public Semester NextSemester()
        {
            if (this.ActiveSchemes.Count > 0)
            {
                return (from s in this.Education.Semesters
                                    orderby s.Number ascending
                                    where s.Number > this.ActiveSchemes[this.ActiveSchemes.Count - 1].Semester.Number
                                    select s).FirstOrDefault();
            }
            else
            {
                return this.Education.Semesters[0];
            }
        }
    }
}