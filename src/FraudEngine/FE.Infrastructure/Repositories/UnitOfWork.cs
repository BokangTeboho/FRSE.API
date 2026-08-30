using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using FE.Infrastructure.Resilience;
using Polly;
using Polly.Registry;

namespace FE.Infrastructure.Repositories
{
    public class UnitOfWork(FraudEngineDbContext db, ResiliencePipelineProvider<string> pipelineProvider) : IUnitOfWork
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline(ResilienceExtensions.DatabasePipelineName);

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _pipeline.ExecuteAsync(async token => await db.SaveChangesAsync(token), ct);
        }
    }
}