using SignalR.Api;
using SignalR.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IDictionary<string, UserConnection>>(opts =>
    new Dictionary<string, UserConnection>()
);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(build =>
    {
        build.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chat");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();