
using FaceAiSharp;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Services;
using WebApplication16.Services;

namespace WebApplication16;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddDbContext<VotingContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))

    );
        builder.Services.AddControllers();

        // Add CORS support
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IFaceRecognitionService, FaceRecognitionService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IVoteService, VoteService>();
        builder.Services.AddScoped<IProtocolService, ProtocolService>();

        // Register the models as Singleton
        builder.Services.AddSingleton<IFaceDetectorWithLandmarks>(sp => FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks());
        builder.Services.AddSingleton<IFaceEmbeddingsGenerator>(sp => FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator());

        // Add MemoryCache service

        builder.Services.AddMemoryCache();


        // Add Hosted Service
        builder.Services.AddHostedService<DataCacheLoaderService>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAllOrigins");

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
