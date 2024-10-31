using Amazon.Core.DBContext;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Identity;
using Amazon.Core.Entities.OrderAggregate;
using Amazon.Services.ProductService.Dto;
using Amazon.Services.Utilities;
using Amazone.Infrastructure.Interfaces;
using Amazone.Infrastructure.Specification.ProductSpecifications;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Amazon.Services.ProductService
{
	public class ProductService : IProductService
	{

		private readonly IProductRepository _productRepo;
		private readonly UserManager<AppUser> _userManager;
		private readonly IGenericRepository<Order> _orderRepo;
		private readonly IMapper _mapper;
		

		public ProductService(IMapper mapper,
						IProductRepository productRepo,
						UserManager<AppUser> userManager,
						IGenericRepository<Order> OrderRepo)
		{
			_mapper = mapper;
			_productRepo = productRepo;
			_userManager = userManager;
			_orderRepo = OrderRepo;
		}


		public async Task<ProductToReturnDto> AddProduct(ProductDto productDto,string sellerEmail)
		{

			var seller = await _userManager.FindByEmailAsync(sellerEmail);
					
			
			if (productDto.Discount != null)
			{
				if (productDto.Discount.StartDate.Value.Date == DateTime.Now.Date)
				{
					productDto.Discount.DiscountStarted = true;
				}
				productDto.Discount.PriceAfterDiscount = productDto.Price * (1 - productDto.Discount.DiscountPercentage);
			}
			var product = _mapper.Map<Product>(productDto);
			product.PictureUrl = await DocumentSettings.UploadFile(productDto.ImageFile, "productImages");

			product.QuantitySold = 0 ;
			product.SellerEmail = seller.Email;
			product.SellerName = seller.SellerName;
            if (product.SellerEmail == "Admin@gmail.com")
                product.IsAccepted = true;
            else
                product.IsAccepted = false;
			
				
			await HandleProductImages(productDto.ImagesFiles, product);


			await _productRepo.Add(product);

			//var productToReturn = _mapper.Map<ProductToReturnDto>(product);

			return await GetProductByIdAsync(product.Id);
		
		}
		public async Task<ProductToReturnDto> UpdateProduct(int id, ProductDto productDto)
		{
			Product existintProduct = await _productRepo.GetByIdAsync(id);
			if (existintProduct == null)
				return null;

			//if (productDto.ImageFile != null)
			//{
			//	DocumentSettings.DeleteFile("productImages", existintProduct.PictureUrl);
			//	existintProduct.PictureUrl = await DocumentSettings.UploadFile(productDto.ImageFile, "productImages");
			//}

			if (productDto.ImagesFiles != null && productDto.ImagesFiles.Count > 0)
			{
				foreach (var image in existintProduct.Images.ToList())
				{
					DocumentSettings.DeleteFile("productImages", image.ImagePath);
					//existintProduct.Images.Remove(image);
				}

				await HandleProductImages(productDto.ImagesFiles, existintProduct);
			}
			if (productDto.Discount != null)
			{
				if (productDto.Discount.StartDate.Value.Date == DateTime.Now.Date)
				{
					productDto.Discount.DiscountStarted = true;
				}
				productDto.Discount.PriceAfterDiscount = productDto.Price * (1 - productDto.Discount.DiscountPercentage);
			}
			if (existintProduct.SellerEmail == "Admin@gmail.com")
				existintProduct.IsAccepted = true;
			else
				existintProduct.IsAccepted = false;
			_mapper.Map(productDto, existintProduct);

			await _productRepo.Update(existintProduct);


			return await GetProductByIdAsync(existintProduct.Id);
		}
		public async Task<bool> DeleteProduct(int id,string sellerEmail)
		{
			var product = await _productRepo.GetByIdAsync(id);
			if (product is null)
				return false;


			var orders = await _orderRepo.GetAllAsync();
			var productOrders = orders.Where(o => o.Items.Any(i => i.Product.ProductId == product.Id));
			
			if (productOrders != null || productOrders.Any())
			{
				var pendingOrders = productOrders.Where(o => o.OrderStatus == OrderStatus.Pending).ToList();
				
				if (pendingOrders.Count > 0)
					return false;
			}

			//DocumentSettings.DeleteFile("productImages", product.PictureUrl);
			foreach (var item in product.Images)
			{
				DocumentSettings.DeleteFile("productImages", item.ImagePath);
			}

			await _productRepo.Delete(product);


			var products = await GetAllSellerProductsAsync(sellerEmail);
			
			return true;
		}


		public async Task<IReadOnlyList<ProductToReturnDto>> GetAllProductsAsync()
		{
			var products = await _productRepo.GetAllAsync();
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		// Helper method to add other product images
		private async Task HandleProductImages(ICollection<IFormFile> imagesFiles, Product product)
		{
			if (imagesFiles != null && imagesFiles.Count() > 0)
			{
				foreach (var image in imagesFiles)
				{
					var productImage = new ProductImages
					{
						ProductId = product.Id,
						ImagePath = await DocumentSettings.UploadFile(image, "productImages")
					};
					product.Images.Add(productImage);
				}
			}
		}


		#region Search product
		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByBrandIdAsync(int id)
		{
			var products = await _productRepo.SearchByBrandAsync(id);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByBrandNameAsync(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return await GetAllProductsAsync();
			}
			var products = await _productRepo.SearchByBrandNameAsync(name);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByCategoryIdAndNameAsync(string name, int? id)
		{

			if (id == 0 || id == null)
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					return await GetAllProductsAsync();
				}
				return await GetProductsByNameAsync(name);
			}


			if (id.HasValue && id != 0)
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					return await GetProductsByCategoryIdAsync(id.Value);
				}
			}
			var products = await _productRepo.SearchByCategoryAndProductNameAsync(name, id);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);
			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByCategoryIdAsync(int id)
		{

			var products = await _productRepo.SearchByCategoryAsync(id);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByCategoryNameAsync(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return await GetAllProductsAsync();
			}
			var products = await _productRepo.SearchByCategoryNameAsync(name);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByNameAsync(string name)
		{

			if (string.IsNullOrWhiteSpace(name))
			{
				return await GetAllProductsAsync();
			}
			var products = await _productRepo.SearchByProductNameAsync(name);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByParentCategoryIdAsync(int id)
		{
			var products = await _productRepo.SearchByParentCategoryAsync(id);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetProductsByParentCategoryNameAsync(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return await GetAllProductsAsync();
			}
			var products = await _productRepo.SearchByParentCategoryNameAsync(name);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> SearchByStringAsync(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
			{
				return await GetAllProductsAsync();
			}
			var products = await _productRepo.SearchByStringAsync(str);
			var mappedProdcts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return mappedProdcts;
		}

		#endregion

		public async Task<Pagination<ProductToReturnDto>> GetAllProductsAsync(ProductSpecParams specParams)
		{
			var spec = new ProductWithBrandAndCategorySpecifications(specParams);

			var products = await _productRepo.GetAllWithSpecAsync(spec);

			var verifiedSeller = _userManager.Users.Where(u => u.IsActiveSeller == true).ToList();

			var VerifiedSelllersEmails = verifiedSeller.Select(s => s.Email).ToList();

			products = products.Where(p => p.IsAccepted == true && VerifiedSelllersEmails.Contains(p.SellerEmail)).ToList();

			if (products.Count <= 0)
				return null;

			var count = await GetCountAsync(specParams);
			var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);	

			return new Pagination<ProductToReturnDto>(specParams.PageIndex,specParams.PageSize,count,data);
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetAllSellerProductsAsync(string sellerEmail)
		{

			var spec = new ProductWithBrandAndCategorySpecifications(sellerEmail);

			var products = await _productRepo.GetAllWithSpecAsync(spec);

			if (products.Count <= 0)
				return null;
			var sellerProducts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return sellerProducts;
		}
		public async Task<ProductToReturnDto> GetSellerProductByIdAsync(string sellerEmail,int productId)
		{

			var spec = new ProductWithBrandAndCategorySpecifications(sellerEmail,productId);

			var product = await _productRepo.GetWithSpecAsync(spec);
			
			if (product == null)
				return null;

			var sellerProducts = _mapper.Map<ProductToReturnDto>(product);

			return sellerProducts;
		}

		public async Task<ProductToReturnDto> GetProductByIdAsync(int id)
		{
			var spec = new ProductWithBrandAndCategorySpecifications(id);

			var product = await _productRepo.GetWithSpecAsync(spec);

			if (product == null)
				return null;


			var mappedProduct = _mapper.Map<ProductToReturnDto>(product);
			

			return mappedProduct;
		}
		private async Task<int> GetCountAsync(ProductSpecParams specParams)
		{
			var counSpec = new ProductWithFilterationForCountSpecification(specParams);

			var count = await _productRepo.GetCountAsync(counSpec);

			return count;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetAllSellerAccepedProductsAsync(string sellerEmail)
		{
			var spec = new ProductWithBrandAndCategorySpecifications(sellerEmail);

			var products = await _productRepo.GetAllWithSpecAsync(spec);

			if (products.Count <= 0)
				return null;
			products = products.Where(p => p.IsAccepted == true).ToList();

			var sellerProducts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return sellerProducts;
		}

		public async Task<IReadOnlyList<ProductToReturnDto>> GetAllSellerPendingProductsAsync(string sellerEmail)
		{
			var spec = new ProductWithBrandAndCategorySpecifications(sellerEmail);

			var products = await _productRepo.GetAllWithSpecAsync(spec);

			if (products.Count <= 0)
				return null;

			products = products.Where(p => p.IsAccepted == false).ToList();

			var sellerProducts = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

			return sellerProducts;
		}

		public async Task<decimal> GetAllSellerEarnings(string sellerEmail)
		{
			var seller = await _userManager.FindByEmailAsync(sellerEmail);

			decimal totalEarnings = 0;

			var orders = await _orderRepo.GetAllAsync();
			orders = orders.Where(o => o.OrderStatus != OrderStatus.Cancelled && o.PaymentStatus== PaymentStatus.PaymentRecieved).ToList();
			foreach (var order in orders)
			{
				foreach (var item in order.Items)
				{
					if (item.Product.SellerEmail == seller.Email)	
					{
						totalEarnings += item.Price * item.Quantity;
					}
				}
			}
			return totalEarnings;
		}

		public async Task<IReadOnlyList<ItemEarningsDto>> GetAllSellerEarningsWithDetails(string sellerEmail)
			=> await CalculateSellerEarnings(sellerEmail);
		

		public async Task<ItemEarningsDto> GetSellerEarningsByProductId(int id ,string sellerEmail)
			=> (await CalculateSellerEarnings(sellerEmail)).Where( i =>i.ProductId == id ).FirstOrDefault();

		

		private async Task<IReadOnlyList<ItemEarningsDto>> CalculateSellerEarnings(string sellerEmail)
		{
			var orders = await _orderRepo.GetAllAsync();
			orders = orders.Where(o => o.OrderStatus != OrderStatus.Cancelled && o.PaymentStatus == PaymentStatus.PaymentRecieved).ToList();

			if (orders == null || !orders.Any())
				return null;

			var productEarnings = orders.Where(o => o.OrderStatus != OrderStatus.Cancelled && o.PaymentStatus == PaymentStatus.PaymentRecieved)
						.SelectMany(order => order.Items)
						.Where(item => item.Product.SellerEmail == sellerEmail)
						.GroupBy(item => new { item.Product.ProductId, item.Product.ProductName })
						.Select(group => new ItemEarningsDto
						{
							ProductId = group.Key.ProductId,
							ProductName = group.Key.ProductName,
							Earnings = group.Sum(item => item.Price * item.Quantity),
							QuantitySold = group.Sum(item => item.Quantity)
						})
						.ToList();


			return productEarnings;
		}
	}
}
