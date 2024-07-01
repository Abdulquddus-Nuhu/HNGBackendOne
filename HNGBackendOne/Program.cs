using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

if (!(builder.Environment.IsDevelopment()))
{
    builder.WebHost.UseUrls("http://localhost:5555");
}


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

//Add Cors
const string CORS_POLICY = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CORS_POLICY,
                      builder =>
                      {
                          builder.AllowAnyOrigin();
                          builder.AllowAnyMethod();
                          builder.AllowAnyHeader();
                      });
});

// Security enhancements 
if (!builder.Environment.IsDevelopment())
{
    // Proxy Server Config
    builder.Services.Configure<ForwardedHeadersOptions>(
          options =>
          {
              options.ForwardedHeaders =
                  ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
          });

    //Persist key
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("/var/keys"));
}

//Remove Server Header
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
