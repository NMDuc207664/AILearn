


using Chatbot.Services;
using Chatbot.Services.Interfaces;
using WEBAPI4;
using WEBAPI4.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký AI Service
builder.Services.AddSingleton<IAIService, AIService>();
builder.Services.AddSingleton<IOnnxPredictionService, OnnxPredictionService>();


// Scoped cho Pipeline Service (có thể tracking per request)
builder.Services.AddScoped<IAIPipelineService, AIPipelineService>();

// Add CORS nếu cần
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{



    // Initialize ONNX Service
    var onnxService = scope.ServiceProvider.GetRequiredService<IOnnxPredictionService>();
    await onnxService.InitializeAsync();

    // Initialize Pipeline Service
    var pipelineService = scope.ServiceProvider.GetRequiredService<IAIPipelineService>();
    await pipelineService.InitializeAsync();


}
// Configure the HTTP request pipeline.
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