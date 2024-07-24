using System.Text.Json;
using System.Text;

var config = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: true)
	.AddEnvironmentVariables()
	.AddCommandLine(args)
	.Build();

var port = config.GetValue<int>("Port");
var ollamaUrl = config.GetValue<string>("OllamaUrl");

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (port < 1)
	port = 11434 + 1; // next to the Ollama port

if (string.IsNullOrWhiteSpace(ollamaUrl))
	throw new ArgumentException("Ollama url is not set. Define it within appsettings.json, per environment variable OLLAMAURL or by command line argument --ollamaUrl='...'");

Console.WriteLine("==================================");
Console.WriteLine($" oef: Ollama Embeddings Forwarder");
Console.WriteLine("----------------------------------");
Console.WriteLine($"Listening on port {port}");
Console.WriteLine($"Forwarding to \"{ollamaUrl}\"");
Console.WriteLine("");

app.MapPost("/v1/embeddings", async (HttpContext context) =>
{
	try
	{
		var body = await JsonSerializer.DeserializeAsync<EmbeddingRequest>(context.Request.Body);

		if (body is null)
			throw new ArgumentException(nameof(body));

		Console.WriteLine($"forwarding /v1/embeddings \"{body.Input.Substring(0, Math.Min(50, body.Input.Length))} ...\"");

		var results = new List<EmbeddingData>();
		var embedding = await FetchEmbeddings(ollamaUrl, body.Model, body.Input);
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
