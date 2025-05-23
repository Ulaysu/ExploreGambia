﻿using Asp.Versioning;
using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]  // Specify API version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IMapper mapper;
        private readonly ILogger<PaymentsController> logger;

        public PaymentsController(IPaymentRepository paymentRepository, IMapper mapper, 
            ILogger<PaymentsController> logger)
        {
            this.paymentRepository = paymentRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments([FromQuery] string? paymentMethod,
    [FromQuery] DateTime? paymentDateFrom,
    [FromQuery] DateTime? paymentDateTo,
    [FromQuery] bool? isSuccessful, [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
    [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var payments = await paymentRepository.GetAllPaymentsAsync(paymentMethod, paymentDateFrom, paymentDateTo, isSuccessful, sortBy, isAscending ?? true, pageNumber, pageSize);

            return Ok(mapper.Map<List<PaymentDto>>(payments));
        }

        // Get Payment By Id Get: api/Payments/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetPaymentById([FromRoute] Guid id)
        {

            var payment = await paymentRepository.GetPaymentById(id);

            return Ok(mapper.Map<PaymentDto>(payment));
        }

        // Create Payment
        [HttpPost]

        public async Task<IActionResult> CreatePayment([FromBody] AddPaymentRequestDto addPaymentRequestDto)
        {
            var payment = mapper.Map<Payment>(addPaymentRequestDto);

            payment = await paymentRepository.CreatePaymentAsync(payment);

            var paymentDto = mapper.Map<PaymentDto>(payment);


            return CreatedAtAction(nameof(GetPaymentById), new { id = paymentDto.PaymentId }, paymentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(Guid id, UpdatePaymentRequestDto updatePaymentRequestDto)
        {
            var payment = mapper.Map<Payment>(updatePaymentRequestDto);

            payment = await paymentRepository.UpdatePaymentAsync(id, payment);
            
           
            return Ok(mapper.Map<PaymentDto>(payment));
        }

        // Delete Payment
        [HttpDelete]
        [Route("{id:Guid}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBooking([FromRoute] Guid id)
        {
            var payment = await paymentRepository.DeletePaymentAsync(id);


            // Log the successful deletion, including non-sensitive information
            logger.LogInformation($"Payment with ID '{id}' deleted successfully. Associated Booking ID: '{payment.BookingId}'.");

            // NOT returning the entire payment object due to PII concerns

            // Optionally return a success message or a minimal DTO without sensitive data
            return Ok(new { Message = $"Payment with ID '{id}' deleted successfully." });


        }
    }
}
