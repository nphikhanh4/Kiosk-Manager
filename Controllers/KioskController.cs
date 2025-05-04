using Microsoft.AspNetCore.Mvc;
using KioskManagementWebApp.Data;
using KioskManagementWebApp.Models;

namespace KioskManagementWebApp.Controllers
{
    public class KioskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KioskController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
                var kiosks = _context.Kiosks.ToList();
            return View(kiosks);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Kiosk kiosk)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kiosk);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(kiosk);
        }
    }
}