using System.Collections.Concurrent;
using AutoMapper;
using Facturación.Infrastructure.Contexts;
using Facturación.Models.Dtos;
using Facturación.Models.Entities;
using Facturación.Transversal;
using Microsoft.EntityFrameworkCore;

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


var queue = new ConcurrentQueue<FacturaDto>();
var processingTask = StartProcessingQueue(queue, app.Services);

app.MapPost("/Factura", (FacturaDto factura) =>
{
    var newId = Guid.NewGuid();
    factura.Id = newId.ToString();
    queue.Enqueue(factura);

    return Results.Ok(newId);
});

async Task StartProcessingQueue(ConcurrentQueue<FacturaDto> queue, IServiceProvider services)
{
    while (true)
    {
        if (queue.TryDequeue(out FacturaDto facturaToSave))
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FacturacionContext>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            await CreateFactura(facturaToSave, Guid.NewGuid(), context, mapper);
        }
        else
        {
            await Task.Delay(1000); // Reduce CPU usage when queue is empty
        }
    }
}

async Task CreateFactura(FacturaDto factura, Guid newId, FacturacionContext context, IMapper mapper)
{
    const int maxRetries = 3;
    int currentRetry = 0;
    Random random = new Random();

    while (currentRetry < maxRetries)
    {
        try
        {
            var facturaEntity = mapper.Map<Factura>(factura);
            facturaEntity.Id = newId.ToString();
            context.Facturas.Add(facturaEntity);
            await context.SaveChangesAsync();
            break;
        }
        catch (Exception ex)
        {
            currentRetry++;
            if (currentRetry >= maxRetries)
            {
                // Log the error
                break;
            }

            int delay = random.Next(10, 51) * 1000;
            await Task.Delay(delay);
        }
    }
}

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
app.Run();
