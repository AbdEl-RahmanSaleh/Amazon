using Amazon.API.Errors;
using Amazon.Services.BrandService;
using Amazon.Services.CategoryServices;
using Amazon.Services.ProductService;
using Amazon.Services.ProductService.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Amazon.API.Controllers
{
	[Authorize(Roles = "Seller")]
	public class SellerController : BaseController
	{
		private readonly IProductService _productService;
		private readonly IBrandService _brandService;
		private readonly ICategoryService _categoryService;

		public SellerController(IProductService productService, IBrandService brandService, ICategoryService categoryService)
		{
			_productService = productService;
			_brandService = brandService;
			_categoryService = categoryService;
		}


		[HttpPost]
		public async Task<ActionResult<ProductToReturnDto>> AddProduct(ProductDto product)
		{
			var sellerEmail = User.FindFirstValue("Email");

			var brand = await _brandService.GetBrandByIdAsync(product.BrandId);
			var category = await _categoryService.GetCategoryByIdAsync(product.CategoryId);

			if (brand == null || category == null)
				return BadRequest(new ApiResponse(400));

			var result = await _productService.AddProduct(product, sellerEmail);

			if (result is null)
				return BadRequest(new ApiResponse(400, "Bad Request You Are Not Active Please Wait For Your Account To be Activated"));
			return Ok(result);
		}


		[HttpPut("{id}")]
		public async Task<ActionResult<ProductToReturnDto>> UpdateProduct(int id, ProductDto product)
		{
			var sellerEmail = User.FindFirstValue("Email");


			var Exsistproduct = await _productService.GetProductByIdAsync(id);
			if (Exsistproduct == null)
				return NotFound(new ApiResponse(404));

			if (Exsistproduct.SellerEmail != sellerEmail)
				return BadRequest(new ApiResponse(400, "Access Denied"));

			var brand = _brandService.GetBrandByIdAsync(product.BrandId);
			var category = _categoryService.GetCategoryByIdAsync(product.CategoryId);
			if (brand == null || category == null)
				return BadRequest(new ApiResponse(400));
			return Ok(await _productService.UpdateProduct(id, product));
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteProduct(int id)
		{
			var sellerEmail = User.FindFirstValue("Email");

			var product = await _productService.GetProductByIdAsync(id);
			if (product is null)
				return NotFound(new ApiResponse(404));

			if (product.SellerEmail != sellerEmail)
				return BadRequest(new ApiResponse(400, "Access Denied"));

			var result = await _productService.DeleteProduct(id, sellerEmail);
			if (result is false)
				return BadRequest(new ApiResponse(400, "Can't Delete This Product"));

			return Ok(new { message = "Product Deleted Successfully" });

		}

		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetAllSellerProducts()
			=> Ok(await _productService.GetAllSellerProductsAsync(User.FindFirstValue("Email")));


		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetSellerAcceptedProducts()
			=> Ok(await _productService.GetAllSellerAccepedProductsAsync(User.FindFirstValue("Email")));


		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetSellerPendingProducts()
			=> Ok(await _productService.GetAllSellerPendingProductsAsync(User.FindFirstValue("Email")));


		[HttpGet("{id}")]
		public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetSellerProductById(int id)
		{
			var sellerEmail = User.FindFirstValue("Email");

			var result = await _productService.GetSellerProductByIdAsync(sellerEmail, id);
			if (result is null)
				return BadRequest(new ApiResponse(404));

			return Ok(result);
		}

		[HttpGet]
		public async Task<ActionResult> GetAllSellerEarnings()
		{
			var sellerEmail = User.FindFirstValue("Email");

			var totalEarnings = await _productService.GetAllSellerEarnings(sellerEmail);

			return Ok(new
			{
				TotalEarnings = totalEarnings
			});
		}
		
		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<ItemEarningsDto>>> GetAllSellerEarningsWithDetails()
		{
			var sellerEmail = User.FindFirstValue("Email");

			var totalEarnings = await _productService.GetAllSellerEarningsWithDetails(sellerEmail);

			return Ok(totalEarnings);
		}
		
		[HttpGet("{id}")]
		public async Task<ActionResult<ItemEarningsDto>> GetSellerEarningsByProductId(int id)
		{
			var sellerEmail = User.FindFirstValue("Email");

			var product = await _productService.GetSellerProductByIdAsync(sellerEmail, id);
			if (product is null)
				return NotFound(new ApiResponse(404));

			var productEarnings = await _productService.GetSellerEarningsByProductId(id,sellerEmail);

			if (productEarnings == null)
				return Ok(new ApiResponse (200,"No Quantity Sold"));

			return Ok(productEarnings);
		}

	}
}
