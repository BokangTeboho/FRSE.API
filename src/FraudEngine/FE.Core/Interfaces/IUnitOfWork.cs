namespace FE.Core.Interfaces
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken ct);
    }
}
