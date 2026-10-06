using EcomSearchApi.Data;
using EcomSearchApi.Infrastructure;
using EcomSearchApi.Repositories; 
using EcomSearchApi.Services;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from appsettings.json
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Configure PostgreSQL connection via DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Configure Elasticsearch client
var esUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
var esSettings = new ElasticsearchClientSettings(new Uri(esUri))
    .DefaultIndex(builder.Configuration["Elasticsearch:IndexName"] ?? "products");
builder.Services.AddSingleton(new ElasticsearchClient(esSettings));

// Register repositories, services, and infrastructure
builder.Services.AddSingleton<ElasticIndexManager>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductSearchRepository, ProductSearchRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IResetService, ResetService>();
builder.Services.AddScoped<IDiagnosticsService, DiagnosticsService>();

// Configure controllers and Swagger OpenAPI documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = ".NET 10 + Elasticsearch 8.17",
        Version = "v1",
        Description = "Demo ELK Stack and Elasticsearch search integration."
    });
});

var app = builder.Build();

// Ensure PostgreSQL database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Log HTTP access requests formatted for Serilog and Logstash Grok filter
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcomSearchApi v1");
    c.RoutePrefix = string.Empty; 
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();