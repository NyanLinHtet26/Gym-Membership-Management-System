using System.Text.Json;
using System.Text.Json.Serialization;
using GMMS.Database.AppDbContextModels;

namespace GMMS.Domain.Features.AuditLog
{
    public class AuditLogService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly AppDbContext _db;

        public AuditLogService(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string tableName, string recordId, string action, int userId, object? oldValue = null, object? newValue = null)
        {
            var log = new TblAuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                Action = action,
                UserId = userId,
                OldValue = Serialize(oldValue),
                NewValue = Serialize(newValue),
                CreatedAt = DateTime.UtcNow
            };

            _db.TblAuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        private static string? Serialize(object? value)
            => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }
}
