using FutureShopBackend.Api;
using FutureShopBackend.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddAppDI(builder.Configuration);

var app = builder.Build();

builder.Services.AddAppDI(builder.Configuration);

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

app.Run();