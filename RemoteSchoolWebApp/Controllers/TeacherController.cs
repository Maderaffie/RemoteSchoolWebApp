using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemoteSchoolWebApp.Data;
using RemoteSchoolWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly SchoolContext _schoolContext;

        public TeacherController(SchoolContext schoolContext)
        {
            _schoolContext = schoolContext;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            var classView = new Class
            {
                Students = await _schoolContext.Students.Where(x => x.ClassId == teacher.ClassId).ToListAsync()
            };

            return View(classView);
        }

        public IActionResult TeacherInformation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TeacherInformation(string teacherFirstName, string teacherLastName)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);
            teacher.FirstName = teacherFirstName;
            teacher.LastName = teacherLastName;

            _schoolContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Exercises()
        {
            return View();
        }

        public IActionResult Raport()
        {
            return View();
        }
    }
}
