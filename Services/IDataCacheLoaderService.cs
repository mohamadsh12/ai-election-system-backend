namespace WebApplication16.Services
{
    public interface IDataCacheLoaderService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
