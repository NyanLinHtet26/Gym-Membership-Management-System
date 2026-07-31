using FluentValidation;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.User.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GMMS.Domain.Features.User;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly IValidator<UserListRequestModel> _listValidator;
    private readonly IValidator<CreateUserRequestModel> _createValidator;
    private readonly IValidator<UpdateUserRequestModel> _updateValidator;
    private readonly IValidator<ResetPasswordRequestModel> _resetPasswordValidator;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AppDbContext db,
        IValidator<UserListRequestModel> listValidator,
        IValidator<CreateUserRequestModel> createValidator,
        IValidator<UpdateUserRequestModel> updateValidator,
        IValidator<ResetPasswordRequestModel> resetPasswordValidator,
        ILogger<UserService> logger)
    {
        _db = db;
        _listValidator = listValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _logger = logger;
    }

    public async Task< Result<UserListResponseModel>> GetList(UserListRequestModel request)
    {
        _logger.LogInformation("Retrieving user list. PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

        var validationResult = await _listValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid user list request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return new Result<UserListResponseModel>
            {
                IsSuccess = false,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        
            var query = _db.TblUsers
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                query = query.Where(x => x.UserName.ToLower().Contains(search) || x.Role.ToLower().Contains(search));
            }

            var totalCount = await  query.CountAsync();

            var users = await query
                .OrderByDescending(x => x.UserId)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new UserModel
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    MustChangePassword = x.MustChangePassword,
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

            _logger.LogInformation("Users retrieved successfully. TotalCount={TotalCount}, PageUsers={PageUsers}", totalCount, users.Count);
            return new Result<UserListResponseModel>
            {
                IsSuccess = true,
                Message = "Users retrieved successfully.",
                Data = new UserListResponseModel
                {
                    TotalCount = totalCount,
                    Users = users
                }
            };
        
       
    }

    public async Task <Result<UserModel>> GetById(int userId)
    {
        _logger.LogInformation("Retrieving user with ID: {UserId}", userId);

            var user = await _db.TblUsers
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.UserId == userId)
                .Select(x => new UserModel
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    MustChangePassword = x.MustChangePassword,
                    CreatedAt = x.CreatedAt,
                    CreatedByUser = x.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == x.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                    UpdatedAt = x.UpdatedAt,
                    UpdatedByUser = x.UpdatedBy.HasValue
                        ? x.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == x.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                        : null
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found.", userId);
                return new Result<UserModel>
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            _logger.LogInformation("User with ID: {UserId} retrieved successfully.", userId);
            return new Result<UserModel>
            {
                IsSuccess = true,
                Message = "User retrieved successfully.",
                Data = user
            };
        
    }

    public async Task<Result<UserModel>> Create(int createdByUserId, CreateUserRequestModel request)
    {
        _logger.LogInformation("Creating user with UserName: {UserName}, Role: {Role}", request.UserName, request.Role);

        var validationResult =await  _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid user creation request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return new Result<UserModel>
            {
                IsSuccess = false,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

       
            request.UserName = request.UserName
                .Trim()
                .ToLowerInvariant();

            var exists = await _db.TblUsers
                .AnyAsync(x => !x.IsDeleted && x.UserName == request.UserName);

            if (exists)
            {
                _logger.LogWarning("Username already exists: {UserName}", request.UserName);
                return new Result<UserModel>
                {
                    IsSuccess = false,
                    Message = "Username already exists."
                };
            }

            var user = new TblUser
            {
                UserName = request.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = request.IsActive,
                MustChangePassword = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId
            };

            await _db.TblUsers.AddAsync(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User created successfully. UserId={UserId}, UserName={UserName}", user.UserId, request.UserName);

            return new Result<UserModel>
            {
                IsSuccess = true,
                Message = "User created successfully.",
                Data = new UserModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    MustChangePassword = user.MustChangePassword,
                    CreatedAt = user.CreatedAt,
                    CreatedByUser = user.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == user.CreatedBy).Select(u => u.UserName).FirstOrDefault()
                }
            };
        
        
    }

    public async Task<Result<UserModel>> Update(int id, int updatedByUserId, UpdateUserRequestModel request)
    {
        _logger.LogInformation("Updating user with ID: {UserId}, UserName: {UserName}, Role: {Role}", id, request.UserName, request.Role);

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid user update request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return new Result<UserModel>
            {
                IsSuccess = false,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

       
            var user =await _db.TblUsers
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == request.UserId);

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found for update.", request.UserId);
                return new Result<UserModel>
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            request.UserName = request.UserName.Trim().ToLowerInvariant();

            var exists = await _db.TblUsers
                .AnyAsync(x => !x.IsDeleted && x.UserName == request.UserName && x.UserId != request.UserId);

            if (exists)
            {
                _logger.LogWarning("Username already exists: {UserName}", request.UserName);
                return new Result<UserModel>
                {
                    IsSuccess = false,
                    Message = "Username already exists."
                };
            }

            user.UserName = request.UserName;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedByUserId;
            await _db.SaveChangesAsync();

            _logger.LogInformation("User with ID: {UserId} updated successfully.", request.UserId);

            return new Result<UserModel>
            {
                IsSuccess = true,
                Message = "User updated successfully.",
                Data = new UserModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    MustChangePassword = user.MustChangePassword,
                    CreatedAt = user.CreatedAt,
                    CreatedByUser = user.CreatedBy + " - " + _db.TblUsers.Where(u => u.UserId == user.CreatedBy).Select(u => u.UserName).FirstOrDefault(),
                    UpdatedAt = user.UpdatedAt,
                    UpdatedByUser = user.UpdatedBy.HasValue
                        ? user.UpdatedBy.Value + " - " + _db.TblUsers.Where(u => u.UserId == user.UpdatedBy.Value).Select(u => u.UserName).FirstOrDefault()
                        : null
                }
            };
        
    }

    public async Task <Result<bool> >ResetPassword(int updatedByUserId, ResetPasswordRequestModel request)
    {
        _logger.LogInformation("Resetting password for UserId: {UserId}", request.UserId);

        var validationResult =await _resetPasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid password reset request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return new Result<bool>
            {
                IsSuccess = false,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        
            var user = await _db.TblUsers
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == request.UserId);

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found for password reset.", request.UserId);
                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.MustChangePassword = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedByUserId;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Password reset successfully for UserId: {UserId}", request.UserId);

            return new Result<bool>
            {
                IsSuccess = true,
                Message = "Password reset successfully. User must change password on next login.",
                Data = true
            };
        
    }

    public async Task <Result<bool>> Delete(int userId, int updatedByUserId)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", userId);

            var user = await _db.TblUsers
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found for deletion.", userId);
                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedByUserId;
            await _db.SaveChangesAsync();

            _logger.LogInformation("User with ID: {UserId} deleted successfully.", userId);

            return new Result<bool>
            {
                IsSuccess = true,
                Message = "User deleted successfully.",
                Data = true
            };
        
       
    }
}
