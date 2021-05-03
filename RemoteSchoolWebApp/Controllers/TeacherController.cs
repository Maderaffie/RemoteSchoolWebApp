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
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            var classView = new Class
            {
                Name = _schoolContext.Classes.SingleOrDefault(x => x.Id == teacher.ClassId).Name,
                Students = await _schoolContext.Students.Where(x => x.ClassId == teacher.ClassId).ToListAsync()
            };

            return View(classView);
        }

        public IActionResult TeacherInformation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TeacherInformation(string teacherFirstName, string teacherLastName)
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
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            var classView = new Class
            {
                Assignments = await _schoolContext.Assignments.Where(x => x.ClassId == teacher.ClassId).OrderByDescending(x => x.Date).ToListAsync()
            };

            return View(classView);
        }

        public IActionResult CreateAssignment()
        {
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
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
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            if (id == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            List<SelectListItem> possibleGrades = new List<SelectListItem>();
            possibleGrades.Add(new SelectListItem("", ""));

            for (int i = 1; i <= 6; i++)
            {
                possibleGrades.Add(new SelectListItem(i.ToString(), i.ToString()));
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
        [ValidateAntiForgeryToken]
        public IActionResult AssignmentDetails(List<Student> students)
        {
            foreach (Student student in students)
            {
                var newGrade = student.Grades[0];
                var grade = _schoolContext.Grades.Find(newGrade.AssignmentId, newGrade.StudentId);
                if (newGrade.Value == 0)
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
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
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
        [ValidateAntiForgeryToken]
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

        public IActionResult StudentDetails(int? id)
        {
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            if (id == null)
            {
                return NotFound();
            }

            Student student = _schoolContext.Students.Include(x => x.Grades).SingleOrDefault(x => x.Id == id);
            student.Grades.ForEach(x => x.Assignment = _schoolContext.Assignments.SingleOrDefault(y => y.Id == x.AssignmentId));
            return View(student);
        }

        public async Task<IActionResult> Messages()
        {
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            var uniqueMessages = await _schoolContext.Messages.Where(x => x.TeacherId == teacher.Id).GroupBy(x => x.Content).Select(x => x.Key).ToListAsync();
            List<Message> messages = new List<Message>();
            foreach (string uniqueMessage in uniqueMessages)
            {
                Message message = _schoolContext.Messages.FirstOrDefault(x => x.Content == uniqueMessage);
                messages.Add(message);
            }
            messages = messages.OrderByDescending(x => x.Id).ToList();
            teacher.Messages = messages;
            return View(teacher);
        }

        public IActionResult CreateMessage()
        {
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            List<SelectListItem> parents = new List<SelectListItem>();
            parents.Add(new SelectListItem("All", "-1"));

            var parentList = _schoolContext.Parents.Where(x => x.ClassId == teacher.ClassId).ToList();
            foreach(Parent parent in parentList)
            {
                string fullName = parent.FirstName + " " + parent.LastName;
                parents.Add(new SelectListItem(fullName, parent.Id.ToString()));
            }

            var messageParentVM = new MessageParentViewModel
            {
                Message = new Message(),
                Parents = parents
            };

            return View(messageParentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateMessage(Message message)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);
            
            if (message.ParentId == -1) //Message to each parent
            {
                var parentList = _schoolContext.Parents.Where(x => x.ClassId == teacher.ClassId);
                foreach(Parent parent in parentList)
                {
                    Message messageToParent = new Message
                    {
                        Subject = message.Subject,
                        Content = message.Content,
                        ParentId = parent.Id,
                        TeacherId = teacher.Id
                    };
                    _schoolContext.Messages.Add(messageToParent);
                }
            }
            else if (message.ParentId is null)
            {
                return NotFound();
            }
            else
            {
                message.TeacherId = teacher.Id;
                _schoolContext.Messages.Add(message);
            }
            
            _schoolContext.SaveChanges();
            return RedirectToAction("Messages");
        }

        public IActionResult MessageDetails(int? id)
        {
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            if (id is null)
            {
                return NotFound();
            }
            Message message = _schoolContext.Messages.FirstOrDefault(x => x.Id == id);
            List<Message> messages = _schoolContext.Messages.Where(x => x.Content == message.Content).Include(x => x.Parent).ToList();
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);
            teacher.Messages = messages;
            return View(teacher);
        }

        public IActionResult Reports()
        {
            if (CheckIfTeacherExists() is false)
            {
                return RedirectToAction("TeacherInformation");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);

            var classView = new Class
            {
                Assignments = _schoolContext.Assignments.Where(x => x.ClassId == teacher.ClassId).Include(y => y.Grades).OrderByDescending(x => x.Date).ToList()
            };

            foreach (var assignment in classView.Assignments)
            {
                if (assignment.Grades is null || assignment.Grades.Count == 0)
                {
                    assignment.GradesCount = 0;
                    assignment.Average = 0;
                }
                else
                {
                    assignment.GradesCount = assignment.Grades.Count;
                    assignment.Average = assignment.Grades.Select(x => x.Value).Average();
                }
            }
            return View(classView);
        }

        public bool CheckIfTeacherExists()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Teacher teacher = _schoolContext.Teachers.SingleOrDefault(x => x.Email == userEmail);
            if (teacher.FirstName is null || teacher.LastName is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
