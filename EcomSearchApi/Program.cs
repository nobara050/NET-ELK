using Elastic.Clients.Elasticsearch;
using EcomSearchApi.Data;
using EcomSearchApi.Infrastructure;
using EcomSearchApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog đọc từ appsettings.json
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Kết nối PostgreSQL qua DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Cấu hình Elasticsearch Client
var esUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
var esSettings = new ElasticsearchClientSettings(new Uri(esUri))
    .DefaultIndex(builder.Configuration["Elasticsearch:IndexName"] ?? "products");
builder.Services.AddSingleton(new ElasticsearchClient(esSettings));

// Đăng ký Services & Infrastructure
builder.Services.AddSingleton<ElasticIndexManager>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IResetService, ResetService>();
builder.Services.AddScoped<IDiagnosticsService, DiagnosticsService>();

// Cấu hình Controllers & Swagger UI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = ".NET 10 + Elasticsearch 8.17)",
        Version = "v1",
        Description = "Demo ELK Stack."
    });
});

var app = builder.Build();

// Khởi tạo Database PostgreSQL nếu chưa tồn tại
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Log HTTP access format chuẩn hóa cho Logstash Grok filter
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