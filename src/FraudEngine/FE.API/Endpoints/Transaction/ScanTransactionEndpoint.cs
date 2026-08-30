using FastEndpoints;
using FE.Core.Features.Transaction.ScanTransaction;

namespace FE.API.Endpoints.Transaction
{
    public class ScanTransactionEndpoint : Endpoint<ScanTransactionCommand, ScanTransactionResult>
    {
        private readonly ILogger<ScanTransactionEndpoint> _logger;

        public ScanTransactionEndpoint(ILogger<ScanTransactionEndpoint> logger)
        {
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/transaction/scan");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ScanTransactionCommand req, CancellationToken ct)
        {
            _logger.LogInformation(
                "Scanning transaction {ReferenceId} for account {AccountNumber}, amount {Amount} {Currency}",
                req.ReferenceId, req.AccountNumber, req.Amount, req.Currency);

            var result = await req.ExecuteAsync(ct);

            _logger.LogInformation(
                "Transaction scan completed for {ReferenceId}, {RuleCount} rule(s) triggered",
                req.ReferenceId, result.TriggeredRules.Count);

            await Send.OkAsync(result, ct);
        }
    }
}
