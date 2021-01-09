using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class Student : Person
    {
        public byte[] Image { get; set; }
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public Parent Parent { get; set; }
        [ForeignKey("Class")]
        public int? ClassId { get; set; }
        public Class Class { get; set; }
        public List<Grade> Grades { get; set; }
    }
}
