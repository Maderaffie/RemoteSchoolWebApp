using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class Parent : Person
    {
        [Required]
        [StringLength(64)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        public Student Student { get; set; }

        [ForeignKey("Class")]
        public int? ClassId { get; set; }
        public Class Class { get; set; }

        public List<Message> Messages { get; set; }
    }
}
