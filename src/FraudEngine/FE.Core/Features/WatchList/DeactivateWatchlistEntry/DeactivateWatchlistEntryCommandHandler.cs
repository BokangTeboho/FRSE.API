using FastEndpoints;
using FE.Core.Interfaces;

namespace FE.Core.Features.WatchList.DeactivateWatchlistEntry
{
    public class DeactivateWatchlistEntryCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork)
        : ICommandHandler<DeactivateWatchlistEntryCommand, DeactivateWatchlistEntryResult?>
    {
        public async Task<DeactivateWatchlistEntryResult?> ExecuteAsync(
            DeactivateWatchlistEntryCommand command, CancellationToken ct)
        {
            var entry = await watchlistService.Deactivate(command.Id, ct);

            if (entry is null)
                return null;

            await unitOfWork.SaveChangesAsync(ct);

            return new DeactivateWatchlistEntryResult(
                entry.Id,
                entry.IsActive,
                DateTimeOffset.UtcNow);
        }
    }
}
