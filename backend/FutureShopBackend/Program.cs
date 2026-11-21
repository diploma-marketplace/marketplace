using FutureShopBackend.Api;
using FutureShopBackend.Api.Middleware;
using FutureShopBackend.Infrastructure.Auth;
using FutureShopBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddAppDI(builder.Configuration);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
// Сесія зберігається в Redis (RedisTicketStore); у cookie лише ключ сесії
builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<RedisTicketStore>((options, store) =>
    {
        options.SessionStore = store;
        options.Cookie.Name = "FutureMarket.Session";
        options.Cookie.HttpOnly = true;                           // недоступна з JS
        // лише HTTPS; локально в Docker API слухає http://localhost, тому там дозволяємо http
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        // замість редіректу 401/403
        options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
    });

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
app.UseAuthentication();
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