using System.Text.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var apiUrl = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? throw new InvalidOperationException("OLLAMA_ENDPOINT environment variable is required");

app.MapPost("/v1/embeddings", async (HttpContext context) =>
{
	try
	{
		var body = await JsonSerializer.DeserializeAsync<EmbeddingRequest>(context.Request.Body);

		if (body is null)
			throw new ArgumentException(nameof(body));

		Console.WriteLine($"forwarding /v1/embeddings \"{body.Input.Substring(0, Math.Min(50, body.Input.Length))} ...\"");

		var results = new List<EmbeddingData>();
		var embedding = await FetchEmbeddings(apiUrl, body.Model, body.Input);
		results.Add(new EmbeddingData
		{
			Object = "embedding",
			Index = 0,
			Embedding = embedding
		});

		var response = new EmbeddingResponse
		{
			Object = "list",
			Data = results,
			Model = body.Model,
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

static async Task<List<float>> FetchEmbeddings(string url, string model, string input)
{
	var client = new HttpClient();

	var requestBody = new OllamaEmbeddingRequest
	{
		Model = model,
		Prompt = input
	};

	var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

	var response = await client.PostAsync($"{url}/api/embeddings", content);
	var responseString = await response.Content.ReadAsStringAsync();

	var jsonData = JsonSerializer.Deserialize<JsonElement>(responseString);
	var embeddings = jsonData.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToList();

	Console.WriteLine($"Embeddings: \"{input.Substring(0, Math.Min(50, input.Length))}...\" -> {responseString.Substring(0, Math.Min(50, responseString.Length))}...");

	return embeddings;
}
