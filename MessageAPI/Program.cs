using MessageAPI;
using MessageAPI.Models;
using MessageAPI.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MessageDatabaseSettings>(
    builder.Configuration.GetSection("MessageDatabase"));

builder.Services.AddControllers();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ChatService>();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:4200")
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});

var mongoClient = new MongoClient("mongodb://localhost:27017");
var database = mongoClient.GetDatabase("ProjektChatApp_Data");

//builder.Services.AddSingleton<UserRepository>(provider => new UserRepository(database));
builder.Services.AddSingleton<UserService>(provider => new UserService(database));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
