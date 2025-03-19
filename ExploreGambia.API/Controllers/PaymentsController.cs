using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExploreGambia.API.Controllers
{
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await paymentRepository.GetAllPaymentsAsync();

            return Ok(mapper.Map<List<PaymentDto>>(payments));
        }

        // Get Payment By Id Get: api/Payments/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetPaymentGuideById([FromRoute] Guid id)
        {

            var payment = await paymentRepository.GetPaymentById(id);

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<PaymentDto>(payment));
        }
    }
}
