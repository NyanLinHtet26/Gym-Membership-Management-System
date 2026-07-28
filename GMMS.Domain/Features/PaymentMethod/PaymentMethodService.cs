 using FluentValidation;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.PaymentMethod.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.PaymentMethod
{
    public class PaymentMethodService
    {
        private readonly AppDbContext _db;
        private readonly IValidator<PaymentMethodListRequestModel> _listValidator;
        private readonly IValidator<PaymentMethodCreateRequestModel> _createValidator;
        private readonly IValidator<PaymentMethodUpdateRequestModel> _updateValidator;
        private readonly ILogger<PaymentMethodService> _logger;

        public PaymentMethodService(
            AppDbContext db,
            IValidator<PaymentMethodListRequestModel> listValidator,
            IValidator<PaymentMethodCreateRequestModel> createValidator,
            IValidator<PaymentMethodUpdateRequestModel> updateValidator,
            ILogger<PaymentMethodService> logger)
        {
            _db = db;
            _listValidator = listValidator;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<Result<PaymentMethodListResponseModel>> GetList(PaymentMethodListRequestModel request)
        {
            _logger.LogInformation("Retrieving payment method list with PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            var validationResult = await _listValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid payment method list request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<PaymentMethodListResponseModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            
                var query = _db.TblPaymentMethods
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.Trim().ToLower();
                    query = query.Where(x =>
                        x.PaymentMethodCode.ToLower().Contains(search) ||
                        x.Name.ToLower().Contains(search));
                }

                var totalCount = await query.CountAsync();
                var paymentMethods = await query

                    .OrderByDescending(x => x.PaymentMethodId)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new PaymentMethodModel
                    {
                        PaymentMethodId = x.PaymentMethodId,
                        PaymentMethodCode = x.PaymentMethodCode,
                        Name = x.Name,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt,
                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == x.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                        UpdatedAt = x.UpdatedAt,
                        UpdatedByUser = x.UpdatedBy.HasValue
                            ? x.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == x.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                            : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} payment methods out of {TotalCount} total.", paymentMethods.Count, totalCount);

                return new Result<PaymentMethodListResponseModel>
                {
                    IsSuccess = true,
                    Message = "Payment methods retrieved successfully.",
                    Data = new PaymentMethodListResponseModel
                    {
                        TotalCount = totalCount,
                        PaymentMethods = paymentMethods
                    }
                };
            
        }

        public async Task<Result<PaymentMethodModel>> GetById(int paymentMethodId)
        {
            _logger.LogInformation("Retrieving payment method with ID: {PaymentMethodId}", paymentMethodId);

                var paymentMethod = await _db.TblPaymentMethods
                    .AsNoTracking()
                    .Where(x => x.PaymentMethodId == paymentMethodId && !x.IsDeleted)
                    .Select(x => new PaymentMethodModel
                    {
                        PaymentMethodId = x.PaymentMethodId,
                        PaymentMethodCode = x.PaymentMethodCode,
                        Name = x.Name,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt,
                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == x.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                        UpdatedAt = x.UpdatedAt,
                        UpdatedByUser = x.UpdatedBy.HasValue
                            ? x.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == x.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                            : null
                    })
                    .FirstOrDefaultAsync();

                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method with ID: {PaymentMethodId} not found.", paymentMethodId);
                    return new Result<PaymentMethodModel>
                    {
                        IsSuccess = false,
                        Message = "Payment method not found."
                    };
                }

                _logger.LogInformation("Payment method with ID: {PaymentMethodId} retrieved successfully.", paymentMethodId);
                return new Result<PaymentMethodModel>
                {
                    IsSuccess = true,
                    Message = "Payment method retrieved successfully.",
                    Data = paymentMethod
                };
            
        }

        public async Task <Result<PaymentMethodModel>> Create(PaymentMethodCreateRequestModel request)
        {
            _logger.LogInformation("Creating payment method with PaymentMethodCode: {PaymentMethodCode}, Name: {Name}", request.PaymentMethodCode, request.Name);

            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid payment method creation request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<PaymentMethodModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            
                var exists = await _db.TblPaymentMethods
                    .AnyAsync(x => !x.IsDeleted && x.PaymentMethodCode
                    .ToUpper() == request.PaymentMethodCode
                    .ToUpperInvariant());
                if (exists)
                {
                    _logger.LogWarning("Payment method with code: {PaymentMethodCode} already exists.", request.PaymentMethodCode);
                    return new Result<PaymentMethodModel>
                    {
                        IsSuccess = false,
                        Message = "Payment method code already exists."
                    };
                }
                var paymentMethod = new TblPaymentMethod
                {
                    PaymentMethodCode = request.PaymentMethodCode.ToUpperInvariant(),
                    Name = request.Name.Trim(),
                    IsActive = request.IsActive,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _db.TblPaymentMethods.AddAsync(paymentMethod);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payment method created successfully with PaymentMethodId: {PaymentMethodId}", paymentMethod.PaymentMethodId);

                return new Result<PaymentMethodModel>
                {
                    IsSuccess = true,
                    Message = "Payment method created successfully.",
                    Data = new PaymentMethodModel
                    {
                        PaymentMethodId = paymentMethod.PaymentMethodId,
                        PaymentMethodCode = paymentMethod.PaymentMethodCode,
                        Name = paymentMethod.Name,
                        IsActive = paymentMethod.IsActive,
                        CreatedAt = paymentMethod.CreatedAt,
                        CreatedByUser = paymentMethod.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == paymentMethod.CreatedBy).Select(u => u.UserName).FirstOrDefault()
                    }
                };
           
        }

        public async Task<Result<PaymentMethodModel>> Update(int id, PaymentMethodUpdateRequestModel request)
        {
            _logger.LogInformation("Updating payment method with ID: {PaymentMethodId}, Code: {PaymentMethodCode}, Name: {Name}", id, request.PaymentMethodCode, request.Name);

            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid payment method update request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<PaymentMethodModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            
                var paymentMethod = await _db.TblPaymentMethods
                    .Where(x => x.PaymentMethodId == request.PaymentMethodId && !x.IsDeleted)
                    .FirstOrDefaultAsync();
                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method with ID: {PaymentMethodId} not found for update.", request.PaymentMethodId);
                    return new Result<PaymentMethodModel>
                    {
                        IsSuccess = false,
                        Message = "Payment method not found."
                    };
                }
                var exists = await _db.TblPaymentMethods
                    .AnyAsync(x => !x.IsDeleted && x.PaymentMethodCode.ToUpper() == request.PaymentMethodCode.ToUpperInvariant() && x.PaymentMethodId != request.PaymentMethodId);
                if (exists)
                {
                    _logger.LogWarning("Payment method with code: {PaymentMethodCode} already exists.", request.PaymentMethodCode);
                    return new Result<PaymentMethodModel>
                    {
                        IsSuccess = false,
                        Message = "Payment method code already exists."
                    };
                }

                paymentMethod.PaymentMethodCode = request.PaymentMethodCode.ToUpperInvariant();
                paymentMethod.Name = request.Name.Trim();
                paymentMethod.IsActive = request.IsActive;
                paymentMethod.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payment method with ID: {PaymentMethodId} updated successfully.", request.PaymentMethodId);

                return new Result<PaymentMethodModel>
                {
                    IsSuccess = true,
                    Message = "Payment method updated successfully.",
                    Data = new PaymentMethodModel
                    {
                        PaymentMethodId = paymentMethod.PaymentMethodId,
                        PaymentMethodCode = paymentMethod.PaymentMethodCode,
                        Name = paymentMethod.Name,
                        IsActive = paymentMethod.IsActive,
                        CreatedAt = paymentMethod.CreatedAt,
                        CreatedByUser = paymentMethod.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == paymentMethod.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                        UpdatedAt = paymentMethod.UpdatedAt,
                        UpdatedByUser = paymentMethod.UpdatedBy.HasValue
                            ? paymentMethod.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == paymentMethod.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                            : null
                    }
                };
            
           
        }

        public async Task <Result<bool>> Delete(int paymentMethodId)
        {
            _logger.LogInformation("Deleting payment method with ID: {PaymentMethodId}", paymentMethodId);

            
                var paymentMethod = await _db.TblPaymentMethods
                    .Where(x => x.PaymentMethodId == paymentMethodId && !x.IsDeleted)
                    .FirstOrDefaultAsync();
                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method with ID: {PaymentMethodId} not found for deletion.", paymentMethodId);
                    return new Result<bool>
                    {
                        IsSuccess = false,
                        Message = "Payment method not found."
                    };
                }
                paymentMethod.IsDeleted = true;
                paymentMethod.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payment method with ID: {PaymentMethodId} deleted successfully.", paymentMethodId);

                return new Result<bool>
                {
                    IsSuccess = true,
                    Message = "Payment method deleted successfully.",
                    Data = true
                };
            
        }
    }
}