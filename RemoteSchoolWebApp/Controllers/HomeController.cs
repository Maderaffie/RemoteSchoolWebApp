using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RemoteSchoolWebApp.Data;
using RemoteSchoolWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly SchoolContext _schoolContext;

        public HomeController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, SchoolContext schoolContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _schoolContext = schoolContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string className, string role)
        {
            Regex emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            if (!emailRegex.IsMatch(email))
            {
                Debug.WriteLine("Wrong email format.");
                return RedirectToAction(nameof(Register));
            }

            await CreateRoleIfNotExists("Teacher");
            await CreateRoleIfNotExists("Parent");

            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                Debug.WriteLine("Creating user failed.");
                return RedirectToAction(nameof(Register));
            }

            if (role.Equals("Teacher"))
            {
                await _userManager.AddToRoleAsync(user, "Teacher");
                Teacher teacher = new Teacher();
                teacher.Email = email;
                int currentId = _schoolContext.Classes.SingleOrDefault(x => x.Name == className).Id;
                teacher.ClassId = currentId;
                _schoolContext.Teachers.Add(teacher);
                _schoolContext.SaveChanges();
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Parent");
                Parent parent = new Parent();
                parent.Email = email;
                int currentId = _schoolContext.Classes.SingleOrDefault(x => x.Name == className).Id;
                parent.ClassId = currentId;
                _schoolContext.Parents.Add(parent);
                _schoolContext.SaveChanges();
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (!signInResult.Succeeded)
            {
                Debug.WriteLine("Login failed.");
                return RedirectToAction(nameof(Register));
            }

            if (role.Equals("Teacher"))
                return RedirectToAction("TeacherInformation", "Teacher");
            else
                return RedirectToAction(nameof(ParentInformation));
            
        }

        private async Task CreateRoleIfNotExists(string role)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                var newRole = new IdentityRole
                {
                    Name = role
                };
                await _roleManager.CreateAsync(newRole);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var userRole = User.IsInRole("Parent") ? "Parent" : "Teacher";

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (!signInResult.Succeeded)
            {
                return RedirectToAction(nameof(Login));
            }

            return RedirectToAction("Index", userRole);
        }

        public IActionResult ParentInformation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ParentInformation(string parentFirstName, string parentLastName, 
                                                            string studentFirstName, string studentLastName)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            Parent parent = _schoolContext.Parents.SingleOrDefault(x => x.Email == userEmail);
            parent.FirstName = parentFirstName;
            parent.LastName = parentLastName;

            Student student = new Student
            {
                FirstName = studentFirstName,
                LastName = studentLastName, 
                ParentId = parent.Id,
                ClassId = parent.ClassId
            };
            _schoolContext.Students.Add(student);

            _schoolContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
