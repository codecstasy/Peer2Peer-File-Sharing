using Microsoft.Extensions.DependencyInjection;
using Peer2Peer_File_Sharing.Services;
using System;
using System.IO;
using Serilog;

// Configure Serilog from appsettings.json
var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddScoped<IFileChunker, FileChunker>();
builder.Services.AddScoped<IFileAssembler, FileAssembler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthorization();

app.Run();