using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return View();
            }
            if (User.IsInRole("Teacher"))
            {
                return RedirectToAction("Index", "Teacher");
            } 
            else
            {
                return RedirectToAction("Index", "Parent");
            }
        }

        public IActionResult Register(string error = "")
        {
            List<SelectListItem> possibleClasses = new List<SelectListItem>();
            List<Class> classes = _schoolContext.Classes.ToList();
            foreach (Class @class in classes)
            {
                possibleClasses.Add(new SelectListItem(@class.Name, @class.Name));
            }
            var registerVM = new RegisterViewModel
            {
                Classes = possibleClasses
            };

            if (error != "")
            {
                ViewData["error"] = error;
            }
            else
            {
                ViewData["error"] = null;
            }

            return View(registerVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            string email = registerViewModel.Login;
            string password = registerViewModel.Password;
            string className = registerViewModel.ClassName;
            string role = registerViewModel.Role;

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

            if (_schoolContext.Parents.Any(x => x.Email == email) || _schoolContext.Teachers.Any(x => x.Email == email))
            {
                return RedirectToAction("Register", new { error = "Email address is already in use!" });
            }

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                Debug.WriteLine("Creating user failed.");
                return RedirectToAction(nameof(Register));
            }

            if (role.Equals("Teacher"))
            {
                int classId = _schoolContext.Classes.SingleOrDefault(x => x.Name == className).Id;
                if (_schoolContext.Teachers.Any(x => x.ClassId == classId))
                {
                    return RedirectToAction("Register", new {error = "There is a teacher in this class!"});
                }
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
                return RedirectToAction("ParentInformation", "Parent");
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

        public IActionResult Login(string error = "")
        {
            if (error != "")
            {
                ViewData["error"] = error;
            }
            else
            {
                ViewData["error"] = null;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(RegisterViewModel registerViewModel)//(string email, string password)
        {
            string email = registerViewModel.Login;
            string password = registerViewModel.Password;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login", new { error = "User does not exist!" });
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);
            IList<string> roles = await _signInManager.UserManager.GetRolesAsync(user);
            var userRole = roles[0];
            if (!signInResult.Succeeded)
            {
                return RedirectToAction("Login", new { error = "Wrong email or password!" });
            }

            return RedirectToAction("Index", userRole);
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
