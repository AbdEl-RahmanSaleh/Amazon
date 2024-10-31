using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Amazon.Core.DBContext;
using Amazon.Core.Entities;
using Amazon.Services.ProductService;
using Amazon.Services.ProductService.Dto;
using Amazon.Services.BrandService;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AdminWebApplication.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AmazonDbContext _context;
        private readonly IProductService productService;
        private readonly IBrandService brandService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(AmazonDbContext context, IProductService productService, 
            IBrandService brandService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.productService = productService;
            this.brandService = brandService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await productService.GetAllProductsAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await productService.GetProductByIdAsync(id);
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            var brands = _context.Brands.ToList();
            var categories = _context.Categories.ToList();
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }


        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                //await productService.AddProduct(productDto,"Admin@gmail.com");
                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    QuantityInStock = productDto.QuantityInStock,
                    BrandId = productDto.BrandId,
                    CategoryId = productDto.CategoryId,
                    SellerEmail = "Admin@gmail.com",
                    SellerName = "Amazon",
                    IsAccepted = true
                };

                // Define the main image folder path
                var apiProjectDirectory = GetApiProjectDirectory();

                string mainImageFolder = Path.Combine(apiProjectDirectory, "wwwroot", "Files", "productImages");
                if (!Directory.Exists(mainImageFolder))
                {
                    Directory.CreateDirectory(mainImageFolder);
                }

                if (productDto.ImageFile != null)
                {
                    var mainImageFileName = $"{Guid.NewGuid()}-{Path.GetFileName(productDto.ImageFile.FileName)}";
                    var mainImageFilePath = Path.Combine(mainImageFolder, mainImageFileName);
                    using var mainImageFileStream = new FileStream(mainImageFilePath, FileMode.Create);
                    await productDto.ImageFile.CopyToAsync(mainImageFileStream);
                    product.PictureUrl = mainImageFileName;
                }

                if (productDto.ImagesFiles != null && productDto.ImagesFiles.Count > 0)
                {
                    foreach (var image in productDto.ImagesFiles)
                    {
                        var additionalImageFileName = $"{Guid.NewGuid()}-{Path.GetFileName(image.FileName)}";
                        var additionalImageFilePath = Path.Combine(mainImageFolder, additionalImageFileName);
                        using var additionalImageFileStream = new FileStream(additionalImageFilePath, FileMode.Create);
                        await image.CopyToAsync(additionalImageFileStream);
                        product.Images.Add(new ProductImages
                        {
                            ImagePath = additionalImageFileName
                        });
                    }
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }
            var brands = _context.Brands.ToList();
            var categories = _context.Categories.ToList();
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(productDto);
        }

        private string GetApiProjectDirectory()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            while (currentDirectory != null && !currentDirectory.EndsWith("Amazon.Solution"))
            {
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }
            if (currentDirectory != null)
            {
                return Path.Combine(currentDirectory, "Amazon.API");
            }
            throw new InvalidOperationException("Could not find Amazon.Solution directory");
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Discount) // Include discount
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            var brands = await brandService.GetAllBrandsAsync();
            ViewData["BrandId"] = new SelectList(brands, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,[FromForm]ProductDto productDto)
        {
            //if (id != product.Id)
            //{
            //    return NotFound();
            //}

            if (productDto.Discount.DiscountPercentage is null)
                productDto.Discount = null;
            else
                productDto.Discount.DiscountPercentage /= 100;

            var currentProduct = _context.Products.Find(id);
            if (currentProduct == null)
                return NotFound();

            // Define the main image folder path
            var apiProjectDirectory = GetApiProjectDirectory();

            string mainImageFolder = Path.Combine(apiProjectDirectory, "wwwroot", "Files", "productImages");
            if (!Directory.Exists(mainImageFolder))
            {
                Directory.CreateDirectory(mainImageFolder);
            }

            if (productDto.ImageFile != null)
            {   
                var mainImageFileName = $"{Guid.NewGuid()}-{Path.GetFileName(productDto.ImageFile.FileName)}";
                var mainImageFilePath = Path.Combine(mainImageFolder, mainImageFileName);
                using var mainImageFileStream = new FileStream(mainImageFilePath, FileMode.Create);
                await productDto.ImageFile.CopyToAsync(mainImageFileStream);
                currentProduct.PictureUrl = mainImageFileName;
            }

            if (productDto.ImagesFiles != null && productDto.ImagesFiles.Count > 0)
            {
                foreach (var item in currentProduct.Images.ToList())
                {
                    _context.Remove(item);
                    _context.SaveChanges();
                }
                foreach (var image in currentProduct.Images)
                {
                    var additionalImagePath = Path.Combine(mainImageFolder, image.ImagePath);
                    if (System.IO.File.Exists(additionalImagePath))
                    {
                        System.IO.File.Delete(additionalImagePath);
                    }
                }
                foreach (var image in productDto.ImagesFiles)
                {
                    var additionalImageFileName = $"{Guid.NewGuid()}-{Path.GetFileName(image.FileName)}";
                    var additionalImageFilePath = Path.Combine(mainImageFolder, additionalImageFileName);
                    using var additionalImageFileStream = new FileStream(additionalImageFilePath, FileMode.Create);
                    await image.CopyToAsync(additionalImageFileStream);
                    currentProduct.Images.Add(new ProductImages
                    {
                        ImagePath = additionalImageFileName
                    });
                }
            }

            if (ModelState.IsValid)
            {
                productDto.ImageFile = null;
                productDto.ImagesFiles = null;
                var result = await productService.UpdateProduct(id, productDto);
                if (result is null)
                    return NotFound();
              
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "name", productDto.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "name", productDto.CategoryId);
            return View(productDto);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Delete the product's images
            var apiProjectDirectory = GetApiProjectDirectory();
            string mainImageFolder = Path.Combine(apiProjectDirectory, "wwwroot", "Files", "productImages");

            // Delete the main product image
            //var mainImagePath = Path.Combine(mainImageFolder, product.PictureUrl);
            //if (System.IO.File.Exists(mainImagePath))
            //{
            //    System.IO.File.Delete(mainImagePath);
            //}

            // Delete the additional product images
            foreach (var image in product.Images)
            {
                var additionalImagePath = Path.Combine(mainImageFolder, image.ImagePath);
                if (System.IO.File.Exists(additionalImagePath))
                {
                    System.IO.File.Delete(additionalImagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
