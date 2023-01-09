using Microsoft.AspNetCore.Mvc;
using System;
using WebApplication1.DAL;
using WebApplication1.ViewModels.Home;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            HomeVM home = new HomeVM { Sliders= _context.Sliders.OrderBy(s=>s.Order), Sponsors = _context.Sponsors };
            return View(home);
        }
    }
}
