using AutoMapper;
using Facturación.Infrastructure.Contexts;
using Facturación.Models.Dtos;
using Facturación.Models.Entities;
using Facturación.Transversal;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddDbContext<FacturacionContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionString"));
    options.EnableServiceProviderCaching(false);
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

builder.Services.AddAutoMapper(typeof(Mappers));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapPost("/Factura", async (Factura factura, FacturacionContext context, IMapper mapper) =>
{
    var stopwatch = new Stopwatch();
    stopwatch.Start(); // Start timing

    var facturaDto = mapper.Map<FacturaDto>(factura);

    // Save Factura to the database
    await context.AddAsync(factura);
    await context.SaveChangesAsync();

    // Optionally, update the DTO with any entity changes (like generated Id)
    mapper.Map(factura, facturaDto);

    stopwatch.Stop(); // Stop timing

    var elapsedMilliseconds = stopwatch.ElapsedMilliseconds; // This is the total time in milliseconds

    // You can use elapsedMilliseconds as needed, for example, logging it
    Console.WriteLine($"Request took {elapsedMilliseconds} ms");

    factura.TimeCompletion=elapsedMilliseconds;
    context.Update(factura);
    await context.SaveChangesAsync();

    return facturaDto; // Return the ID from the DTO
})
.WithOpenApi();

app.MapGet("/Factura", async (FacturacionContext context, IMapper mapper) =>
{
    return mapper.Map<List<FacturaDto>>(await context.Facturas.ToListAsync());
})
.WithOpenApi();

app.MapDelete("/Factura", async (FacturacionContext context, IMapper mapper) =>
{
    await context.Facturas.ExecuteDeleteAsync();
})
.WithOpenApi();

app.MapGet("/FacturaPromedio", async (FacturacionContext context) =>
{
    // Assuming TimeCompletion is a nullable type. If it's non-nullable, adjust accordingly.
    var averageTimeCompletion = await context.Facturas
                                    .Where(f => f.TimeCompletion.HasValue)
                                    .AverageAsync(f => f.TimeCompletion.Value);

    return Results.Ok(averageTimeCompletion.ToString()+" milisegundos");
})
.WithOpenApi();

app.Run();
