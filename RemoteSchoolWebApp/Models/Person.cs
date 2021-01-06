﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public abstract class Person : EntityBase
    {
        [Required]
        [StringLength(16)]
        [RegularExpression(@"[A-Za-z]")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(32)]
        [RegularExpression(@"[A-Za-z]")]
        public string LastName { get; set; }
    }
}
