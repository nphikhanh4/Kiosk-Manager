using Microsoft.AspNetCore.Mvc;
using KioskManagementWebApp.Data;
using KioskManagementWebApp.Models;

namespace KioskManagementWebApp.Controllers
{
    public class TuitionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TuitionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tuitions = _context.Tuitions.ToList();
            return View(tuitions);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Tuition tuition)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tuition);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(tuition);
        }
    }
}