using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P129Allup.DAL;
using P129Allup.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using P129Allup.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using P129Allup.Helpers;

namespace P129Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ProductController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted && !b.IsMain).ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted && !b.IsMain).ToListAsync();

            if (!ModelState.IsValid) return View();

            if (!await _context.Brands.AnyAsync(b=>!b.IsDeleted && b.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Select Correct Brand");
                return View();
            }

            if(product.CategoryId == null)
            {
                ModelState.AddModelError("CategoryId", "You Must Select Correct");
                return View();
            }

            if (!await _context.Categories.AnyAsync(c=>!c.IsDeleted && !c.IsMain && c.Id == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Select Correct Category");
                return View();
            }

            if (product.Files != null && product.Files.Count() > 5)
            {
                ModelState.AddModelError("Files", "Can Select Maximum 5 Image");
                return View();
            }

            if (product.MainFile != null)
            {
                if (!product.MainFile.CheckContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFile", "Main Image Must Be .jpg");
                    return View();
                }

                if (product.MainFile.CheckFileLength(50))
                {
                    ModelState.AddModelError("MainFile", "Main Image Length Must Be 50kb");
                    return View();
                }

                product.MainImage = await product.MainFile.CreateAsync(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("MainFile", "Main Image Is Requered");
                return View();
            }

            if (product.HoveFile != null)
            {
                if (!product.HoveFile.CheckContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFHoveFileile", "Second Image Must Be .jpg");
                    return View();
                }

                if (product.HoveFile.CheckFileLength(50))
                {
                    ModelState.AddModelError("HoveFile", "Second Image Length Must Be 50kb");
                    return View();
                }

                product.HoverImage = await product.HoveFile.CreateAsync(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("HoveFile", "Second Image Is Requered");
                return View();
            }

            if (product.Files != null && product.Files.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile file in product.Files)
                {
                    if (!file.CheckContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("Files", "Files Image Must Be .jpg");
                        return View();
                    }

                    if (file.CheckFileLength(50))
                    {
                        ModelState.AddModelError("Files", "Files Image Length Must Be 50kb");
                        return View();
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = await file.CreateAsync(_env, "assets", "images", "product-quick")
                    };

                    productImages.Add(productImage);
                }

                product.ProductImages = productImages;
            }

            string seria = (_context.Brands.FirstOrDefault(b => b.Id == product.BrandId).Name.Substring(0, 2) + product.Name.Trim().Substring(0, 2)).ToLower();

            int code = _context.Products.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Seria == seria) != null ? _context.Products.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Seria == seria).Code += 1 : 1;

            product.Code = code;
            product.Seria = seria;
            product.Name = product.Name.Trim();

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted && !b.IsMain).ToListAsync();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted && !b.IsMain).ToListAsync();

            if (!ModelState.IsValid) return View();

            if (id == null) return BadRequest();

            Product dbProduct = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);

            if (dbProduct == null) return NotFound();

            if (!await _context.Brands.AnyAsync(b => !b.IsDeleted && b.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Select Correct Brand");
                return View();
            }

            if (product.CategoryId == null)
            {
                ModelState.AddModelError("CategoryId", "You Must Select Correct");
                return View();
            }

            if (!await _context.Categories.AnyAsync(c => !c.IsDeleted && !c.IsMain && c.Id == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Select Correct Category");
                return View();
            }

            int canSelecteCount = 5 - dbProduct.ProductImages.Count();

            if (product.Files != null &&  canSelecteCount < product.Files.Count())
            {
                ModelState.AddModelError("Files", $"Can Select {canSelecteCount} items");
                return View();
            }

            if (product.MainFile != null)
            {
                if (!product.MainFile.CheckContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFile", "Main Image Must Be .jpg");
                    return View();
                }

                if (product.MainFile.CheckFileLength(50))
                {
                    ModelState.AddModelError("MainFile", "Main Image Length Must Be 50kb");
                    return View();
                }

                FileHelper.DeleteFile(_env, dbProduct.MainImage, "assets", "images", "product");

                dbProduct.MainImage = await product.MainFile.CreateAsync(_env, "assets", "images", "product");
            }

            if (product.HoveFile != null)
            {
                if (!product.HoveFile.CheckContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFHoveFileile", "Second Image Must Be .jpg");
                    return View();
                }

                if (product.HoveFile.CheckFileLength(50))
                {
                    ModelState.AddModelError("HoveFile", "Second Image Length Must Be 50kb");
                    return View();
                }

                FileHelper.DeleteFile(_env, dbProduct.HoverImage, "assets", "images", "product");

                dbProduct.HoverImage = await product.HoveFile.CreateAsync(_env, "assets", "images", "product");
            }

            if (product.Files != null && product.Files.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile file in product.Files)
                {
                    if (!file.CheckContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("Files", "Files Image Must Be .jpg");
                        return View();
                    }

                    if (file.CheckFileLength(50))
                    {
                        ModelState.AddModelError("Files", "Files Image Length Must Be 50kb");
                        return View();
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = await file.CreateAsync(_env, "assets", "images", "product-quick")
                    };

                    productImages.Add(productImage);
                }

                dbProduct.ProductImages.AddRange(productImages);
            }

            product.Name = product.Name.Trim();
            
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null) return BadRequest();

            ProductImage productImage = await _context.ProductImages.FirstOrDefaultAsync(p => p.Id == id);

            if (productImage == null) return NotFound();

            Product product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == productImage.ProductId);

            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();

            FileHelper.DeleteFile(_env, productImage.Image, "assets", "images", "product-quick");

            return PartialView("_ProductImagePartial",product.ProductImages);
        }

        //public async Task<IActionResult> Create()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Create(Student student)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    return Content($"{student.Name} {student.SurName} {student.Age} {student.Email}");
        //}
    }

    //public class Student
    //{
    //    [Required(ErrorMessage ="Mecburidi qaqa")]
    //    [StringLength(maximumLength:255,ErrorMessage = "Maksimum 255 Simvol Ola Biler")]
    //    public string Name { get; set; }
    //    public string SurName { get; set; }
    //    [Display(Name ="Yas")]
    //    public int Age { get; set; }
    //    [EmailAddress]
    //    public string Email { get; set; }
    //}
}
