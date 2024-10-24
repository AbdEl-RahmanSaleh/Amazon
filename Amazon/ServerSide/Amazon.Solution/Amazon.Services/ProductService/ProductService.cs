using Amazon.Core.Entities;
using Amazon.Core.Entities.Identity;
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
		private readonly IMapper _mapper;

		public ProductService(IMapper mapper, IProductRepository productRepo,UserManager<AppUser> userManager)
		{
			_mapper = mapper;
			_productRepo = productRepo;
			_userManager = userManager;
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
			product.SellerEmail = seller.Email;
			product.SellerName = seller.SellerName;
				
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

			if (productDto.ImageFile != null)
			{
				DocumentSettings.DeleteFile("productImages", existintProduct.PictureUrl);
				existintProduct.PictureUrl = await DocumentSettings.UploadFile(productDto.ImageFile, "productImages");
			}

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
			_mapper.Map(productDto, existintProduct);

			await _productRepo.Update(existintProduct);


			return await GetProductByIdAsync(existintProduct.Id);
		}
		public async Task<IReadOnlyList<ProductToReturnDto>> DeleteProduct(int id)
		{
			var product = await _productRepo.GetByIdAsync(id);
			if (product is null)
				return null;


			DocumentSettings.DeleteFile("productImages", product.PictureUrl);
			foreach (var item in product.Images)
			{
				DocumentSettings.DeleteFile("productImages", item.ImagePath);
			}

			await _productRepo.Delete(product);


			var products = await GetAllProductsAsync();
			return products;
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

	}
}
