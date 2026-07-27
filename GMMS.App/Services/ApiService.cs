using Azure;

namespace GMMS.App.Services
{
    public class ApiService
    {
        private readonly HttpClientService _http;

        public ApiService(HttpClientService http)
        {
            _http = http;
        }

        #region Member
        public async Task<TResponse?> GetMemberListAsync<TResponse>(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var endpoint = $"{ApiEndpoints.MemberList}?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                endpoint += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> GetMemberDetailsAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.MemberDetails.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> CreateMemberAsync<TRequest, TResponse>(TRequest request)
        {
            return await ExecuteAsync<TResponse>(() => _http.PostAsync<TRequest, TResponse>(ApiEndpoints.CreateMember, request));
        }

        public async Task<TResponse?> UpdateMemberAsync<TRequest, TResponse>(int id, TRequest request)
        {
            var endpoint = ApiEndpoints.UpdateMember.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.PutAsync<TRequest, TResponse>(endpoint, request));
        }

        public async Task<TResponse?> DeleteMemberAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.DeleteMember.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.DeleteAsync<TResponse>(endpoint));
        }
        #endregion

        #region Membership
        public async Task<TResponse?> GetMembershipListAsync<TResponse>(int memberId, int pageNumber = 1, int pageSize = 10)
        {
            var endpoint = $"{ApiEndpoints.MembershipList}?pageNumber={pageNumber}&pageSize={pageSize}&memberId={memberId}";
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> GetAllMembershipsAsync<TResponse>(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? status = null)
        {
            var endpoint = $"{ApiEndpoints.MembershipListAll}?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                endpoint += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (!string.IsNullOrWhiteSpace(status))
                endpoint += $"&status={Uri.EscapeDataString(status)}";
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> GetMembershipDetailsAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.MembershipDetails.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> CreateMembershipAsync<TRequest, TResponse>(TRequest request)
        {
            return await ExecuteAsync<TResponse>(() => _http.PostAsync<TRequest, TResponse>(ApiEndpoints.CreateMembership, request));
        }

        public async Task<TResponse?> UpdateMembershipAsync<TRequest, TResponse>(int id, TRequest request)
        {
            var endpoint = ApiEndpoints.UpdateMembership.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.PutAsync<TRequest, TResponse>(endpoint, request));
        }

        public async Task<TResponse?> DeleteMembershipAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.DeleteMembership.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.DeleteAsync<TResponse>(endpoint));
        }
        #endregion

        #region MembershipPlan
        public async Task<TResponse?> GetMembershipPlanListAsync<TResponse>(int pageNumber = 1, int pageSize = 10)
        {
            var endpoint = $"{ApiEndpoints.MembershipPlanList}?pageNumber={pageNumber}&pageSize={pageSize}";
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> GetMembershipPlanDetailsAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.MembershipPlanDetails.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> CreateMembershipPlanAsync<TRequest, TResponse>(TRequest request)
        {
            return await ExecuteAsync<TResponse>(() => _http.PostAsync<TRequest, TResponse>(ApiEndpoints.CreateMembershipPlan, request));
        }

        public async Task<TResponse?> UpdateMembershipPlanAsync<TRequest, TResponse>(int id, TRequest request)
        {
            var endpoint = ApiEndpoints.UpdateMembershipPlan.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.PutAsync<TRequest, TResponse>(endpoint, request));
        }

        public async Task<TResponse?> DeleteMembershipPlanAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.DeleteMembershipPlan.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.DeleteAsync<TResponse>(endpoint));
        }
        #endregion

        #region PaymentMethod
        public async Task<TResponse?> GetPaymentMethodListAsync<TResponse>(int pageNumber = 1, int pageSize = 10)
        {
            var endpoint = $"{ApiEndpoints.PaymentMethodList}?pageNumber={pageNumber}&pageSize={pageSize}";
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> GetPaymentMethodDetailsAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.PaymentMethodDetails.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> CreatePaymentMethodAsync<TRequest, TResponse>(TRequest request)
        {
            return await ExecuteAsync<TResponse>(() => _http.PostAsync<TRequest, TResponse>(ApiEndpoints.CreatePaymentMethod, request));
        }

        public async Task<TResponse?> UpdatePaymentMethodAsync<TRequest, TResponse>(int id, TRequest request)
        {
            var endpoint = ApiEndpoints.UpdatePaymentMethod.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.PutAsync<TRequest, TResponse>(endpoint, request));
        }

        public async Task<TResponse?> DeletePaymentMethodAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.DeletePaymentMethod.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.DeleteAsync<TResponse>(endpoint));
        }
        #endregion

        #region Payment
        public async Task<TResponse?> GetPaymentListAsync<TResponse>(int pageNumber = 1, int pageSize = 10)
        {
            var endpoint = $"{ApiEndpoints.PaymentList}?pageNumber={pageNumber}&pageSize={pageSize}";
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> GetPaymentDetailsAsync<TResponse>(int id)
        {
            var endpoint = ApiEndpoints.PaymentDetails.Replace("{id}", id.ToString());
            return await ExecuteAsync<TResponse>(() => _http.GetAsync<TResponse>(endpoint));
        }

        public async Task<TResponse?> CreatePaymentAsync<TRequest, TResponse>(TRequest request)
        {
            return await ExecuteAsync<TResponse>(() => _http.PostAsync<TRequest, TResponse>(ApiEndpoints.CreatePayment, request));
        }
        #endregion

        #region Auth

        public async Task<TResponse?> LoginAsync<TRequest, TResponse> (TRequest request)
        {
            return await ExecuteAsync<TResponse>(() => _http.PostAsync<TRequest, TResponse>(ApiEndpoints.Login, request));
        }

        public async Task<TResponse?> ChangePasswordAsync<TRequest, TResponse>(TRequest request)
        {
            return await ExecuteAsync<TResponse>(() => _http.PostAsync<TRequest, TResponse>(ApiEndpoints.ChangePassword,request));
        }
        #endregion


        private async Task<TResponse?> ExecuteAsync<TResponse>(Func<Task<TResponse?>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                return HandleError<TResponse>(ex);
            }
        }

        private TResponse? HandleError<TResponse>(Exception ex)
        {
            var message = ExtractErrorMessage(ex.Message);
            // Return a default Result<T> with error if TResponse is Result<T>
            // For simplicity, we'll re-throw with clean message
            throw new Exception(message);
        }

        private string ExtractErrorMessage(string rawMessage)
        {
            // rawMessage format: "API error (BadRequest): {\"isSuccess\":false,\"message\":\"Member code can only contain...\"}"
            var prefix = "API error (";
            var startIdx = rawMessage.IndexOf(prefix);
            if (startIdx < 0) return rawMessage;

            var jsonStart = rawMessage.IndexOf('{', startIdx);
            if (jsonStart < 0) return rawMessage;

            var json = rawMessage.Substring(jsonStart);
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgElement))
                {
                    return msgElement.GetString() ?? rawMessage;
                }
            }
            catch { }
            return rawMessage;
        }
    }
}