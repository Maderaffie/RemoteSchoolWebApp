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
            Student student = _schoolContext.Students.Include(x => x.Grades).SingleOrDefault(x => x.ParentId == parent.Id);
            student.Grades.ForEach(x => x.Assignment = _schoolContext.Assignments.SingleOrDefault(y => y.Id == x.AssignmentId));

            return View(student);
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
    }
}
