using GMMS.Domain.Features.PaymentMethod;
using GMMS.Domain.Features.PaymentMethod.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "MustNotRequirePasswordChange")]
    public class PaymentMethodController : BaseController
    {
        private readonly PaymentMethodService _paymentMethodService;
        private readonly ILogger<PaymentMethodController> _logger;

        public PaymentMethodController(PaymentMethodService paymentMethodService, ILogger<PaymentMethodController> logger)
        {
            _paymentMethodService = paymentMethodService;
            _logger = logger;
        }
        [HttpGet]
        public async Task <IActionResult> PaymentMethodList([FromQuery] PaymentMethodListRequestModel request)
        {
            _logger.LogInformation("PaymentMethodList API called. Page={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            var result = await _paymentMethodService.GetList(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("PaymentMethodList API successful. Total payment methods fetched: {Count}", result.Data?.PaymentMethods?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("PaymentMethodList API failed. Message: {Message}", result.Message);
            }
            return Execute(result);
        }
        [HttpGet("{id}")]
        public async Task <IActionResult> GetPaymentMethod([FromRoute] int id)
        {
            _logger.LogInformation("GetPaymentMethod API called. PaymentMethodId={PaymentMethodId}", id);

            var result = await _paymentMethodService.GetById(id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetPaymentMethod API completed successfully. PaymentMethodId={PaymentMethodId}", id);
            }
            else
            {
                _logger.LogWarning("GetPaymentMethod API failed. PaymentMethodId={PaymentMethodId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
        [HttpPost]
        public async Task <IActionResult> CreatePaymentMethod([FromBody] PaymentMethodCreateRequestModel request)
        {
            _logger.LogInformation("CreatePaymentMethod API called. PaymentMethodCode={PaymentMethodCode}, Name={Name}", request.PaymentMethodCode, request.Name);

            var result = await _paymentMethodService.Create(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("CreatePaymentMethod API completed successfully. PaymentMethodCode={PaymentMethodCode}", request.PaymentMethodCode);
            }
            else
            {
                _logger.LogWarning("CreatePaymentMethod API failed. Message={Message}", result.Message);
            }
            return Execute(result);
        }
        [HttpPut("{id}")]
        public async Task <IActionResult> UpdatePaymentMethod([FromRoute] int id, [FromBody] PaymentMethodUpdateRequestModel request)
        {
            _logger.LogInformation("UpdatePaymentMethod API called. PaymentMethodId={PaymentMethodId}, Code={PaymentMethodCode}", id, request.PaymentMethodCode);

            if (id != request.PaymentMethodId)
            {
                _logger.LogWarning("Route ID does not match request body ID. RouteId={RouteId}, BodyId={BodyId}", id, request.PaymentMethodId);
                return BadRequest("Payment Method ID in the route does not match the ID in the request body.");
            }
            var result = await _paymentMethodService.Update(id, request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("UpdatePaymentMethod API completed successfully. PaymentMethodId={PaymentMethodId}", id);
            }
            else
            {
                _logger.LogWarning("UpdatePaymentMethod API failed. PaymentMethodId={PaymentMethodId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
        [HttpDelete("{id}")]
        public async Task <IActionResult> DeletePaymentMethod([FromRoute] int id)
        {
            _logger.LogInformation("DeletePaymentMethod API called. PaymentMethodId={PaymentMethodId}", id);

            var result = await _paymentMethodService.Delete(id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("DeletePaymentMethod API completed successfully. PaymentMethodId={PaymentMethodId}", id);
            }
            else
            {
                _logger.LogWarning("DeletePaymentMethod API failed. PaymentMethodId={PaymentMethodId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        } 

    }
}
