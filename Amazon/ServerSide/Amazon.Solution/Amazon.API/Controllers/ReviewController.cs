using Amazon.Services.ReviewServices.Dto;
using Amazon.Services.ReviewServices;
using Microsoft.AspNetCore.Mvc;
using Amazon.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Amazon.Core.Entities.Identity;
using Amazon.Core.Entities;

public class ReviewController : BaseController
{
	private readonly IReviewService _reviewService;
	private readonly UserManager<AppUser> _userManager;

	public ReviewController(IReviewService reviewService,UserManager<AppUser> userManager)
	{
		_reviewService = reviewService;
		_userManager = userManager;
	}

	[HttpGet]
	public async Task<IReadOnlyList<ReviewToReturnDto>> GetAllReviews()
	{
		var reviews = await _reviewService.GetAllReviewsAsync();
		return reviews;
	}


	[HttpGet("{productId}")]
	public async Task<IReadOnlyList<ReviewToReturnDto>> GetAllProductReviewsById(int productId)
	{
		var reviews = await _reviewService.GetAllReviewsByProductIdAsync(productId);
		if (reviews == null)
		{
			return null;
		}
		return reviews;
	}

	[HttpGet("{id}")]
	public async Task<ReviewToReturnDto> GetReviewById(int id)
	{

		var review = await _reviewService.GetReviewByIdAsync(id);
		if (review == null)
		{
			return null;
		}
		return review;
	}
	
	[HttpPost]
	public async Task<ReviewToReturnDto> AddReview([FromBody] ReviewDto reviewDto)
	{
		var userEmail = User.FindFirstValue("Email"); 
		var user = await _userManager.FindByEmailAsync(userEmail);
		var returnResult = await _reviewService.AddReviewAsync(reviewDto, user);
		return returnResult;
	}
		
	[HttpPut("{id}")]
	public async Task<ReviewToReturnDto> UpdateReview(int id, [FromBody] ReviewDto reviewDto)
	{
		if (id != reviewDto.Id)
		{
			return null;
		}

		var userEmail = User.FindFirstValue("Email");
		var user = await _userManager.FindByEmailAsync(userEmail);
		await _reviewService.UpdateReviewAsync(reviewDto,user);
		return await _reviewService.UpdateReviewAsync(reviewDto, user); ;
	}

	[HttpDelete("{id}")]
	public async Task<IReadOnlyList<ReviewToReturnDto>> DeleteReview(int id)
		 => await _reviewService.DeleteReviewAsync(id);

}
