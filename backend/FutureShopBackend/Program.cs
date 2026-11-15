using FutureShopBackend.Api;
using FutureShopBackend.Api.Middleware;
using FutureShopBackend.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddAppDI(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Future Market API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Future Market — API Contract (Swagger UI)";
    options.DisplayRequestDuration();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", async (AppDbContext db, CancellationToken ct) =>
{
    try
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(3));
        var dbOk = await db.Database.CanConnectAsync(cts.Token);
        return dbOk
            ? Results.Ok(new { status = "healthy", database = "up" })
            : Results.Json(new { status = "unhealthy", database = "down" }, statusCode: 503);
    }
    catch (Exception)
    {
        return Results.Json(new { status = "unhealthy", database = "down" }, statusCode: 503);
    }
})
.WithTags("Health")
.WithSummary("Перевірка стану бекенду та бази даних")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status503ServiceUnavailable);

app.Run();