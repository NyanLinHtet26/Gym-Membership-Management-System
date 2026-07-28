using FluentValidation;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.MemberShipPlan.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.MemberShipPlan
{
    public class MemberShipPlanService
    {
        private readonly AppDbContext _db;
        private readonly IValidator<MemberShipPlanlistRequestModel> _listValidator;
        private readonly IValidator<CreateMemberShipPlanRequestModel> _createValidator;
        private readonly IValidator<UpdateMemberShipPlanRequestModel> _updateValidator;
        private readonly ILogger<MemberShipPlanService> _logger;

        public MemberShipPlanService(
            AppDbContext db,
            IValidator<MemberShipPlanlistRequestModel> listValidator,
            IValidator<CreateMemberShipPlanRequestModel> createValidator,
            IValidator<UpdateMemberShipPlanRequestModel> updateValidator,
            ILogger<MemberShipPlanService> logger)
        {
            _db = db;
            _listValidator = listValidator;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<Result<MemberShipPlanListResponseModel>> GetList(MemberShipPlanlistRequestModel request)
        {
            _logger.LogInformation("Retrieving membership plan list with PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            var validationResult = await  _listValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid membership plan list request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MemberShipPlanListResponseModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            
                var query = _db.TblMembershipPlans
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.Trim().ToLower();
                    query = query.Where(x =>
                        x.PlanCode.ToLower().Contains(search) ||
                        x.PlanName.ToLower().Contains(search) ||
                        (x.Description != null && x.Description.ToLower().Contains(search)));
                }

                var  totalCount = await query.CountAsync();

                var plans = await query
                    .OrderByDescending(x => x.MembershipPlanId)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new MemberShipPlanModel
                    {
                        MemberShipPlanId = x.MembershipPlanId,

                        PlanCode = x.PlanCode,
                        PlanName = x.PlanName,
                        Price = x.Price,

                        DurationDays = x.DurationDays,

                        IsActive = x.IsActive,

                        CreatedAt = x.CreatedAt,

                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers
                        .Where(u => u.UserId == x.CreatedBy)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),

                        UpdatedAt = x.UpdatedAt,
                        UpdatedByUser = x.UpdatedBy.HasValue
                            ? x.UpdatedBy.Value + " - " + _db.TblUsers
                            .Where(u => u.UserId == x.UpdatedBy.Value)
                            .Select(u => u.UserName)
                            .FirstOrDefault()
                            : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} membership plans out of {TotalCount} total.", plans.Count, totalCount);

                return new Result<MemberShipPlanListResponseModel>
                {
                    IsSuccess = true,
                    Message = "Membership plans retrieved successfully.",
                    Data = new MemberShipPlanListResponseModel
                    {
                        TotalCount = totalCount,
                        MemberShipPlans = plans
                    }
                };
            
            
        }

        public async Task<Result<MembershipPlanDetailModel>> GetById(int membershipPlanId)
        {
            _logger.LogInformation("Retrieving membership plan with ID: {MembershipPlanId}", membershipPlanId);

                var plan = await _db.TblMembershipPlans
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.MembershipPlanId == membershipPlanId)
                    .Select(x => new MembershipPlanDetailModel
                    {
                        MemberShipPlanId = x.MembershipPlanId,
                        PlanCode = x.PlanCode,
                        PlanName = x.PlanName,
                        Price = x.Price,
                        DurationDays = x.DurationDays,
                        Description = x.Description,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt,
                        CreatedByUser = x.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == x.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                        UpdatedAt = x.UpdatedAt,
                        UpdatedByUser = x.UpdatedBy.HasValue
                            ? x.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == x.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                            : null
                    })
                    .FirstOrDefaultAsync();
                if (plan == null)
                {
                    _logger.LogWarning("Membership plan with ID: {MembershipPlanId} not found.", membershipPlanId);
                    return new Result<MembershipPlanDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership plan not found."
                    };
                }

                _logger.LogInformation("Membership plan with ID: {MembershipPlanId} retrieved successfully.", membershipPlanId);
                return new Result<MembershipPlanDetailModel>
                {
                    IsSuccess = true,
                    Message = "Membership plan retrieved successfully.",
                    Data = plan
                };
            
        }

        public async Task<Result<MembershipPlanDetailModel>> Create(CreateMemberShipPlanRequestModel request)
        {
            _logger.LogInformation("Creating membership plan with PlanCode: {PlanCode}, PlanName: {PlanName}", request.PlanCode, request.PlanName);

            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid membership plan creation request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MembershipPlanDetailModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            var planCode = request.PlanCode.Trim().ToUpperInvariant();
            var planName = request.PlanName.Trim();
            var description = request.Description?.Trim();

            var exists = await _db.TblMembershipPlans
                    .AnyAsync(x => !x.IsDeleted && x.PlanCode == planCode);
                    

                if (exists)
                {
                    _logger.LogWarning("Membership plan with PlanCode: {PlanCode} already exists.", planCode);
                    return new Result<MembershipPlanDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership plan already exists."
                    };
                }

                var plan =  new TblMembershipPlan
                {
                    PlanCode = planCode,
                    PlanName = planName,
                    Description = description,

                    Price = request.Price,
                    DurationDays = request.DurationDays,

                    IsActive = request.IsActive,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _db.TblMembershipPlans.AddAsync(plan);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Membership plan created successfully with PlanCode: {PlanCode}, MembershipPlanId: {MembershipPlanId}", planCode, plan.MembershipPlanId);

                return new Result<MembershipPlanDetailModel>
                {
                    IsSuccess = true,
                    Message = "Membership plan created successfully.",
                    Data = new MembershipPlanDetailModel
                    {
                        MemberShipPlanId = plan.MembershipPlanId,
                        PlanCode = request.PlanCode,
                        PlanName = plan.PlanName,
                        Price = plan.Price,
                        DurationDays = plan.DurationDays,
                        Description = plan.Description,
                        CreatedAt = plan.CreatedAt,
                        CreatedByUser = plan.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == plan.CreatedBy).Select(u => u.UserName).FirstOrDefault()
                    }
                };
            
           
        }

        public async Task<Result<MembershipPlanDetailModel>> Update(int id, UpdateMemberShipPlanRequestModel request)
        {
            _logger.LogInformation("Updating membership plan with ID: {MembershipPlanId}, PlanCode: {PlanCode}, PlanName: {PlanName}", id, request.PlanCode, request.PlanName);

            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid membership plan update request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MembershipPlanDetailModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            var planCode = request.PlanCode.Trim().ToUpperInvariant();
            var planName = request.PlanName.Trim();
            var description = request.Description?.Trim();

            var plan = await _db.TblMembershipPlans
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.MembershipPlanId == id);

                if (plan == null)
                {
                    _logger.LogWarning("Membership plan with ID: {MembershipPlanId} not found for update.", id);
                    return new Result<MembershipPlanDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership plan not found."
                    };
                }

                var existsplan = await _db.TblMembershipPlans
                    .AnyAsync(x => !x.IsDeleted && x.PlanCode == planCode && x.MembershipPlanId!= id);
                    
                if (existsplan)
                {
                    _logger.LogWarning("Membership plan with PlanCode: {PlanCode} already exists.", planCode);
                    return new Result<MembershipPlanDetailModel>
                    {
                        IsSuccess = false,
                        Message = "Membership plan already exists."
                    };
                }

                plan.PlanCode = request.PlanCode;
                plan.PlanName = request.PlanName;
                plan.Description = request.Description;
                plan.Price = request.Price;
                plan.DurationDays = request.DurationDays;
                plan.IsActive = request.IsActive;
                plan.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Membership plan with ID: {MembershipPlanId} updated successfully.", id);

                return new Result<MembershipPlanDetailModel>
                {
                    IsSuccess = true,
                    Message = "Membership plan updated successfully.",
                    Data = new MembershipPlanDetailModel
                    {
                        MemberShipPlanId = plan.MembershipPlanId,
                        PlanCode = plan.PlanCode,
                        PlanName = plan.PlanName,
                        Price = plan.Price,
                        DurationDays = plan.DurationDays,
                        Description = plan.Description,
                        IsActive = plan.IsActive,
                        CreatedAt = plan.CreatedAt,
                        CreatedByUser = plan.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == plan.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                        UpdatedAt = plan.UpdatedAt,
                        UpdatedByUser = plan.UpdatedBy.HasValue
                            ? plan.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == plan.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                            : null
                    }
                };
            
        }

        public async Task<Result<bool>> Delete(int membershipPlanId)
        {
            _logger.LogInformation("Deleting membership plan with ID: {MembershipPlanId}", membershipPlanId);

            var plan = await _db.TblMembershipPlans
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.MembershipPlanId == membershipPlanId);
                if (plan == null)
                {
                    _logger.LogWarning("Membership plan with ID: {MembershipPlanId} not found for deletion.", membershipPlanId);
                    return new Result<bool>
                    {
                        IsSuccess = false,
                        Message = "Membership plan not found."
                    };
                }
                plan.IsDeleted = true;
                plan.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Membership plan with ID: {MembershipPlanId} deleted successfully.", membershipPlanId);

                return new Result<bool>
                {
                    IsSuccess = true,
                    Message = "Membership plan deleted successfully.",
                    Data = true
                };
            
        }
    }
}