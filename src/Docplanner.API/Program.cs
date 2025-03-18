using Docplanner.API.Routes;
using Docplanner.API.Configuration;
using Docplanner.API.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();  
builder.Logging.AddConsole();  
builder.Logging.AddDebug();  

builder.Services.AddAuth();
builder.Services.AddSwagger();
builder.Services.AddMediatRServices();
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapSlotsEndpoints();

app.Run();
