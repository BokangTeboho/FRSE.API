using FastEndpoints;
using FE.Core.Entities;
using FE.Core.Interfaces;

namespace FE.Core.Features.WatchList.AddWatchlistEntry
{
    public class AddWatchlistEntryCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork)
        : ICommandHandler<AddWatchlistEntryCommand, AddWatchlistEntryResult>
    {
        public async Task<AddWatchlistEntryResult> ExecuteAsync(AddWatchlistEntryCommand command, CancellationToken ct)
        {
            var entry = new WatchlistEntry
            {
                Id = Guid.NewGuid(),
                EntityType = command.EntityType,
                EntityIdentifier = command.EntityIdentifier,
                RiskLevel = command.RiskLevel,
                Reason = command.Reason,
                IsManualEntry = true,
                ModifiedByIdentifier = command.ModifiedByIdentifier,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await watchlistService.Add(entry, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return new AddWatchlistEntryResult(
                entry.Id,
                entry.EntityType,
                entry.EntityIdentifier,
                entry.RiskLevel,
                entry.Reason,
                entry.AlertCount,
                entry.IsManualEntry,
                entry.ModifiedByIdentifier,
                entry.IsActive,
                entry.CreatedAt);
        }
    }
}
