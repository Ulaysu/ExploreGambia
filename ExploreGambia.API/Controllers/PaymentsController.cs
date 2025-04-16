using Asp.Versioning;
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

        public PaymentsController(IPaymentRepository paymentRepository, IMapper mapper)
        {
            this.paymentRepository = paymentRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments([FromQuery] string? sortBy, [FromQuery] bool? isAscending)
        {
            var payments = await paymentRepository.GetAllPaymentsAsync(sortBy, isAscending ?? true);

            return Ok(mapper.Map<List<PaymentDto>>(payments));
        }

        // Get Payment By Id Get: api/Payments/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetPaymentById([FromRoute] Guid id)
        {

            var payment = await paymentRepository.GetPaymentById(id);

            if (payment == null)
            {
                return NotFound();
            }

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
            if (payment == null)
            {
                return NotFound("Payment not found.");
            }

            return Ok(mapper.Map<PaymentDto>(payment));
        }

        // Delete Payment
        [HttpDelete]
        [Route("{id:Guid}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBooking([FromRoute] Guid id)
        {
            var payment = await paymentRepository.DeletePaymentAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            // return deleted Payment back
            // Convert Domain Model to DTO
            var paymentDto = mapper.Map<PaymentDto>(payment);


            return Ok(payment);


        }
    }
}
