using CalculatorUI.Abstractions;
using ConsoleApp1;

var builder = WebApplication.CreateBuilder(args);

var booter = new Bootstrapper();
// Add services to the container.
builder.Services.AddSingleton<ICalculatorUi>(booter.Initialize());
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
