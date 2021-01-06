using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RemoteSchoolWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
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
            Debug.WriteLine(email);
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
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Parent");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (!signInResult.Succeeded)
            {
                Debug.WriteLine("Login failed.");
                return RedirectToAction(nameof(Register));
            } 
            else
            {
                return RedirectToAction(nameof(Index));
            }
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
