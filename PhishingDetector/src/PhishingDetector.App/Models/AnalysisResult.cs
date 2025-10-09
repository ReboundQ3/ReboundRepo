using System.Text.Json.Serialization;

namespace PhishingDetector.App.Models;

/// <summary>
/// Result of phishing email analysis
/// </summary>
public class AnalysisResult
{
    [JsonPropertyName("is_phishing")]
    public bool IsPhishing { get; set; }
    
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
    
    [JsonPropertyName("threat_score")]
    public double ThreatScore { get; set; }
    
    [JsonPropertyName("risk_factors")]
    public List<string> RiskFactors { get; set; } = new();
    
    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
    
    [JsonPropertyName("processing_time_ms")]
    public int ProcessingTimeMs { get; set; }
    
    [JsonPropertyName("ml_label")]
    public string? MlLabel { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    // Additional properties for C# side
    public string? FileName { get; set; }
    public DateTime AnalyzedAt { get; set; }
}
