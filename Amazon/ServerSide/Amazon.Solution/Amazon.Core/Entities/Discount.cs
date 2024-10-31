using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities
{
	 [Owned]
	public class Discount 
{
		[Column(TypeName = "decimal(5, 2)")] 
		public decimal DiscountPercentage { get; set; }
		[Column(TypeName = "money")]
		public decimal? PriceAfterDiscount { get; set; }
		public bool DiscountStarted { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
