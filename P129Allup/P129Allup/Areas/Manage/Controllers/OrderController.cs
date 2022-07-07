using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P129Allup.DAL;
using P129Allup.Enums;
using P129Allup.Models;
using P129Allup.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P129Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles ="SuperAdmin,Admin")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? status, int page = 1)
        {
            IQueryable<Order> query = _context.Orders.Include(o=>o.OrderItems).ThenInclude(oi=>oi.Product);

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
            return View(PageNatedLisd<Order>.Create(query, page, itemCount));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Order order = await _context.Orders
                .Include(o=>o.OrderItems).ThenInclude(oi=>oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        //Order Rejectdise Accept Olmasin
        [HttpPost]
        public async Task<IActionResult> Update(int? id, OrderStatus orderstatus, string Comment)
        {
            if (id == null) return BadRequest();

            Order order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            order.OrderStatus = orderstatus;
            order.Comment = Comment;

            await _context.SaveChangesAsync();

            return RedirectToAction("index");
        }
    }
}
