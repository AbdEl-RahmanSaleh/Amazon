using AdminWebApplication.Models;
using Amazon.Core.DBContext;
using Amazon.Core.Entities.OrderAggregate;
using Amazon.Services.BrandService;
using Amazon.Services.ProductService;
using Amazon.Services.ProductService.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdminWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService productService;
        private readonly IBrandService brandService;
        private readonly AmazonDbContext amazonDbContext;

        public HomeController(IProductService productService, IBrandService brandService, AmazonDbContext amazonDbContext)
        {
            this.productService = productService;
            this.brandService = brandService;
            this.amazonDbContext = amazonDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var products = await productService.GetAllProductsAsync();
            var brands = await brandService.GetAllBrandsAsync();
            var lowQuantityProducts = products.Where(p => p.QuantityInStock < 10).ToList();

            var orderItems = amazonDbContext.OrderItems.GroupBy(o => o.Product.ProductId)
                .Select(g => new 
                {
                    Id = g.Key, 
                    Name = g.First().Product.ProductName, 
                    TotalQuantity = g.Sum(s => s.Quantity)
                }).OrderByDescending(o => o.TotalQuantity).ToList();


            //var test = (from OrderItem in amazonDbContext.OrderItems
            //            select OrderItem.Id, OrderItem.Product.ProductName, OrderIt.Quantity)

            //var test2 = amazonDbContext.OrderItems.Select(o => new { o.Id, o.Product.ProductName, o.Quantity})
            //    .GroupBy(o => o.Id).Aggregate((x, y) => x + y);

                       ;
            ViewBag.TopOrders = orderItems;

            DashboardData data = new()
            {
                ProductsNo = products.Count,
                BrandsNo = brands.Count,
                Products = lowQuantityProducts
            };
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


    public class DashboardData
    {
        public int ProductsNo { get; set; }
        public int UsersNo { get; set; }
        public int BrandsNo { get; set; }
        public List<ProductToReturnDto> Products { get; set; }
        public List<object> TopOrders { get; set; }
    }

    public class TopOrders
    {

    }
}
