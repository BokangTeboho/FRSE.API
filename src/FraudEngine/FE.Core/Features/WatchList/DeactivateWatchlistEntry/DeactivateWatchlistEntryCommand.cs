using System.Text.Json.Serialization;
using FastEndpoints;

namespace FE.Core.Features.WatchList.DeactivateWatchlistEntry
{
    public record DeactivateWatchlistEntryCommand : ICommand<DeactivateWatchlistEntryResult?>
    {
        public Guid Id { get; init; }

        [JsonIgnore]
        public string ModifiedByIdentifier { get; set; } = string.Empty;
    }

    public record DeactivateWatchlistEntryResult(
        Guid Id,
        bool IsActive,
        string ModifiedByIdentifier,
        DateTimeOffset DeactivatedAt);
}
