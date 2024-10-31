using Amazon.Core.Entities;
using Amazon.Core.Entities.OrderAggregate;
using Amazon.Services.CartService.Dto;
using Amazone.Infrastructure.Interfaces;
using Amazone.Infrastructure.Repos;
using Amazone.Infrastructure.Specification.OrderSpecefifcations;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Product = Amazon.Core.Entities.Product;

namespace Amazon.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IGenericCacheRepository<Cart> _cartRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<DeliveryMethod> _deliveryMethodRepo;
        private readonly IGenericRepository<Order> _orderRepo;
        public PaymentService(IConfiguration config, IMapper mapper, IGenericCacheRepository<Cart> cartRepo, 
            IGenericRepository<Product> productRepo, IGenericRepository<DeliveryMethod> deliveryMethodRepo, 
            IGenericRepository<Order> orderRepo)
        {
            _config = config;
            _mapper = mapper;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _deliveryMethodRepo = deliveryMethodRepo;
            _orderRepo = orderRepo;
        }

        public async Task<CartDto> SetPaymentIntent(string cartId)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

            var cart = await _cartRepo.GetAsync(cartId);
            if (cart == null) return null;

            var shippingCost = 0m;
            if (cart.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _deliveryMethodRepo.GetByIdAsync(cart.DeliveryMethodId.Value);
                if (deliveryMethod != null)
                {
                    shippingCost = deliveryMethod.Cost;
                    cart.ShippingPrice = deliveryMethod.Cost;
                }
            }

            // Checking for items price coming from client and handling discount
            foreach (var item in cart.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.Id);

                if (product?.Discount != null &&
                    product.Discount.DiscountStarted && 
                    product.Discount.StartDate.Date <= DateTime.Now &&  
                    product.Discount.EndDate >= DateTime.Now) 
                {
                    if (product.Discount.PriceAfterDiscount.HasValue)
                    {
                        item.Price = product.Discount.PriceAfterDiscount.Value; 
                    }
                    else
                    {
                        item.Price = product.Price - (product.Price * (product.Discount.DiscountPercentage / 100));
                    }
                }
                else
                {
                    item.Price = product.Price;
                }
            }

            var service = new PaymentIntentService();
            PaymentIntent intent;
            if (string.IsNullOrEmpty(cart.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)cart.Items.Sum(i => i.Quantity * (i.Price * 100)) +
                        (long)shippingCost * 100,
                    Currency = "egp",
                    PaymentMethodTypes = new List<string> {"card"}
                };
                intent = await service.CreateAsync(options);
                cart.PaymentIntentId = intent.Id;
                cart.ClientSecret = intent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = (long)cart.Items.Sum(i => i.Quantity * (i.Price * 100)) +
                        (long)shippingCost * 100,
                };
                await service.UpdateAsync(cart.PaymentIntentId, options);
            }

            await _cartRepo.CreateOrUpdateAsync(cart.Id, cart);

            return _mapper.Map<CartDto>(cart);
        }


		public async Task<Order> UpdatePaymentIntentToSucceededOrFailed(string paymentIntentId, bool isSusseeded)
		{
			var spec = new OrderWithPaymentIntentSpecification(paymentIntentId);

			var order = await _orderRepo.GetWithSpecAsync(spec);

			if (isSusseeded)
				order.PaymentStatus = PaymentStatus.PaymentRecieved;
			else
				order.PaymentStatus = PaymentStatus.PaymentFailed;

			await _orderRepo.Update(order);

			return order;
		}

		public async Task<bool> RefundPayment(string paymentIntentId)
		{
			var spec = new OrderWithPaymentIntentSpecification(paymentIntentId);

			var order = await _orderRepo.GetWithSpecAsync(spec);
			var refundOptions = new RefundCreateOptions { PaymentIntent = paymentIntentId };
			var client = new StripeClient(_config["StripeSettings:SecretKey"]);
			var service = new RefundService(client);
			var refund = await service.CreateAsync(refundOptions);

            if (refund.Status == "succeeded")
                return true;

            return false;

		}

		//public async Task UpdateOrderStatusToRefunded(string paymentIntentId)
		//{
		//	var orderSpec = new OrderWithPaymentIntentSpecification(paymentIntentId);
		//	var order = await _orderRepo.GetWithSpecAsync(orderSpec);

		//	if (order != null)
		//	{
		//		order.PaymentStatus = PaymentStatus.Refunded;
		//		order.OrderStatus = OrderStatus.Cancelled; // Set order as canceled if applicable
		//		await _orderRepo.Update(order);
		//	}
		//}
	}
}
