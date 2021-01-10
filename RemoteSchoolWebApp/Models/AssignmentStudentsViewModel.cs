using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class AssignmentStudentsViewModel
    {
        public List<Student> Students { get; set; }
        public Assignment Assignment { get; set; }
        public List<SelectListItem> PossibleGrades { get; set; }
    }
}
