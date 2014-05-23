﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SkemaSystem.Models
{
    public class Subject
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}