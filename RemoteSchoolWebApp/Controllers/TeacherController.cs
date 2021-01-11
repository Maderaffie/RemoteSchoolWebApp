using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Assignments()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            var classView = new Class
            {
                Assignments = await _schoolContext.Assignments.Where(x => x.ClassId == teacher.ClassId).ToListAsync()
            };

            return View(classView);
        }

        public IActionResult CreateAssignment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment([Bind("Id,Description,Date")] Assignment assignment)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            if (ModelState.IsValid)
            {
                assignment.ClassId = teacher.ClassId;
                _schoolContext.Add(assignment);
                await _schoolContext.SaveChangesAsync();
                return RedirectToAction("Assignments");
            }
            return View(assignment);
        }

        public async Task<IActionResult> AssignmentDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            List<SelectListItem> possibleGrades = new List<SelectListItem>();
            possibleGrades.Add(new SelectListItem("", ""));

            foreach (GradeValue gradeValue in (GradeValue[])Enum.GetValues(typeof(GradeValue)))
            {
                Debug.WriteLine(gradeValue);
                possibleGrades.Add(new SelectListItem(gradeValue.ToString(), gradeValue.ToString()));
            }

            var AssignmentStudentsVM = new AssignmentStudentsViewModel
            {
                Assignment = await _schoolContext.Assignments.FirstOrDefaultAsync(x => x.Id == id),
                Students = await _schoolContext.Students.Where(x => x.ClassId == teacher.ClassId)
                                               .Include(z => z.Grades.Where(y => y.AssignmentId == id))
                                               .OrderBy(u => u.LastName).ToListAsync(),
                PossibleGrades = possibleGrades
            };

            if (AssignmentStudentsVM.Assignment == null)
            {
                return NotFound();
            }

            return View(AssignmentStudentsVM);
        }

        [HttpPost]
        public IActionResult AssignmentDetails(List<Student> students)
        {
            foreach (Student student in students)
            {
                var newGrade = student.Grades[0];
                var grade = _schoolContext.Grades.Find(newGrade.AssignmentId, newGrade.StudentId);
                if (newGrade.Value is null)
                {
                    if (grade is not null)
                    {
                        _schoolContext.Remove(grade);
                        _schoolContext.SaveChanges();
                    }
                    continue;
                }
                
                if (grade is null)
                {
                    _schoolContext.Grades.Add(newGrade);
                } 
                else
                {
                    grade.Value = newGrade.Value;
                }
                _schoolContext.SaveChanges();
            }
            return RedirectToAction("Assignments");
        }

        public IActionResult EditAssignment(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }
            var assignment = _schoolContext.Assignments.Find(id);
            if (assignment is null)
            {
                return NotFound();
            }
            return View(assignment);
        }

        [HttpPost]
        public async Task<IActionResult> EditAssignment(int id, [Bind("Id, ClassId, Description,Date")] Assignment assignment)
        {
            if (id != assignment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _schoolContext.Update(assignment);
                    await _schoolContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_schoolContext.Assignments.Any(x => x.Id == assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Assignments");
            }
            return View(assignment);
        }

        public IActionResult Raport()
        {
            return View();
        }
    }
}
