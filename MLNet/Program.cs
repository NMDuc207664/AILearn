using WEBAPI2.ML;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddSingleton<SentimentModel>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();
if (args.Contains("console"))
{
    using var scope = app.Services.CreateScope();
    var sentimentModel = scope.ServiceProvider.GetRequiredService<SentimentModel>();

    Console.WriteLine("\nNhập câu để phân tích cảm xúc (gõ 'exit' để thoát):");
    while (true)
    {
        Console.Write("> ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;

        var result = sentimentModel.Predict(input);
        Console.WriteLine($"Positive: {result.Prediction}, Score: {result.Score:F2}, Probability: {result.Probability:P2}");
    }

    return;
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapControllers();
app.Run();