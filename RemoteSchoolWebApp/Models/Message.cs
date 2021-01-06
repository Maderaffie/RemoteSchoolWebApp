using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class Message : EntityBase
    {
        [Required]
        public bool IsRead { get; set; }
        [Required]
        [StringLength(1024)]
        public string Content { get; set; }
        [ForeignKey("Teacher")]
        public int? TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public Parent Parent { get; set; }
    }
}
