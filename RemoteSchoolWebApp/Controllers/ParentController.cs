using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemoteSchoolWebApp.Data;
using RemoteSchoolWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using SixLabors.ImageSharp.Formats;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            if (CheckIfParentExists() is false)
            {
                return RedirectToAction("ParentInformation");
            }
            if (CheckIfStudentExists() is false)
            {
                return RedirectToAction("AddStudent");
            }
            int unreadMessagesCount = CheckForUnreadMessages();
            if (unreadMessagesCount > 0)
            {
                ViewBag.Message = string.Format("Liczba wiadomosci do przeczytania: " + unreadMessagesCount.ToString());
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            Student student = _schoolContext.Students.Include(x => x.Grades).SingleOrDefault(x => x.ParentId == parent.Id);
            student.Grades.ForEach(x => x.Assignment = _schoolContext.Assignments.SingleOrDefault(y => y.Id == x.AssignmentId));

            return View(student);
        }

        public IActionResult ParentInformation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ParentInformation(string parentFirstName, string parentLastName)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            parent.FirstName = parentFirstName;
            parent.LastName = parentLastName;

            _schoolContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile image)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            Student student = _schoolContext.Students.SingleOrDefault(x => x.ParentId == parent.Id);

            using (var img = Image.Load(image.OpenReadStream(), out IImageFormat format))
            {
                img.Mutate(img => img.AutoOrient());
                int imgHeight = img.Height;
                float imgWidth = img.Width;
                float ratio = 360 / imgWidth;
                int newHeight = (int)(imgHeight * ratio);
                img.Mutate(x => x.Resize(360, newHeight));
                using (var memoryStream = new MemoryStream())
                {
                    img.Save(memoryStream, format);
                    student.Image = memoryStream.ToArray();
                }
            }

            await _schoolContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Messages()
        {
            if (CheckIfParentExists() is false)
            {
                return RedirectToAction("ParentInformation");
            }
            if (CheckIfStudentExists() is false)
            {
                return RedirectToAction("AddStudent");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            var messages = _schoolContext.Messages.Where(x => x.ParentId == parent.Id).OrderByDescending(x => x.Id).ToList();
            foreach (Message message in messages)
            {
                message.IsRead = true;
            }
            _schoolContext.SaveChanges();
            parent.Messages = messages;
            return View(parent);
        }

        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddStudent(string studentFirstName, string studentLastName)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            Student student = new Student
            {
                FirstName = studentFirstName,
                LastName = studentLastName,
                ParentId = parent.Id,
                ClassId = parent.ClassId
            };
            _schoolContext.Students.Add(student);
            _schoolContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult AssignmentDetails(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            Student student = _schoolContext.Students.SingleOrDefault(x => x.ParentId == parent.Id);
            List<SelectListItem> studentGrade = new List<SelectListItem>();
            var assignment = _schoolContext.Assignments.FirstOrDefault(x => x.Id == id);
            var grade = _schoolContext.Grades.SingleOrDefault(x => x.AssignmentId == assignment.Id && x.StudentId == student.Id);
            studentGrade.Add(new SelectListItem(grade.Value, grade.Value));
            List<Student> students = new List<Student>();
            students.Add(student);
            var AssignmentStudentsVM = new AssignmentStudentsViewModel
            {
                Assignment = assignment,
                PossibleGrades = studentGrade,
                Students = students
            };
            if (AssignmentStudentsVM.Assignment is null)
            {
                return NotFound();
            }
            return View(AssignmentStudentsVM);
        }

        public int CheckForUnreadMessages()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            List<Message> messages = _schoolContext.Messages.Where(x => x.IsRead == false && x.ParentId == parent.Id).ToList();
            return messages.Count;
        }

        public IActionResult DeleteStudent()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            Student student = _schoolContext.Students.SingleOrDefault(x => x.ParentId == parent.Id);
            _schoolContext.Students.Remove(student);
            _schoolContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public bool CheckIfParentExists()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            if (parent.FirstName is null || parent.LastName is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckIfStudentExists()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            return _schoolContext.Students.Where(x => x.ParentId == parent.Id).Any();
        }
    }
}
