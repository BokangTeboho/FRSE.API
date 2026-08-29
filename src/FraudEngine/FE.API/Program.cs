using FE.Infrastructure.Rules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<ThresholdRuleOptions>(builder.Configuration.GetSection("FraudRules:Threshold"));
builder.Services.Configure<VelocityRuleOptions>(builder.Configuration.GetSection("FraudRules:Velocity"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
