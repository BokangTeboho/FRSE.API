using FastEndpoints;
using FE.Core.Features.Transaction.ScanTransaction;

namespace FE.API.Endpoints.Transaction
{
    public class ScanTransactionEndpoint : Endpoint<ScanTransactionCommand, ScanTransactionResult>
    {
        public override void Configure()
        {
            Post("/transaction/scan");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ScanTransactionCommand req, CancellationToken ct)
        {
            var result = await req.ExecuteAsync(ct);

            await Send.OkAsync(result, ct); // will fix
        }
    }
}
