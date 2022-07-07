using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P129Allup.Areas.Manage.ViewModels.AccountViewModels;
using P129Allup.DAL;
using P129Allup.Models;
using P129Allup.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P129Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UserController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index(int? status, int page = 1)
        {
            IQueryable<AppUser> query = _userManager.Users.Where(u=>u.UserName != User.Identity.Name);

            if (status != null && status > 0)
            {
                if (status == 1)
                {
                    query = query.Where(b => b.IsDeleted);
                }
                else if (status == 2)
                {
                    query = query.Where(b => !b.IsDeleted);
                }
            }
            int itemCount = int.Parse(_context.Settings.FirstOrDefault(s => s.Key == "PageItemCount").Value);

            ViewBag.Status = status;

            return View(PageNatedLisd<AppUser>.Create(query, page, itemCount));
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
        public async Task<IActionResult> ResetPassword(string id, ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid) return View();

            if (id == null) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            //if (await _userManager.CheckPasswordAsync(appUser, resetPasswordVM.Password)) return BadRequest();

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            //string emailConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            IdentityResult identityResult = await _userManager.ResetPasswordAsync(appUser, token, resetPasswordVM.Password);
            //await _userManager.ChangeEmailAsync(appUser, newEmail, emailConfirm);

            if (!identityResult.Succeeded)
            {
                foreach (var item in identityResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

                return View();
            }

            return RedirectToAction("Index");
        }
    }
}
