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

builder.Services.AddSingleton<IDictionary<Guid, string>>(opts =>
    {
        var rooms = new Dictionary<Guid, string>();
        rooms.Add(Guid.NewGuid(), "GENEL");
        return rooms;
    }
);


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(build =>
    {
        build.WithOrigins("http://localhost:3000"
                ,"http://192.168.1.139:3000"
                ,"https://chatapp.askforetu.com.tr/"
                ,"http://chatapp.askforetu.com.tr/")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseWebSockets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors();

app.UseEndpoints(endpoints => { endpoints.MapHub<ChatHub>("/chat"); });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();