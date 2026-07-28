using FluentValidation;
using GMMS.Domain.Features.Payment.Models;

namespace GMMS.Domain.Features.Payment.Models
{
    public class PaymentListRequestValidator : AbstractValidator<PaymentListRequestModel>
    {
        public PaymentListRequestValidator()
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

    public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequestModel>
    {
        public CreatePaymentRequestValidator()
        {
            RuleFor(x => x.MembershipId)
                .GreaterThan(0).WithMessage("Membership ID is required.");

            RuleFor(x => x.PaymentMethodId)
                .GreaterThan(0).WithMessage("Payment method ID is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed 1,000,000.");

            RuleFor(x => x.Sspath)
                .MaximumLength(500).WithMessage("Receipt path must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Sspath));
        }
    }
}