namespace PhishingDetector.App.Models;

/// <summary>
/// Results from batch email analysis
/// </summary>
public class BatchResults
{
    public List<AnalysisResult> Results { get; set; } = new();
    public int TotalEmails { get; set; }
    public int PhishingCount { get; set; }
    public int LegitimateCount { get; set; }
    public int SafeCount { get; set; }
    public double AverageThreatScore { get; set; }
    public double AverageScore { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public long TotalTimeMs { get; set; }
    public DateTime AnalyzedAt { get; set; }
}
