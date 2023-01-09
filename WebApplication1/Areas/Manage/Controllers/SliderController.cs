using Microsoft.AspNetCore.Mvc;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class SliderController : Controller
    {
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        public SliderController(AppDbContext context, IWebHostEnvironment env = null)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View(_context.Sliders.ToList());
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(CreateSliderVM  sliderVM)
        {
            if (!ModelState.IsValid) return View();
            IFormFile file = sliderVM.Image;
            if (!file.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Image", "Yuklediyiniz fal shekil deyil");
                return View();
            }
            if (file.Length > 200 * 1024)
            {
                ModelState.AddModelError("Image" , "Shekilin olcusu 200 kb-dan artiq ola bilmez.");
                return View();
            }
            //string fileName = Guid.NewGuid().ToString() + (file.FileName.Length>64 ? file.FileName.Substring(0,64) : file.FileName);
            string fileName = Guid.NewGuid().ToString() + file.FileName;
            using (var stream = new FileStream(Path.Combine(_env.WebRootPath, "assets", "images", "slider", fileName), FileMode.Create))
            {
                file.CopyTo(stream);
            }
            Slider slider = new Slider { Description = sliderVM.Description, Order = sliderVM.Order, PrimaryTitle = sliderVM.PrimaryTitle, SecondaryTitle = sliderVM.SecondaryTitle, ImageUrl=fileName };
            if (_context.Sliders.Any(s => s.Order == slider.Order))
            {
                ModelState.AddModelError("Order", $"{slider.Order} sirasinda artiq slider movcuddur.");
                return View();
            }
            _context.Sliders.Add(slider);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Update(int? id)
        {
            if (id == null || id == 0) return BadRequest();
            Slider slider = _context.Sliders.Find(id);
            if (slider is null) return NotFound();
            return View(slider);
        }
        [HttpPost]
        public IActionResult Update(int? id, Slider slider)
        {
            if (id == null || id == 0 || id!=slider.Id || slider is null) return BadRequest();
            if (!ModelState.IsValid) return View();
            Slider anotherSlider = _context.Sliders.FirstOrDefault(s=>s.Order==slider.Order);
            if (anotherSlider != null)
            {
                anotherSlider.Order = _context.Sliders.Find(id).Order;
            }
            Slider exist = _context.Sliders.Find(slider.Id);
            exist.Order = slider.Order;
            exist.Description = slider.Description;
            exist.PrimaryTitle= slider.PrimaryTitle;
            exist.SecondaryTitle= slider.SecondaryTitle;
            exist.ImageUrl= slider.ImageUrl;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int? id)
        {
            if (id is null) return BadRequest();

            Slider slider = _context.Sliders.Find(id);
            if (slider is null) return NotFound();
            _context.Sliders.Remove(slider);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
