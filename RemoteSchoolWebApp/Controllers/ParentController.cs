using Microsoft.AspNetCore.Mvc;
using RemoteSchoolWebApp.Data;
using RemoteSchoolWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Controllers
{
    public class ParentController : Controller
    {
        private readonly SchoolContext _schoolContext;

        public ParentController(SchoolContext schoolContext)
        {
            _schoolContext = schoolContext;
        }
        public IActionResult Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            Student student = _schoolContext.Students.SingleOrDefault(x => x.ParentId == parent.Id);
            return View(student);
        }
    }
}
