using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_com.Data;
using E_com.Models;

namespace E_com.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons.OrderByDescending(c => c.ExpiryDate).ToListAsync();
            return View(coupons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                // Ensure code is stored in uppercase
                coupon.CouponCode = coupon.CouponCode.ToUpper();

                // Check for duplicate code
                var exists = await _context.Coupons.AnyAsync(c => c.CouponCode == coupon.CouponCode);
                if (exists)
                {
                    ModelState.AddModelError("CouponCode", "A coupon with this code already exists.");
                    return View(coupon);
                }

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coupon created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                coupon.CouponCode = coupon.CouponCode.ToUpper();

                // Check for duplicate code (ignoring current coupon)
                var exists = await _context.Coupons.AnyAsync(c => c.CouponCode == coupon.CouponCode && c.Id != id);
                if (exists)
                {
                    ModelState.AddModelError("CouponCode", "A coupon with this code already exists.");
                    return View(coupon);
                }

                _context.Coupons.Update(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coupon updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coupon deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
