using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using P129Allup.DAL;
using P129Allup.Models;
using P129Allup.ViewModels.AccountViewModels;
using P129Allup.ViewModels.BasketViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P129Allup.Controllers
{
    public class AccountController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser appUser = new AppUser
            {
                Name = registerVM.Name,
                SurName = registerVM.SurName,
                UserName = registerVM.UserName,
                Email = registerVM.Email
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }

            result =  await _userManager.AddToRoleAsync(appUser, "Member");

            //if (!result.Succeeded)
            //{
            //    foreach (IdentityError error in result.Errors)
            //    {
            //        ModelState.AddModelError("", error.Description);
            //    }

            //    return View();
            //}

            return RedirectToAction("login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            AppUser appUser = await _userManager.Users.Include(u=>u.Baskets).FirstOrDefaultAsync(u=>u.NormalizedEmail == loginVM.Email.Trim().ToUpperInvariant() && !u.IsAdmin && !u.IsDeleted);

            if (appUser == null)
            {
                ModelState.AddModelError("", "Email Or Password Is InCorrect");
                return View(loginVM);
            }

            //if (appUser.IsAdmin)
            //{
            //    ModelState.AddModelError("", "Email Or Password Is InCorrect");
            //    return View(loginVM);
            //}

            if (!await _userManager.CheckPasswordAsync(appUser, loginVM.Password))
            {
                ModelState.AddModelError("", "Email Or Password Is InCorrect");
                return View(loginVM);
            }

            await _signInManager.SignInAsync(appUser, loginVM.RemindMe);

            string basketCoockie = HttpContext.Request.Cookies["basket"];

            if (!string.IsNullOrWhiteSpace(basketCoockie))
            {
                List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basketCoockie);

                List<Basket> baskets = new List<Basket>();

                foreach (BasketVM basketVM in basketVMs)
                {
                    if (appUser.Baskets != null && appUser.Baskets.Count() > 0)
                    {
                        Basket exsitedBasket = appUser.Baskets.FirstOrDefault(b => b.ProductId != basketVM.ProductId);

                        if (exsitedBasket == null)
                        {
                            Basket basket = new Basket
                            {
                                AppUserId = appUser.Id,
                                ProductId = basketVM.ProductId,
                                Count = basketVM.Count
                            };

                            baskets.Add(basket);
                        }
                        else
                        {
                            //exsitedBasket.Count = basketVM.Count;
                            exsitedBasket.Count += basketVM.Count;
                            basketVM.Count = exsitedBasket.Count;
                        }
                    }
                    else
                    {
                        Basket basket = new Basket
                        {
                            AppUserId = appUser.Id,
                            ProductId = basketVM.ProductId,
                            Count = basketVM.Count
                        };

                        baskets.Add(basket);
                    }
                }

                basketCoockie = JsonConvert.SerializeObject(basketVMs);

                HttpContext.Response.Cookies.Append("basket", basketCoockie);

                await _context.Baskets.AddRangeAsync(baskets);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (appUser.Baskets != null && appUser.Baskets.Count() > 0)
                {
                    List<BasketVM> basketVMs = new List<BasketVM>();

                    foreach (Basket basket in appUser.Baskets)
                    {
                        BasketVM basketVM = new BasketVM
                        {
                            ProductId = basket.ProductId,
                            Count = basket.Count
                        };

                        basketVMs.Add(basketVM);
                    }

                    basketCoockie = JsonConvert.SerializeObject(basketVMs);

                    HttpContext.Response.Cookies.Append("basket", basketCoockie);
                }
            }

            return RedirectToAction("index","home");
        }

        [Authorize(Roles ="Member")]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.Users
                .Include(u=>u.Orders).ThenInclude(o=>o.OrderItems).ThenInclude(oi=>oi.Product)
                .FirstOrDefaultAsync(u=>u.UserName == User.Identity.Name);

            if (appUser == null) return NotFound();

            ProfileVM profileVM = new ProfileVM
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                Email = appUser.Email,
                UserName = appUser.UserName
            };

            MemberVM memberVM = new MemberVM
            {
                ProfileVM = profileVM,
                Orders = appUser.Orders
            };

            return View(memberVM);
        }

        [Authorize(Roles = "Member")]
        [HttpPost]
        public async Task<IActionResult> Update(ProfileVM profileVM)
        {
            if (!ModelState.IsValid) return View("Profile",profileVM);

            AppUser dbAppUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (dbAppUser.NormalizedUserName != profileVM.UserName.Trim().ToUpperInvariant() || 
                dbAppUser.Name.ToUpperInvariant() != profileVM.Name.Trim().ToUpperInvariant() || 
                dbAppUser.SurName.ToUpperInvariant() != profileVM.SurName.Trim().ToUpperInvariant() ||
                dbAppUser.NormalizedEmail != profileVM.Email.Trim().ToUpperInvariant())
            {
                dbAppUser.Name = profileVM.Name;
                dbAppUser.SurName = profileVM.SurName;
                dbAppUser.Email = profileVM.Email;
                dbAppUser.UserName = profileVM.UserName;

                IdentityResult identityResult = await _userManager.UpdateAsync(dbAppUser);

                if (!identityResult.Succeeded)
                {
                    foreach (var item in identityResult.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }

                    return View("Profile", profileVM);
                }

                TempData["success"] = "Pr0fil Ugurla Yenilendi";
            }

            if (profileVM.CurrentPassword != null && profileVM.NewPassword != null)
            {
                if (await _userManager.CheckPasswordAsync(dbAppUser,profileVM.CurrentPassword) && profileVM.CurrentPassword == profileVM.NewPassword)
                {
                    ModelState.AddModelError("","New Password Is The Same Current Password");
                    return View("Profile", profileVM);
                }

                IdentityResult identityResult = await _userManager.ChangePasswordAsync(dbAppUser, profileVM.CurrentPassword, profileVM.NewPassword);

                if (!identityResult.Succeeded)
                {
                    foreach (var item in identityResult.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }

                    return View("Profile", profileVM);
                }

                TempData["successPassword"] = "Sifre Ugurla Yenilendi";
            }

            return RedirectToAction("Profile");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("index", "home");
        }

        #region Create Role
        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole { Name = "SuperAdmin" });
        //    await _roleManager.CreateAsync(new IdentityRole { Name = "Member" });
        //    await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });

        //    return Content("Rol Yarandi");
        //}
        #endregion

        #region Create Super Admin
        //public async Task<IActionResult> CreateSuperAdmin()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Name = "Super",
        //        SurName = "Admin",
        //        UserName = "SuperAdmin",
        //        Email = "superadmin@gmail.com"
        //    };

        //    await _userManager.CreateAsync(appUser, "SuperAdmin@123");

        //    await _userManager.AddToRoleAsync(appUser, "SuperAdmin");

        //    return Content("Super Admin Yarandi");
        //}
        #endregion

    }
}
