using FluentValidation;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Enums;
using GMMS.Domain.Features.Member;
using GMMS.Domain.Features.MemberShip.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.MemberShip
{
    public class MemberShipService
    {
        private readonly AppDbContext _db;
        private readonly IValidator<MemberShipListRequestModel> _listValidator;
        private readonly IValidator<AllMemberShipListRequestModel> _allListValidator;
        private readonly IValidator<CreateMemberShipRequestModel> _createValidator;
        private readonly IValidator<UpdateMembershipRequestModel> _updateValidator;
        private readonly ILogger<MemberShipService> _logger;

        public MemberShipService(
            AppDbContext db,
            IValidator<MemberShipListRequestModel> listValidator,
            IValidator<AllMemberShipListRequestModel> allListValidator,
            IValidator<CreateMemberShipRequestModel> createValidator,
            IValidator<UpdateMembershipRequestModel> updateValidator,
            ILogger<MemberShipService> logger)

        {
            _db = db;
            _listValidator = listValidator;
            _allListValidator = allListValidator;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<Result<MemberShipListResponseModel>> GetList(MemberShipListRequestModel request)
        {
            _logger.LogInformation("Retrieving membership list for MemberId: {MemberId}, PageNumber: {PageNumber}, PageSize: {PageSize}", request.MemberId, request.PageNumber, request.PageSize);

            var validationResult = await _listValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid membership list request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MemberShipListResponseModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }
            var query = _db.TblMemberships
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.MemberId == request.MemberId);

                var totalCount = query.Count();

                var memberships = await query
                    .OrderByDescending(x => x.MembershipId)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new MemberShipModel
                    {
                        MembershipId = x.MembershipId,

                        MemberCode = x.Member.MemberCode,
                        MemberName = x.Member.Name,

                        PlanCode = x.MembershipPlan.PlanCode,
                        PlanName = x.MembershipPlan.PlanName,

                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Status = x.Status,

                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers
                        .Where(u => u.UserId == x.CreatedBy)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),

                        UpdatedByUser = x.UpdatedBy.HasValue
                            ? x.UpdatedBy.Value + " - " + _db.TblUsers
                            .Where(u => u.UserId == x.UpdatedBy.Value)
                            .Select(u => u.UserName)
                            .FirstOrDefault()
                            : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} memberships out of {TotalCount} total for MemberId: {MemberId}", memberships.Count, totalCount, request.MemberId);

                return new Result<MemberShipListResponseModel>
                {
                    IsSuccess = true,
                    Message = "Memberships retrieved successfully.",
                    Data = new MemberShipListResponseModel
                    {
                        TotalCount = totalCount,
                        MemberShips = memberships
                    }
                };
        }
        public async Task <Result<MemberShipListResponseModel>> GetAllList(AllMemberShipListRequestModel request)
        {
            _logger.LogInformation("Retrieving all memberships with PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, Status: {Status}", request.PageNumber, request.PageSize, request.SearchTerm, request.Status);

            var validationResult = await _allListValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid all membership list request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MemberShipListResponseModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            
                var query = _db.TblMemberships
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.Trim().ToLower();
                    query = query.Where(x => x.Member.MemberCode.ToLower().Contains(search)
                        || x.Member.Name.Contains(search)
                        || x.MembershipPlan.PlanName.Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    query = query.Where(x => x.Status == request.Status);
                }

                var totalCount = await query.CountAsync();

                var memberships = await query
                    .OrderByDescending(x => x.MembershipId)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new MemberShipModel
                    {
                        MembershipId = x.MembershipId,
                        MemberCode = x.Member.MemberCode,
                        MemberName = x.Member.Name,

                        PlanCode = x.MembershipPlan.PlanCode,
                        PlanName = x.MembershipPlan.PlanName,

                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Status = x.Status,

                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers
                        .Where(u => u.UserId == x.CreatedBy).Select(u => u.UserName)
                        .FirstOrDefault(),

                        UpdatedByUser = x.UpdatedBy.HasValue
                            ? x.UpdatedBy.Value + " - " + _db.TblUsers
                            .Where(u => u.UserId == x.UpdatedBy.Value)
                            .Select(u => u.UserName).FirstOrDefault()
                            : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} all memberships out of {TotalCount} total.", memberships.Count, totalCount);

                return new Result<MemberShipListResponseModel>
                {
                    IsSuccess = true,
                    Message = "All memberships retrieved successfully.",
                    Data = new MemberShipListResponseModel
                    {
                        TotalCount = totalCount,
                        MemberShips = memberships
                    }
                };
        }
        public async Task <Result<MembershipDetailModel>> GetById(int membershipId)
        {
            _logger.LogInformation("Retrieving membership with ID: {MembershipId}", membershipId);

                var membership = await _db.TblMemberships
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.MembershipId == membershipId)
                    .Select(x => new MembershipDetailModel
                    {
                        MembershipId = x.MembershipId,

                        MemberId = x.MemberId,
                        MemberCode = x.Member.MemberCode,
                        MemberName = x.Member.Name,

                        MembershipPlanId = x.MembershipPlanId,
                        PlanCode = x.MembershipPlan.PlanCode,
                        PlanName = x.MembershipPlan.PlanName,

                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Status = x.Status,

                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,

                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers
                        .Where(u => u.UserId == x.CreatedBy)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                        
                        UpdatedByUser = x.UpdatedBy.HasValue
                            ? x.UpdatedBy.Value + " - " + _db.TblUsers
                            .Where(u => u.UserId == x.UpdatedBy.Value)
                            .Select(u => u.UserName)
                            .FirstOrDefault()
                            : null
                    })
                    .FirstOrDefaultAsync();

                if (membership == null)
                {
                    _logger.LogWarning("Membership with ID: {MembershipId} not found.", membershipId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership not found."
                    };
                }

                _logger.LogInformation("Membership with ID: {MembershipId} retrieved successfully.", membershipId);
                return new Result<MembershipDetailModel>
                {
                    IsSuccess = true,
                    Message = "Membership detail retrieved successfully.",
                    Data = membership
                };
            
            
        }

        public async Task <Result<MembershipDetailModel>> Create(int createdByUserId, CreateMemberShipRequestModel request)
        {
            _logger.LogInformation("Creating membership for MemberId: {MemberId}, PlanId: {PlanId}, Amount: {Amount}", request.MemberId, request.MembershipPlanId, request.Amount);

            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid membership creation request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MembershipDetailModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            
                var member = await _db.TblMembers
                    .AsNoTracking()
                    .FirstOrDefaultAsync (x => !x.IsDeleted && x.MemberId == request.MemberId);

                if (member == null)
                {
                    _logger.LogWarning("Member not found for MemberId: {MemberId}", request.MemberId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Member not found."
                    };
                }

                var plan = await _db.TblMembershipPlans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        x.MembershipPlanId == request.MembershipPlanId);

                if (plan == null)
                {
                    _logger.LogWarning("Membership plan not found or inactive for PlanId: {PlanId}", request.MembershipPlanId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership plan not found or inactive."
                    };
                }

                var exists = await _db.TblMemberships
                    .AnyAsync(x =>
                        !x.IsDeleted &&
                        x.MemberId == request.MemberId &&
                        x.Status == MembershipPlanStatus.Active.ToString());

                if (exists)
                {
                    _logger.LogWarning("Member already has active membership. MemberId: {MemberId}", request.MemberId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Member already has an active membership."
                    };
                }

                var paymentMethod = await _db.TblPaymentMethods
                    .AsNoTracking()
                    .FirstOrDefaultAsync (x => !x.IsDeleted && 
                                                x.IsActive && 
                                                x.PaymentMethodId == 
                                                request.PaymentMethodId);

                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method not found or inactive for PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Payment method not found or inactive."
                    };
                }

                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var newMembership = new TblMembership
                {
                    MemberId = request.MemberId,
                    MembershipPlanId = request.MembershipPlanId,
                    StartDate = today,
                    EndDate = today.AddDays(plan.DurationDays),
                    Status = MembershipPlanStatus.Active.ToString(),
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            try
            {
                await _db.TblMemberships
                    .AddAsync(newMembership);
               await _db.SaveChangesAsync();

                var newPayment = new TblPayment
                {
                    MembershipId = newMembership.MembershipId,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = request.Amount,
                    Sspath = request.Sspath,
                    Status = PaymentStatus.Pending.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _db.TblPayments
                    .AddAsync(newPayment);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
               await transaction.RollbackAsync();
                throw;
            }
                

                _logger.LogInformation("Membership created successfully with MembershipId: {MembershipId} for MemberId: {MemberId}", newMembership.MembershipId, request.MemberId);

                return new Result<MembershipDetailModel>
                {
                    IsSuccess = true,
                    Message = "Membership and payment created successfully.",
                    Data = new MembershipDetailModel
                    {
                        MembershipId = newMembership.MembershipId,
                        MemberId = member.MemberId,
                        MemberCode = member.MemberCode,
                        MemberName = member.Name,
                        MembershipPlanId = plan.MembershipPlanId,
                        PlanCode = plan.PlanCode,
                        PlanName = plan.PlanName,
                        StartDate = newMembership.StartDate,
                        EndDate = newMembership.EndDate,
                        Status = newMembership.Status,
                        CreatedAt = newMembership.CreatedAt,
                        CreatedByUser = newMembership.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == newMembership.CreatedBy).Select(u => u.UserName).FirstOrDefault()
                    }
                };
            
          
        }

        public async Task <Result<MembershipDetailModel>> Update(int updatedByUserId, UpdateMembershipRequestModel request)
        {
            _logger.LogInformation("Updating membership with ID: {MembershipId}, New PlanId: {PlanId}", request.MembershipId, request.MembershipPlanId);

            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid membership update request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MembershipDetailModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

           
                var membership = await _db.TblMemberships

                    .Include(x => x.Member)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.MembershipId == request.MembershipId);

                if (membership == null)
                {
                    _logger.LogWarning("Membership not found for MembershipId: {MembershipId}", request.MembershipId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership not found."
                    };
                }

                var plan = await _db.TblMembershipPlans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        x.MembershipPlanId == request.MembershipPlanId);

                if (plan == null)
                {
                    _logger.LogWarning("Membership plan not found or inactive for PlanId: {PlanId}", request.MembershipPlanId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership plan not found or inactive."
                    };
                }

                var exits = await _db.TblMemberships
                    .AnyAsync(x =>
                        !x.IsDeleted &&
                        x.MemberId == membership.MemberId &&
                        x.MembershipId != membership.MembershipId &&
                        x.Status == MembershipPlanStatus.Active.ToString());

                if (exits)
                {
                    _logger.LogWarning("Member already has another active membership. MemberId: {MemberId}", membership.MemberId);
                    return new Result<MembershipDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Member already has an active membership."
                    };
                }

                membership.MembershipPlanId = request.MembershipPlanId;
                membership.EndDate = membership.StartDate.AddDays(plan.DurationDays);
                membership.UpdatedAt = DateTime.UtcNow;
                membership.UpdatedBy = updatedByUserId;

               await  _db.SaveChangesAsync();

                _logger.LogInformation("Membership with ID: {MembershipId} updated successfully.", request.MembershipId);

                return new Result<MembershipDetailModel>
                {
                    IsSuccess = true,
                    Message = "Membership updated successfully.",
                    Data = new MembershipDetailModel
                    {
                        MembershipId = membership.MembershipId,

                        MemberId = membership.MemberId,
                        MemberCode = membership.Member.MemberCode,
                        MemberName = membership.Member.Name,

                        MembershipPlanId = plan.MembershipPlanId,
                        PlanCode = plan.PlanCode,
                        PlanName = plan.PlanName,

                        StartDate = membership.StartDate,
                        EndDate = membership.EndDate,
                        Status = membership.Status,

                        CreatedAt = membership.CreatedAt,

                        CreatedByUser = membership.CreatedBy + " - " + _db.TblUsers
                        .Where(u => u.UserId == membership.CreatedBy)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),

                        UpdatedAt = membership.UpdatedAt,

                        UpdatedByUser = membership.UpdatedBy.HasValue
                            ? membership.UpdatedBy.Value + " - " + _db.TblUsers
                            .Where(u => u.UserId == membership.UpdatedBy.Value)
                            .Select(u => u.UserName)
                            .FirstOrDefault()
                            : null
                    }
                };
        }
        public async Task<Result<bool>> Delete(int membershipId, int updatedByUserId)
        {
            _logger.LogInformation("Deleting membership with ID: {MembershipId}", membershipId);

                var membership = await _db.TblMemberships
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.MembershipId == membershipId);

                if (membership == null)
                {
                    _logger.LogWarning("Membership with ID: {MembershipId} not found.", membershipId);
                    return new Result<bool>
                    {
                        IsSuccess = false,
                        Message = "Membership not found."
                    };
                }

                membership.IsDeleted = true;
                membership.UpdatedAt = DateTime.UtcNow;
                membership.UpdatedBy = updatedByUserId;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Membership with ID: {MembershipId} deleted successfully.", membershipId);

                return new Result<bool>
                {
                    IsSuccess = true,
                    Message = "Membership deleted successfully.",
                    Data = true
                };
        }
            
            
    }
}