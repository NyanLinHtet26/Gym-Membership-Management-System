using FluentValidation;
using GMMS.Domain.Features.PaymentMethod.Models;

namespace GMMS.Domain.Features.PaymentMethod.Models
{
    public class PaymentMethodListRequestValidator : AbstractValidator<PaymentMethodListRequestModel>
    {
        public PaymentMethodListRequestValidator()
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

    public class PaymentMethodCreateRequestValidator : AbstractValidator<PaymentMethodCreateRequestModel>
    {
        public PaymentMethodCreateRequestValidator()
        {
            RuleFor(x => x.PaymentMethodCode)
                .NotEmpty().WithMessage("Payment method code is required.")
                .MaximumLength(50).WithMessage("Payment method code must not exceed 50 characters.")
                .Matches("^[A-Z0-9-]+$").WithMessage("Payment method code can only contain uppercase letters, numbers, and hyphens.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Name cannot be only whitespace.");
        }
    }

    public class PaymentMethodUpdateRequestValidator : AbstractValidator<PaymentMethodUpdateRequestModel>
    {
        public PaymentMethodUpdateRequestValidator()
        {
            RuleFor(x => x.PaymentMethodId)
                .GreaterThan(0).WithMessage("Payment method ID is required.");

            RuleFor(x => x.PaymentMethodCode)
                .NotEmpty().WithMessage("Payment method code is required.")
                .MaximumLength(50).WithMessage("Payment method code must not exceed 50 characters.")
                .Matches("^[A-Z0-9-]+$").WithMessage("Payment method code can only contain uppercase letters, numbers, and hyphens.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Name cannot be only whitespace.");
        }
    }
}