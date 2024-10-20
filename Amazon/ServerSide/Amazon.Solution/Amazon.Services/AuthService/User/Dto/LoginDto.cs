﻿using System.ComponentModel.DataAnnotations;

namespace Amazon.Services.AuthService.User.Dto
{
	public class LoginDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
