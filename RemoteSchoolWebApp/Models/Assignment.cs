using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class Assignment : EntityBase
    {
        [Required]
        [StringLength(256)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [ForeignKey("Class")]
        public int? ClassId { get; set; }
        public Class Class { get; set; }

        public List<Grade> Grades { get; set; }

        [NotMapped]
        public string Average { get; set; }
        [NotMapped]
        public int GradesCount { get; set; }
    }
}
