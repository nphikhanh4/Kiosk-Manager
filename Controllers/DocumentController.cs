using Microsoft.AspNetCore.Mvc;
using KioskManagementWebApp.Data;
using KioskManagementWebApp.Models;

namespace KioskManagementWebApp.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var documents = _context.Documents.ToList();
            return View(documents);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Document document)
        {
            if (ModelState.IsValid)
            {
                _context.Add(document);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }
    }
}