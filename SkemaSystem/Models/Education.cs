﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkemaSystem.Models
{
    public class Education
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Teacher> Teachers { get; set; }
        public virtual ICollection<Scheme> Schemes{ get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual List<Semester> Semesters { get; set; }
        public int NumberOfSemesters { get; set; }
        public override string ToString()
        {
            return Name;
        }

        //Semester --> mange

        //lokaler --> mange

        //skema --> én
    }
}