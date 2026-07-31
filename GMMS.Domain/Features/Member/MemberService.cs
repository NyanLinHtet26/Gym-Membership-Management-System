using FluentValidation;
using GMMS.Database.AppDbContextModels;
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


        public MemberService(
            AppDbContext db,
            IValidator<CreateMemberRequestModel> createValidator,
            IValidator<UpdateMemberRequestModel> updateValidator,
            IValidator<MemberListRequestModel> listValidator,
            ILogger<MemberService> logger)
        {
            _db = db;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _listValidator = listValidator;
            _logger = logger;
        }
        
        public async Task <Result<MemberListResponseModel>> GetList(MemberListRequestModel request)
        {

            _logger.LogInformation("Retrieving member list with PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

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

                var members = await query
                    .OrderByDescending(x => x.MemberId)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new MemberModel
                    {
                        MemberId = x.MemberId,
                        MemberCode = x.MemberCode,
                        Name = x.Name,
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
        public async Task <Result<MemberModel>> GetById(int memberId)
        {
            _logger.LogInformation("Retrieving member with ID: {MemberId}", memberId);
            
                var member = await _db.TblMembers
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.MemberId == memberId)
                    .Select(x => new MemberModel
                    {
                        MemberId = x.MemberId,
                        MemberCode = x.MemberCode,
                        Name = x.Name,
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
                    .FirstOrDefaultAsync();

                if (member == null)
                {
                    _logger.LogWarning("Member with ID: {MemberId} not found.", memberId);
                    return new Result<MemberModel>
                    {
                        IsSuccess = false,
                        Message = "Member not found."
                    };
                }

                _logger.LogInformation("Member with ID: {MemberId} retrieved successfully.", memberId);
                return new Result<MemberModel>
                {
                    IsSuccess = true,
                    Message = "Member retrieved successfully.",
                    Data = member
                };
            }
        public async Task <Result<MemberModel>> Create(int createdByUserId, CreateMemberRequestModel request)
        {
            _logger.LogInformation("Creating a new member with MemberCode: {MemberCode}, Name: {Name}", request.MemberCode, request.Name);
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

            
                request.MemberCode = request.MemberCode.Trim().ToUpperInvariant();
                request.Name = request.Name.Trim();

                var exists = await _db.TblMembers
                    .AnyAsync(x => !x.IsDeleted && x.MemberCode.ToUpper() == request.MemberCode);

                if (exists)
                {
                    _logger.LogWarning("Member with MemberCode: {MemberCode} already exists.", request.MemberCode);
                    return new Result<MemberModel>
                    {
                        IsSuccess = false,
                        Message = "Member already exists."
                    };
                }
                var member =  new TblMember
                {
                    MemberCode = request.MemberCode,
                    Name = request.Name,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };


                await _db.TblMembers.AddAsync(member);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Member created successfully with MemberId: {MemberId} and MemberCode: {MemberCode}", member.MemberId, member.MemberCode);
                return new Result<MemberModel>
                {
                    IsSuccess = true,
                    Message = "Member created successfully.",
                    Data = new MemberModel
                    {
                        MemberId = member.MemberId,
                        MemberCode = member.MemberCode,
                        Name = member.Name,
                        CreatedAt = member.CreatedAt,
                        UpdatedAt = member.UpdatedAt
                    }
                    
                };
            
        }
        public async Task  <Result<MemberModel>>   Update(int id, int updatedByUserId, UpdateMemberRequestModel request)
        {
            _logger.LogInformation("Updating member with ID: {MemberId}, MemberCode: {MemberCode}, Name: {Name}", id, request.MemberCode, request.Name);
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

           
                var member = await _db.TblMembers
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.MemberId == request.MemberId);
                if (member == null)
                {
                    _logger.LogWarning("Member with ID: {MemberId} not found.", request.MemberId);
                    return new Result<MemberModel>
                    {
                        IsSuccess = false,
                        Message = "Member not found."
                    };
                }

                request.MemberCode = request.MemberCode.Trim().ToUpperInvariant();
                request.Name = request.Name.Trim();

                var exists = await _db.TblMembers
                    .AnyAsync(x => !x.IsDeleted && x.MemberCode.ToUpper() == request.MemberCode && x.MemberId != request.MemberId);
                if (exists)
                {
                    _logger.LogWarning("Member with MemberCode: {MemberCode} already exists.", request.MemberCode);
                    return new Result<MemberModel>
                    {
                        IsSuccess = false,
                        Message = "Member already exists."
                    };
                }

                member.MemberCode = request.MemberCode;
                member.Name = request.Name;
                member.UpdatedAt = DateTime.UtcNow;
                member.UpdatedBy = updatedByUserId;
              
               await _db.SaveChangesAsync();
                _logger.LogInformation("Member with ID: {MemberId} updated successfully.", request.MemberId);
                return new Result<MemberModel>
                {
                    IsSuccess = true,
                    Message = "Member updated successfully.",
                    Data = new MemberModel
                    {
                        MemberId = member.MemberId,
                        MemberCode = member.MemberCode,
                        Name = member.Name,
                        CreatedAt = member.CreatedAt,
                        CreatedByUser = member.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == member.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                        UpdatedAt = member.UpdatedAt,
                        UpdatedByUser = member.UpdatedBy.HasValue
                            ? member.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == member.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                            : null
                    }
                };
            }
        public async Task <Result<bool>> Delete(int memberId, int updatedByUserId)
        {
            _logger.LogInformation("Deleting member with ID: {MemberId}", memberId);
           
                var member = await _db.TblMembers
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.MemberId == memberId);
                if (member == null)
                {
                    _logger.LogWarning("Member with ID: {MemberId} not found.", memberId);
                    return new Result<bool>
                    {
                        IsSuccess = false,
                        Message = "Member not found."
                    };
                }
                member.IsDeleted = true;
                member.UpdatedAt = DateTime.UtcNow;
                member.UpdatedBy = updatedByUserId;
                await _db.SaveChangesAsync();
                _logger.LogInformation("Member with ID: {MemberId} deleted successfully.", memberId);
                return new Result<bool>
                {
                    IsSuccess = true,
                    Message = "Member deleted successfully.",
                    Data = true
                };
        }
           
    }
}
