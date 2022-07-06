using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using P225Allup.Areas.Manage.ViewModels.AccountViewModels;
using P225Allup.Areas.Manage.ViewModels.UserViewModels;
using P225Allup.Models;
using P225Allup.ViewModels.AccountViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P225Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles ="SuperAdmin")]
    public class UserController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public UserController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index(bool? status)
        {
            List<AppUser> query = await _userManager.Users.Where(u=>u.UserName != User.Identity.Name).ToListAsync();

            if (status != null)
            {
                query = query.Where(b => b.IsDeActive == status).ToList();
            }

            ViewBag.Status = status;

            foreach (AppUser item in query)
            {
                var roles = await _userManager.GetRolesAsync(item);
                item.Role = roles[0];
            }

            return View(query);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await  _roleManager.Roles.ToListAsync();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserVM user)
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            if (!ModelState.IsValid) return View();

            AppUser appUser = new AppUser
            {
                Role= user.Role,
                Name = user.Name,
                SurName = user.SurName,
                FatherName = user.FatherName,
                Age = user.Age,
                Email = user.Email,
                UserName = user.UserName
            };

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, user.Password);

            if (!identityResult.Succeeded)
            {
                foreach (var item in identityResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

                return View();
            }

            await _userManager.AddToRoleAsync(appUser, user.Role);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id,ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid) return View();

            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            //string emailconfirmToke = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            await _userManager.ResetPasswordAsync(appUser, token, resetPasswordVM.Password);
            //await _userManager.ConfirmEmailAsync(appUser, emailconfirmToke);

            return RedirectToAction("index");
        }
    }
}
