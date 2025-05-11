using Microsoft.AspNetCore.Mvc;
using KioskManagementWebApp.Data;
using KioskManagementWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KioskManagementWebApp.Controllers
{
    public class TuitionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TuitionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tuition/Index
        public async Task<IActionResult> Index()
        {
            var tuitionFees = await _context.TuitionFees
                .Include(tf => tf.Student)
                .Include(tf => tf.TuitionFeeDetails)
                .Where(tf => !tf.IsPaid)
                .ToListAsync();

            return View(tuitionFees);
        }

        // POST: Tuition/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var tuitionFee = await _context.TuitionFees.FindAsync(id);
            if (tuitionFee == null)
            {
                return NotFound();
            }

            tuitionFee.IsPaid = true;
            tuitionFee.PaymentDate = DateTime.Now;
            _context.Update(tuitionFee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}