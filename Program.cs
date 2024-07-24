using System.Text.Json;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapPost("/v1/embeddings", async (HttpContext context) =>
{
    try
    {
        var body = await JsonSerializer.DeserializeAsync<EmbeddingRequest>(context.Request.Body);

        if (body is null)
            throw new ArgumentNullException(nameof(body));

        var model = body.Model;
        var inputData = body.Input;

        Console.WriteLine($"/v1/embeddings handler {string.Join(", ", inputData.Take(1))}...");

        var results = new List<EmbeddingData>();
        for (int i = 0; i < inputData.Count; i++)
        {
            var embedding = await FetchEmbeddings(model, inputData[i]);
            results.Add(new EmbeddingData
            {
                Object = "embedding",
                Index = i,
                Embedding = embedding
            });
        }

        var response = new EmbeddingResponse
        {
            Object = "list",
            Data = results,
            Model = model,
            Usage = new UsageData
            {
                PromptTokens = 0,
                TotalTokens = 0
            }
        };

        await context.Response.WriteAsJsonAsync(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        context.Response.StatusCode = 500;
    }
});

await app.RunAsync();

static async Task<List<float>> FetchEmbeddings(string model, string inputText)
{
    using var client = new HttpClient();
    var apiUrl = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");

    if (apiUrl == null)
        throw new InvalidOperationException("OLLAMA_ENDPOINT environment variable is required");

    var requestBody = new
    {
        model,
        prompt = inputText
    };

    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

    var response = await client.PostAsync($"{apiUrl}/api/embeddings", content);
    var responseString = await response.Content.ReadAsStringAsync();

    var jsonData = JsonSerializer.Deserialize<JsonElement>(responseString);
    var embeddings = jsonData.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToList();

    Console.WriteLine($"Embeddings: {inputText.Substring(0, Math.Min(50, inputText.Length))}... -> {responseString.Substring(0, Math.Min(50, responseString.Length))}...");

    return embeddings;
}
