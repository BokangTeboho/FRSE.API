using FE.Infrastructure.Rules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<ThresholdRuleOptions>(builder.Configuration.GetSection("FraudRules:Threshold"));
builder.Services.Configure<VelocityRuleOptions>(builder.Configuration.GetSection("FraudRules:Velocity"));
builder.Services.Configure<StructuringRuleOptions>(builder.Configuration.GetSection("FraudRules:Structuring"));
builder.Services.Configure<BehavioralDeviationRuleOptions>(builder.Configuration.GetSection("FraudRules:BehavioralDeviation"));
builder.Services.Configure<RoundNumberRuleOptions>(builder.Configuration.GetSection("FraudRules:RoundNumber"));
builder.Services.Configure<GeographicRuleOptions>(builder.Configuration.GetSection("FraudRules:GeographicRule"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
