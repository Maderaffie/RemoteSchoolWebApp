using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class RegisterViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Login { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(4)]
        public string Password { get; set; }
        [Required]
        public string ClassName { get; set; }
        [Required]
        public string Role { get; set; }
        public List<SelectListItem> Classes { get; set; }
    }
}
