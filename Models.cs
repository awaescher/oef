
public record EmbeddingRequest
{
    public string Model { get; set; } = "";
    public List<string> Input { get; set; } = [];
}

public record EmbeddingResponse
{
    public string Object { get; set; } = "";
    public List<EmbeddingData> Data { get; set; } = [];
    public string Model { get; set; } = "";
    public UsageData Usage { get; set; } = new();
}

public record EmbeddingData
{
    public string Object { get; set; } = "";
    public int Index { get; set; } = 0;
    public List<float> Embedding { get; set; } = [];
}

public record UsageData
{
    public int PromptTokens { get; set; } = 0;
    public int TotalTokens { get; set; } = 0;
}
