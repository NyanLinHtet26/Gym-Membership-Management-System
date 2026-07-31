using GMMS.Domain.Features.Payment;
using GMMS.Domain.Features.Payment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PaymentList([FromQuery] PaymentListRequestModel request)
        {
            _logger.LogInformation("PaymentList API called. PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            var result = await _paymentService.GetList(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("PaymentList API successful. TotalCount={TotalCount}", result.Data?.TotalCount ?? 0);
            }
            else
            {
                _logger.LogWarning("PaymentList API failed. Message={Message}", result.Message);
            }
            return Execute(result);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetail(int id)
        {
            _logger.LogInformation("GetPaymentDetail API called. PaymentId={PaymentId}", id);

            var result = await _paymentService.GetById(id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetPaymentDetail API completed successfully. PaymentId={PaymentId}", id);
            }
            else
            {
                _logger.LogWarning("GetPaymentDetail API failed. PaymentId={PaymentId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestModel request)
        {
            _logger.LogInformation("CreatePayment API called. MembershipId={MembershipId}, Amount={Amount}", request.MembershipId, request.Amount);

            var result = await _paymentService.Create(GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("CreatePayment API completed successfully. MembershipId={MembershipId}", request.MembershipId);
            }
            else
            {
                _logger.LogWarning("CreatePayment API failed. Message={Message}", result.Message);
            }
            return Execute(result);
        }
    }
}
