namespace GMMS.App.Services
{
    public class ApiEndpoints
    {
        #region Member Endpoints
        public const string MemberList = "api/Member";
        public const string MemberDetails = "api/Member/{id}";
        public const string CreateMember = "api/Member";
        public const string UpdateMember = "api/Member/{id}";
        public const string DeleteMember = "api/Member/{id}";
        #endregion
        #region Membership Endpoints
        public const string MembershipList = "api/MemberShip";
        public const string MembershipListAll = "api/MemberShip/all";
        public const string MembershipDetails = "api/MemberShip/{id}";
        public const string CreateMembership = "api/MemberShip";
        public const string UpdateMembership = "api/MemberShip/{id}";
        public const string DeleteMembership = "api/MemberShip/{id}";
        #endregion
        #region MembershipPlan Endpoints
        public const string MembershipPlanList = "api/MemberShipPlan";
        public const string MembershipPlanDetails = "api/MemberShipPlan/{id}";
        public const string CreateMembershipPlan = "api/MemberShipPlan";
        public const string UpdateMembershipPlan = "api/MemberShipPlan/{id}";
        public const string DeleteMembershipPlan = "api/MemberShipPlan/{id}";
        #endregion
        #region PaymentMethod Endpoints
        public const string PaymentMethodList = "api/PaymentMethod";
        public const string PaymentMethodDetails = "api/PaymentMethod/{id}";
        public const string CreatePaymentMethod = "api/PaymentMethod";
        public const string UpdatePaymentMethod = "api/PaymentMethod/{id}";
        public const string DeletePaymentMethod = "api/PaymentMethod/{id}";
        #endregion
        #region Payment Endpoints
        public const string PaymentList = "api/Payment";
        public const string PaymentDetails = "api/Payment/{id}";
        public const string CreatePayment = "api/Payment";
        #endregion
        #region Auth

        public const string Login = "api/Auth/login";
        public const string ChangePassword = "api/Auth/change-password";

        #endregion
    }
}
