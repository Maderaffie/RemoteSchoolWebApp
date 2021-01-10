﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class Grade : EntityBase
    {
        [Required]
        public string Value { get; set; }

        [ForeignKey("Student")]
        public int? StudentId { get; set; }
        public Student Student { get; set; }
        [ForeignKey("Assignment")]
        public int? AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

    }

    public enum GradeValue
    {
        A = 5,
        B = 4,
        C = 3,
        D = 2,
        E = 1,
    }
}
