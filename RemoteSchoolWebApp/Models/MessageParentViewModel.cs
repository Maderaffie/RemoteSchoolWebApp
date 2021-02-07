using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class MessageParentViewModel
    {
        public Message Message { get; set; }
        public List<SelectListItem> Parents { get; set; }
        public List<Parent> ParentsList { get; set; }
    }
}
