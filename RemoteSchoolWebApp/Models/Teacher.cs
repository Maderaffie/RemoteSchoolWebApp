using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class Teacher : Person
    {
        [Required]
        [StringLength(64)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Login { get; set; }
        [Required]
        [StringLength(32)]
        public string Password { get; set; }

        [ForeignKey("Class")]
        public int? ClassId { get; set; }
        public Class Class { get; set; }
    }
}
