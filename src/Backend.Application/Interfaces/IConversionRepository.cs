using Backend.Application.Common;
using Backend.Domain.Entities;
namespace Backend.Application.Interfaces
{
    public interface IConversionRepository
    {
        Task AddAsync(ConversionHistory entity, CancellationToken ct = default);
        Task<PaginatedResult<ConversionHistory>> GetHistoryAsync(string userId, int page, int pageSize, CancellationToken ct = default);
    }
}