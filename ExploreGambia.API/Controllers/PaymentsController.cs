using Asp.Versioning;
using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services;
using ExploreGambia.API.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using ExploreGambia.API.Services.Payments;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]  // Specify API version
    [Route("api/v{version:apiVersion}/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IPaymentService paymentService;
        private readonly IMapper mapper;
        private readonly ILogger<PaymentsController> logger;
        private readonly IStripePaymentService stripePaymentService;

        public PaymentsController(IPaymentRepository paymentRepository, IPaymentService paymentService, IStripePaymentService stripePaymentService, IMapper mapper,
            ILogger<PaymentsController> logger)
        {
            this.paymentRepository = paymentRepository;
            this.paymentService = paymentService;
            this.mapper = mapper;
            this.logger = logger;
            this.stripePaymentService = stripePaymentService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayments([FromQuery] string? paymentMethod,
    [FromQuery] DateTime? paymentDateFrom,
    [FromQuery] DateTime? paymentDateTo,
    [FromQuery] PaymentStatus? status, [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
    [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var payments = await paymentRepository.GetAllPaymentsAsync(paymentMethod, paymentDateFrom, paymentDateTo, status, sortBy, isAscending ?? true, pageNumber, pageSize);

            return Ok(mapper.Map<List<PaymentDto>>(payments));
        }

        // Get Payment By Id Get: api/Payments/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetPaymentById([FromRoute] Guid id)
        {

            var payment = await paymentRepository.GetPaymentById(id);

            return Ok(mapper.Map<PaymentDto>(payment));
        }

        // Create Payment
        [HttpPost]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreatePayment([FromBody] AddPaymentRequestDto addPaymentRequestDto)
        {
            var payment = await paymentService.CreatePaymentAsync(addPaymentRequestDto);

            var paymentDto = mapper.Map<PaymentDto>(payment);


            return CreatedAtAction(nameof(GetPaymentById), new { id = paymentDto.PaymentId }, paymentDto);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePayment(Guid id, UpdatePaymentRequestDto updatePaymentRequestDto)
        {
            var payment = await paymentService.UpdatePaymentAsync(id, updatePaymentRequestDto);
            
           
            return Ok(mapper.Map<PaymentDto>(payment));
        }

        [HttpPost("{id:guid}/confirm")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmPayment([FromRoute] Guid id, [FromBody] ConfirmPaymentRequestDto confirmPaymentRequestDto)
        {
            var payment = await paymentService.ConfirmPaymentAsync(id, confirmPaymentRequestDto);

            return Ok(mapper.Map<PaymentDto>(payment));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("bookings/{bookingId:guid}/modempay/inline")]
        public async Task<IActionResult> PrepareModemPayInlinePayment(
            [FromRoute] Guid bookingId,
            [FromBody] ModemPayInlinePaymentRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User identity could not be determined.");
            }

            var customerContext = new ModemPayCustomerContextDto
            {
                UserId = userId,
                IsAdmin = User.IsInRole("Admin"),
                Email = User.FindFirstValue(ClaimTypes.Email),
                Name = User.Identity?.Name
            };

            var checkout = await paymentService.PrepareModemPayInlinePaymentAsync(bookingId, request, customerContext);
            return Ok(checkout);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("modempay/verify")]
        public async Task<IActionResult> VerifyModemPayPayment([FromBody] VerifyModemPayPaymentRequestDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User identity could not be determined.");
            }

            var customerContext = new ModemPayCustomerContextDto
            {
                UserId = userId,
                IsAdmin = User.IsInRole("Admin"),
                Email = User.FindFirstValue(ClaimTypes.Email),
                Name = User.Identity?.Name
            };

            var payment = await paymentService.VerifyModemPayPaymentAsync(request, customerContext, cancellationToken);
            return Ok(mapper.Map<PaymentDto>(payment));
        }

        [AllowAnonymous]
        [HttpPost("modempay/webhook")]
        public async Task<IActionResult> HandleModemPayWebhook([FromServices] IModemPayClient modemPayClient)
        {
            using var reader = new StreamReader(Request.Body);
            var rawPayload = await reader.ReadToEndAsync();
            var signature = Request.Headers["x-modem-signature"].FirstOrDefault();

            if (!modemPayClient.IsValidWebhookSignature(rawPayload, signature))
            {
                return BadRequest(new { Message = "Invalid Modem Pay signature." });
            }

            var webhookEvent = JsonSerializer.Deserialize<ModemPayWebhookEvent>(
                rawPayload,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (webhookEvent == null || string.IsNullOrWhiteSpace(webhookEvent.Event))
            {
                return BadRequest(new { Message = "Invalid Modem Pay webhook payload." });
            }

            await paymentService.ProcessModemPayWebhookAsync(webhookEvent);
            return Ok(new { Received = true });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("bookings/{bookingId:guid}/modempay/intent")]
        public async Task<IActionResult> CreateModemPayIntent([FromRoute] Guid bookingId,[FromBody] CreateModemPayIntentRequestDto request,
        CancellationToken cancellationToken)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var customerContext =
                new ModemPayCustomerContextDto
                {
                    UserId = userId,
                    IsAdmin = User.IsInRole("Admin"),
                    Email = User.FindFirstValue(ClaimTypes.Email),
                    Name = User.Identity?.Name
                };

            var response =
                await paymentService.CreateModemPayPaymentIntentAsync(
                    bookingId,
                    request,
                    customerContext,
                    cancellationToken);

            return Ok(response);
        }

        // Delete Payment
        [HttpDelete]
        [Route("{id:Guid}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBooking([FromRoute] Guid id)
        {
            var payment = await paymentRepository.DeletePaymentAsync(id)
                ?? throw new PaymentNotFoundException(id);


            // Log the successful deletion, including non-sensitive information
            logger.LogInformation($"Payment with ID '{id}' deleted successfully. Associated Booking ID: '{payment.BookingId}'.");

            // NOT returning the entire payment object due to PII concerns

            // Optionally return a success message or a minimal DTO without sensitive data
            return Ok(new { Message = $"Payment with ID '{id}' deleted successfully." });


        }


        [Authorize(Roles = "User,Admin")]
        [HttpPost("bookings/{bookingId:guid}/stripe/checkout")]
        public async Task<IActionResult>
    CreateStripeCheckout(
        [FromRoute] Guid bookingId,
        [FromBody] CreateStripeCheckoutRequestDto request)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var response =
                await stripePaymentService
                    .CreateCheckoutSessionAsync(
                        bookingId,
                        userId,
                        request);

            return Ok(response);
        }
    }

}
