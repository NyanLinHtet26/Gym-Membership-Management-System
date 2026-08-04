namespace GMMS.Mobile.Models;

/// <summary>
/// Mirrors the API's Result{T} envelope: { isSuccess, message, data }.
/// </summary>
public sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
