using FluentValidation;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.AuditLog;
using GMMS.Domain.Features.Member.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.Member
{
    public class MemberService
    {
        private readonly AppDbContext _db;
        private readonly IValidator<CreateMemberRequestModel> _createValidator;
        private readonly IValidator<UpdateMemberRequestModel> _updateValidator;
        private readonly IValidator<MemberListRequestModel> _listValidator;
        private readonly ILogger<MemberService> _logger;
        private readonly AuditLogService _auditLog;

        public MemberService(
            AppDbContext db,
            IValidator<CreateMemberRequestModel> createValidator,
            IValidator<UpdateMemberRequestModel> updateValidator,
            IValidator<MemberListRequestModel> listValidator,
            ILogger<MemberService> logger,
            AuditLogService auditLog)
        {
            _db = db;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _listValidator = listValidator;
            _logger = logger;
            _auditLog = auditLog;
        }

        public async Task<Result<MemberListResponseModel>> GetList(MemberListRequestModel request)
        {
            _logger.LogInformation("Retrieving member list with PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            #region Check: Request is valid (400)
            var validationResult = await _listValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid member list request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MemberListResponseModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }
            #endregion

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.PageSize <= 0 || request.PageSize > 100)
                request.PageSize = 10;

            var query = _db.TblMembers
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim();

                query = query.Where(x =>
                    x.MemberCode.Contains(search) ||
                    x.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var members = await ProjectMembers(query
                .OrderByDescending(x => x.MemberId)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize))
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} members out of {TotalCount} total members.", members.Count, totalCount);

            return new Result<MemberListResponseModel>
            {
                IsSuccess = true,
                Message = "Members retrieved successfully.",
                Data = new MemberListResponseModel
                {
                    TotalCount = totalCount,
                    Members = members
                }
            };
        }

        public async Task<Result<MemberModel>> GetById(int memberId)
        {
            _logger.LogInformation("Retrieving member with ID: {MemberId}", memberId);

            var member = await ProjectMembers(_db.TblMembers
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.MemberId == memberId))
                .FirstOrDefaultAsync();

            #region Check: Member exists (404)
            if (member == null)
            {
                _logger.LogWarning("Member with ID: {MemberId} not found.", memberId);
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = "Member not found.",
                    StatusCode = 404
                };
            }
            #endregion

            _logger.LogInformation("Member with ID: {MemberId} retrieved successfully.", memberId);

            return new Result<MemberModel>
            {
                IsSuccess = true,
                Message = "Member retrieved successfully.",
                Data = member
            };
        }

        public async Task<Result<MemberModel>> Create(int createdByUserId, CreateMemberRequestModel request)
        {
            _logger.LogInformation("Creating a new member with MemberCode: {MemberCode}, Name: {Name}", request.MemberCode, request.Name);

            #region Check: Request is valid (400)
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid member creation request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }
            #endregion

            request.MemberCode = request.MemberCode.Trim().ToUpperInvariant();
            request.Name = request.Name.Trim();

            #region Check: MemberCode is unique (409)
            var exists = await _db.TblMembers
                .AnyAsync(x => !x.IsDeleted && x.MemberCode == request.MemberCode);

            if (exists)
            {
                _logger.LogWarning("Member with MemberCode: {MemberCode} already exists.", request.MemberCode);
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = "Member already exists.",
                    StatusCode = 409
                };
            }
            #endregion

            var member = new TblMember
            {
                MemberCode = request.MemberCode,
                Name = request.Name,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId
            };

            #region Handle: Unique-constraint race (409)
            try
            {
                await _db.TblMembers.AddAsync(member);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Concurrent duplicate MemberCode: {MemberCode}", request.MemberCode);
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = "Member already exists.",
                    StatusCode = 409
                };
            }
            #endregion

            _logger.LogInformation("Member created successfully with MemberId: {MemberId} and MemberCode: {MemberCode}", member.MemberId, member.MemberCode);

            await _auditLog.LogAsync("Tbl_Member", member.MemberId.ToString(), "Create", createdByUserId,
                newValue: new { member.MemberCode, member.Name });

            var created = await ProjectMembers(_db.TblMembers
                .AsNoTracking()
                .Where(x => x.MemberId == member.MemberId))
                .FirstOrDefaultAsync();

            return new Result<MemberModel>
            {
                IsSuccess = true,
                Message = "Member created successfully.",
                Data = created
            };
        }

        public async Task<Result<MemberModel>> Update(int id, int updatedByUserId, UpdateMemberRequestModel request)
        {
            _logger.LogInformation("Updating member with ID: {MemberId}, MemberCode: {MemberCode}, Name: {Name}", id, request.MemberCode, request.Name);

            #region Check: Request is valid (400)
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid member update request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }
            #endregion

            var member = await _db.TblMembers
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.MemberId == id);

            #region Check: Member exists (404)
            if (member == null)
            {
                _logger.LogWarning("Member with ID: {MemberId} not found.", id);
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = "Member not found.",
                    StatusCode = 404
                };
            }
            #endregion

            request.MemberCode = request.MemberCode.Trim().ToUpperInvariant();
            request.Name = request.Name.Trim();

            #region Check: MemberCode is unique (409)
            var exists = await _db.TblMembers
                .AnyAsync(x => !x.IsDeleted && x.MemberCode == request.MemberCode && x.MemberId != id);
            if (exists)
            {
                _logger.LogWarning("Member with MemberCode: {MemberCode} already exists.", request.MemberCode);
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = "Member already exists.",
                    StatusCode = 409
                };
            }
            #endregion

            var oldValue = new { member.MemberCode, member.Name };

            member.MemberCode = request.MemberCode;
            member.Name = request.Name;
            member.UpdatedAt = DateTime.UtcNow;
            member.UpdatedBy = updatedByUserId;

            #region Handle: Unique-constraint race (409)
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Concurrent duplicate MemberCode: {MemberCode}", request.MemberCode);
                return new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = "Member already exists.",
                    StatusCode = 409
                };
            }
            #endregion

            _logger.LogInformation("Member with ID: {MemberId} updated successfully.", id);

            await _auditLog.LogAsync("Tbl_Member", id.ToString(), "Update", updatedByUserId,
                oldValue: oldValue,
                newValue: new { member.MemberCode, member.Name });

            var updated = await ProjectMembers(_db.TblMembers
                .AsNoTracking()
                .Where(x => x.MemberId == id))
                .FirstOrDefaultAsync();

            return new Result<MemberModel>
            {
                IsSuccess = true,
                Message = "Member updated successfully.",
                Data = updated
            };
        }

        public async Task<Result<bool>> Delete(int memberId, int updatedByUserId)
        {
            _logger.LogInformation("Deleting member with ID: {MemberId}", memberId);

            var member = await _db.TblMembers
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.MemberId == memberId);

            #region Check: Member exists (404)
            if (member == null)
            {
                _logger.LogWarning("Member with ID: {MemberId} not found.", memberId);
                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = "Member not found.",
                    StatusCode = 404
                };
            }
            #endregion

            #region Check: No active memberships (409)
            var hasActiveMemberships = await _db.TblMemberships
                .AnyAsync(x => x.MemberId == memberId && !x.IsDeleted);
            if (hasActiveMemberships)
            {
                _logger.LogWarning("Member with ID: {MemberId} has active memberships and cannot be deleted.", memberId);
                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = "Member has active memberships. Remove or deactivate them before deleting this member.",
                    StatusCode = 409
                };
            }
            #endregion

            member.IsDeleted = true;
            member.UpdatedAt = DateTime.UtcNow;
            member.UpdatedBy = updatedByUserId;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Member with ID: {MemberId} deleted successfully.", memberId);

            await _auditLog.LogAsync("Tbl_Member", memberId.ToString(), "Delete", updatedByUserId,
                oldValue: new { member.MemberCode, member.Name });

            return new Result<bool>
            {
                IsSuccess = true,
                Message = "Member deleted successfully.",
                Data = true
            };
        }

        #region Private: Helpers
        private IQueryable<MemberModel> ProjectMembers(IQueryable<TblMember> source)
        {
            return source.Select(x => new MemberModel
            {
                MemberId = x.MemberId,
                MemberCode = x.MemberCode,
                Name = x.Name,
                CreatedAt = x.CreatedAt,
                CreatedByUser = x.CreatedBy + " - " + (_db.TblUsers
                    .Where(u => u.UserId == x.CreatedBy)
                    .Select(u => u.UserName)
                    .FirstOrDefault() ?? "Deleted"),
                UpdatedAt = x.UpdatedAt,
                UpdatedByUser = x.UpdatedBy.HasValue
                    ? x.UpdatedBy.Value + " - " + (_db.TblUsers
                        .Where(u => u.UserId == x.UpdatedBy.Value)
                        .Select(u => u.UserName)
                        .FirstOrDefault() ?? "Deleted")
                    : null
            });
        }
        #endregion
    }
}
