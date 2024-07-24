using System.Text.Json.Serialization;

public record EmbeddingRequest
{
	[JsonPropertyName("model")]
	public string Model { get; set; } = "";

	[JsonPropertyName("input")]
	public string Input { get; set; } = "";
}

public record EmbeddingResponse
{
	[JsonPropertyName("object")]
	public string Object { get; set; } = "";

	[JsonPropertyName("data")]
	public List<EmbeddingData> Data { get; set; } = [];

	[JsonPropertyName("model")]
	public string Model { get; set; } = "";

	[JsonPropertyName("usage")]
	public UsageData Usage { get; set; } = new();
}

public record EmbeddingData
{
	[JsonPropertyName("object")]
	public string Object { get; set; } = "";

	[JsonPropertyName("index")]
	public int Index { get; set; } = 0;

	[JsonPropertyName("embedding")]
	public List<float> Embedding { get; set; } = [];
}

public record UsageData
{
	[JsonPropertyName("prompt_token")]
	public int PromptTokens { get; set; } = 0;

	[JsonPropertyName("total_tokens")]
	public int TotalTokens { get; set; } = 0;
}
