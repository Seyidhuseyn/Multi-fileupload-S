using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Utilies.Extension;
using WebApplication1.ViewModels;

namespace WebApplication1.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class ProductController : Controller
    {
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env = null)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View(_context.Products.Include(p=>p.ProductColors).ThenInclude(pc=>pc.Color).Include(p=>p.ProductSizes)
                .ThenInclude(ps=>ps.Size).Include(p=>p.ProductImages));
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return BadRequest();
            Product existed = _context.Products.Include(p=>p.ProductImages).FirstOrDefault(p=>p.Id==id);
            if (existed == null) return NotFound();
            foreach (ProductImage image in existed.ProductImages)
            {
                image.ImageUrl.DeleteFile(_env.WebRootPath,"assets/images/product");
                //_context.ProductImages.Remove(image);
            }
            _context.ProductImages.RemoveRange(existed.ProductImages);
            existed.IsDeleted = true;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create()
        {
            ViewBag.Colors = new SelectList(_context.Colors, nameof(Color.Id), nameof(Color.Name));
            ViewBag.Sizes = new SelectList(_context.Sizes,nameof(Size.Id), nameof(Size.Name));
            return View();
        }
        [HttpPost]
        public IActionResult Create(CreateProductVM cp)
        {
            var coverImg = cp.CoverImage;
            var hoverImg = cp.HoverImage;
            var otherImgs = cp.OtherImages;
            if (coverImg?.CheckType("image/") == false)
            {
                ModelState.AddModelError("CoverImage", "Yuklediyiniz file shekil deyil");
            }
            if (coverImg?.CheckSize(300) == true)
            {
                ModelState.AddModelError("CoverImage", "Sekilin olcusu 300kb-dan az olmalidir.");
            }

            if (hoverImg?.CheckType("image/") == false)
            {
                ModelState.AddModelError("HoverImage", "Yuklediyiniz file shekil deyil");
            }
            if (hoverImg?.CheckSize(300) == true)
            {
                ModelState.AddModelError("HoverImage", "Sekilin olcusu 300kb-dan az olmalidir.");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Colors = new SelectList(_context.Colors, nameof(Color.Id), nameof(Color.Name));
                ViewBag.Colors = new SelectList(_context.Sizes, nameof(Size.Id), nameof(Size.Name));
                return View();
            }
            var sizes = _context.Sizes.Where(s => cp.SizeIds.Contains(s.Id));
            var colors = _context.Colors.Where(c => cp.ColorIds.Contains(c.Id));
            Product newProduct = new Product
            {
                Name = cp.Name,
                CostPrice = cp.CostPrice,
                SellPrice = cp.SellPrice,
                Description = cp.Description,
                Discount = cp.Discount,
                IsDeleted = false,
                SKU = "1"
            };
            List<ProductImage> images = new List<ProductImage>();
            images.Add(new ProductImage { ImageUrl = coverImg.SaveFile(Path.Combine(_env.WebRootPath, "assets", "images", "product")), IsCover = true, Product = newProduct });
            images.Add(new ProductImage { ImageUrl = hoverImg.SaveFile(Path.Combine(_env.WebRootPath, "assets", "images", "product")), IsCover = false, Product = newProduct });
            
            //newProduct.ProductImages = new ProductImage[] { new ProductImage { ImageUrl = coverImg.SaveFile(Path.Combine(_env.WebRootPath, "assets", "images", "product")), IsHover = true, Product = newProduct } };
            _context.Products.Add(newProduct);
            foreach (var item in sizes)
            {
                _context.ProductSizes.Add(new ProductSize { Product = newProduct, SizeId = item.Id});
            }
            foreach (var item in colors)
            {
                _context.ProductColors.Add(new ProductColor { Product = newProduct, ColorId = item.Id});
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}