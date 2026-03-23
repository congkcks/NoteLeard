using Microsoft.EntityFrameworkCore;
using NoteLearn.Models;
using NoteLearn.Services;
using NoteLearn.Services.AI;
using NoteLearn.Services.Background;
using NoteLearn.Services.Download;
using NoteLearn.Services.Ingest;
using NoteLearn.Services.Pdf;
using NoteLearn.Services.Storage;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()   
              .AllowAnyMethod()  
              .AllowAnyHeader()); 
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EngLishContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()
    )
);
builder.Services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(200));
builder.Services.AddHttpClient<RemoteFileDownloader>();
builder.Services.AddSingleton<PdfTextExtractor>();
builder.Services.AddScoped<PdfRagIngestJob>();
builder.Services.AddScoped<PdfPipelineService>();
builder.Services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddHttpClient<ILlmService, OpenAiLlmService>();
builder.Services.AddScoped<YoutubeTranscriptService>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();