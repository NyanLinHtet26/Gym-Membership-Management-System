using FluentValidation;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.Payment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.Payment
{
    public class PaymentService
    {
        private readonly AppDbContext _db;
        private readonly IValidator<PaymentListRequestModel> _listValidator;
        private readonly IValidator<CreatePaymentRequestModel> _createValidator;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            AppDbContext db,
            IValidator<PaymentListRequestModel> listValidator,
            IValidator<CreatePaymentRequestModel> createValidator,
            ILogger<PaymentService> logger)
        {
            _db = db;
            _listValidator = listValidator;
            _createValidator = createValidator;
            _logger = logger;
        }

        public async Task<Result<PaymentListResponseModel>> GetList(PaymentListRequestModel request)
        {
            _logger.LogInformation("Retrieving payment list. PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            var validationResult = await _listValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid payment list request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<PaymentListResponseModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            
                var query = _db.TblPayments
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.Trim().ToLower();
                    query = query.Where(x =>
                        x.PaymentMethod.Name.ToLower().Contains(search) ||
                        x.Status.ToLower().Contains(search) ||
                        x.Membership.Member.Name.ToLower().Contains(search) ||
                        x.Membership.MembershipPlan.PlanName.ToLower().Contains(search));
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(x => x.CreatedAt <= request.ToDate.Value);
                }

                var totalCount = await  query.CountAsync();


                var payments = await query
                    .OrderByDescending(x => x.PaymentId)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new PaymentModel
                    {
                        PaymentId = x.PaymentId,

                        MembershipId = x.MembershipId,

                        PaymentMethodName = x.PaymentMethod.Name,

                        Amount = x.Amount,

                        Sspath = x.Sspath,

                        Status = x.Status,

                        CreatedAt = x.CreatedAt,
                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == x.CreatedBy).Select(u => u.UserName).FirstOrDefault()
                    })
                    .ToListAsync();


                _logger.LogInformation("Payments retrieved successfully. TotalCount={TotalCount}, PagePayments={PagePayments}", totalCount, payments.Count);
                return new Result<PaymentListResponseModel>
                {
                    IsSuccess = true,
                    Message = "Payments retrieved successfully.",
                    Data = new PaymentListResponseModel
                    {
                        TotalCount = totalCount,
                        Payments = payments
                    }
                };
            
        }

        public async Task< Result<PaymentDetailModel> >GetById(int paymentId)
        {
            _logger.LogInformation("Retrieving payment with ID: {PaymentId}", paymentId);

            var payment = await _db.TblPayments
                    .AsNoTracking()
                    .Where(x => x.PaymentId == paymentId)
                    .Select(x => new PaymentDetailModel
                    {
                        PaymentId = x.PaymentId,
                        MemberName = x.Membership.Member.Name,
                        MembershipName = x.Membership.MembershipPlan.PlanName,
                        PaymentMethodName = x.PaymentMethod.Name,
                        Amount = x.Amount,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt,
                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == x.CreatedBy).Select(u => u.UserName).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();
                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID: {PaymentId} not found.", paymentId);
                    return new Result<PaymentDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Payment not found."
                    };
                }
                _logger.LogInformation("Payment with ID: {PaymentId} retrieved successfully.", paymentId);
                return new Result<PaymentDetailModel>
                {
                    IsSuccess = true,
                    Message = "Payment retrieved successfully.",
                    Data = payment
                };
            
        }

        public async Task <Result<PaymentModel>> Create(int createdByUserId, CreatePaymentRequestModel request)
        {
            _logger.LogInformation("Creating payment for MembershipId={MembershipId}, Amount={Amount}, PaymentMethodId={PaymentMethodId}", request.MembershipId, request.Amount, request.PaymentMethodId);

            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid payment creation request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<PaymentModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

             var payment = await _db.TblPayments
                    .FirstOrDefaultAsync(x => x.MembershipId == request.MembershipId && x.Status == "Pending");
                if (payment != null)
                {
                    _logger.LogWarning("A pending payment already exists for MembershipId={MembershipId}.", request.MembershipId);
                    return new Result<PaymentModel>
                    {
                        IsSuccess = false,
                        Message = "A pending payment already exists for this membership."
                    };
                }

                var PaymentMethod = await _db.TblPaymentMethods
                    .FirstOrDefaultAsync(x => x.PaymentMethodId == request.PaymentMethodId && !x.IsDeleted && x.IsActive);
                     
                if (PaymentMethod == null)
                {
                    _logger.LogWarning("Payment method not found or inactive. PaymentMethodId={PaymentMethodId}", request.PaymentMethodId);
                    return new Result<PaymentModel>
                    {
                        IsSuccess = false,
                        Message = "Payment method not found or inactive."
                    };
                }
                
                var membership = await _db.TblMemberships
                    .FirstOrDefaultAsync(x => x.MembershipId == request.MembershipId && !x.IsDeleted);
                if (membership == null)
                {
                    _logger.LogWarning("Membership not found. MembershipId={MembershipId}", request.MembershipId);
                    return new Result<PaymentModel>
                    {
                        IsSuccess = false,
                        Message = "Membership not found."
                    };
                }


                var newPayment = new TblPayment
                {
                    MembershipId = request.MembershipId,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = request.Amount,
                    Sspath = request.Sspath,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _db.TblPayments.AddAsync(newPayment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payment created successfully. PaymentId={PaymentId}, MembershipId={MembershipId}", newPayment.PaymentId, request.MembershipId);

                return new Result<PaymentModel>
                {
                    IsSuccess = true,
                    Message = "Payment created successfully."
                };
            
           
        }
    }
}