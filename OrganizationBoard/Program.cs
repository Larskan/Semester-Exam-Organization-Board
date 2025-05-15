using System.Threading.RateLimiting;
using EFramework.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<LoginServiceApi.Service.ILoginService, LoginServiceApi.Service.LoginService>();


var configuration = builder.Configuration;
builder.Services.AddDbContext<OBDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
);

var retryPolicy = Policy
    .Handle<SqlException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
        {
            return attempt switch
            {
                1 => TimeSpan.FromSeconds(5),  // First retry after 5 seconds
                2 => TimeSpan.FromSeconds(15), // Second retry after 15 seconds
                3 => TimeSpan.FromSeconds(30), // Third retry after 30 seconds
                _ => TimeSpan.Zero // No retry after 3 attempts
            };
        },
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timeSpan.Seconds} seconds due to: {exception.Message}");
        });

builder.Services.AddSingleton<IAsyncPolicy>(retryPolicy);

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.Get(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: key => new FixedWindowRateLimiter(
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10, // Max 10 requests
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2
                })));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthentication();
//app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers().RequireRateLimiting("fixed");
//app.UseHttpsRedirection();

app.Run();