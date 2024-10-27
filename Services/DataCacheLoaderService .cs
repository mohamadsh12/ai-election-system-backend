using FaceAiSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VotingApp.Data;

namespace WebApplication16.Services;

public class DataCacheLoaderService : IHostedService, IDataCacheLoaderService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;
    private readonly IFaceDetectorWithLandmarks _faceDetector;
    private readonly IFaceEmbeddingsGenerator _faceEmbeddingsGenerator;

    public DataCacheLoaderService(IServiceProvider serviceProvider, IMemoryCache cache, IFaceDetectorWithLandmarks faceDetector, IFaceEmbeddingsGenerator faceEmbeddingsGenerator)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
        _faceDetector = FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks();
        _faceEmbeddingsGenerator = FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator();
    }
    //טוענים את הדאטא של כל המשתמשים פעם אחת לפני שירות קאש שמשפר ביצועים
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VotingContext>();
                var users = await context.Users.ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10000));

                _cache.Set("usersList", users, cacheEntryOptions);

            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
