using Backend.Application.Common;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Backend.Infrastructure.Repositories
{
    public class ConversionRepository : IConversionRepository
    {
        private readonly AppDbContext _ctx;
        public ConversionRepository(AppDbContext ctx) => _ctx = ctx;


        public async Task AddAsync(ConversionHistory entity, CancellationToken ct = default)
        {
            _ctx.ConversionHistories.Add(entity);
            await _ctx.SaveChangesAsync(ct);
        }


        public async Task<PaginatedResult<ConversionHistory>> GetHistoryAsync(string userId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _ctx.ConversionHistories.Where(x => x.UserId == userId).OrderByDescending(x => x.ConversionDate);
            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);


            return new PaginatedResult<ConversionHistory>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}