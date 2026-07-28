using FluentValidation;
using GMMS.Domain.Features.MemberShipPlan.Models;

namespace GMMS.Domain.Features.MemberShipPlan.Models
{
    public class MemberShipPlanListRequestValidator : AbstractValidator<MemberShipPlanlistRequestModel>
    {
        public MemberShipPlanListRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }

    public class CreateMemberShipPlanRequestValidator : AbstractValidator<CreateMemberShipPlanRequestModel>
    {
        public CreateMemberShipPlanRequestValidator()
        {
            RuleFor(x => x.PlanCode)
                .NotEmpty().WithMessage("Plan code is required.")
                .MaximumLength(50).WithMessage("Plan code must not exceed 50 characters.")
                .Matches("^[A-Z0-9-]+$").WithMessage("Plan code can only contain uppercase letters, numbers, and hyphens.");

            RuleFor(x => x.PlanName)
                .NotEmpty().WithMessage("Plan name is required.")
                .MaximumLength(100).WithMessage("Plan name must not exceed 100 characters.")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Plan name cannot be only whitespace.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Price cannot exceed 1,000,000.");

            RuleFor(x => x.DurationDays)
                .GreaterThan(0).WithMessage("Duration must be at least 1 day.")
                .LessThanOrEqualTo(3650).WithMessage("Duration cannot exceed 10 years (3650 days).");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }

    public class UpdateMemberShipPlanRequestValidator : AbstractValidator<UpdateMemberShipPlanRequestModel>
    {
        public UpdateMemberShipPlanRequestValidator()
        {
            RuleFor(x => x.MemberShipPlanId)
                .GreaterThan(0).WithMessage("Plan ID is required.");

            RuleFor(x => x.PlanCode)
                .NotEmpty().WithMessage("Plan code is required.")
                .MaximumLength(50).WithMessage("Plan code must not exceed 50 characters.")
                .Matches("^[A-Z0-9-]+$").WithMessage("Plan code can only contain uppercase letters, numbers, and hyphens.");

            RuleFor(x => x.PlanName)
                .NotEmpty().WithMessage("Plan name is required.")
                .MaximumLength(100).WithMessage("Plan name must not exceed 100 characters.")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Plan name cannot be only whitespace.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Price cannot exceed 1,000,000.");

            RuleFor(x => x.DurationDays)
                .GreaterThan(0).WithMessage("Duration must be at least 1 day.")
                .LessThanOrEqualTo(3650).WithMessage("Duration cannot exceed 10 years (3650 days).");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}