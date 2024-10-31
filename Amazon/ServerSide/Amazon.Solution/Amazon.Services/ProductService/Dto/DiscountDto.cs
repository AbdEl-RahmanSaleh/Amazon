using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Services.ProductService.Dto
{
	public class DiscountDto
	{
		[Range(0, 99)]
		[Column(TypeName = "decimal(5, 2)")]
		public decimal? DiscountPercentage { get; set; }
		[Column(TypeName = "money")]
		public decimal? PriceAfterDiscount { get; set; }
		public bool? DiscountStarted { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
}
