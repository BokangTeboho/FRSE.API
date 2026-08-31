using FastEndpoints;

namespace FE.Core.Features.WatchList.DeactivateWatchlistEntry
{
    public record DeactivateWatchlistEntryCommand : ICommand<DeactivateWatchlistEntryResult?>
    {
        public Guid Id { get; init; }
    }

    public record DeactivateWatchlistEntryResult(
        Guid Id,
        bool IsActive,
        DateTimeOffset DeactivatedAt);
}
