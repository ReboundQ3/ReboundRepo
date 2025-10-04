using PhishingDetector.App.Services;
using PhishingDetector.App.Models;

namespace PhishingDetector.App;

/// <summary>
/// Command-line interface for the Phishing Detector
/// Usage: 
///   dotnet run -- analyze <file.txt>
///   dotnet run -- batch <folder>
///   dotnet run -- analyze <file.txt> -o output.txt
/// </summary>
class ProgramCLI
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        // If no arguments, show help
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }
        
        string command = args[0].ToLower();
        string? outputFile = null;
        bool autoOutput = false;
        
        // Check for output file flag
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-o" || args[i] == "--output")
            {
                if (i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                }
                else
                {
                    autoOutput = true; // -o without filename means auto-generate
                }
            }
        }
        
        // Auto-generate output filename if not specified
        if (autoOutput && string.IsNullOrEmpty(outputFile))
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            outputFile = command switch
            {
                "analyze" or "-a" or "--analyze" => $"analysis_{timestamp}.txt",
                "batch" or "-b" or "--batch" => $"batch_analysis_{timestamp}.txt",
                _ => $"output_{timestamp}.txt"
            };
        }
        
        // Ensure results directory exists
        if (!string.IsNullOrEmpty(outputFile))
        {
            var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), "../../data/results");
            Directory.CreateDirectory(resultsDir);
            outputFile = Path.Combine(resultsDir, Path.GetFileName(outputFile));
        }
        
        // Redirect console output to file if specified
        StreamWriter? fileWriter = null;
        TextWriter originalOut = Console.Out;
        
        if (!string.IsNullOrEmpty(outputFile))
        {
            try
            {
                fileWriter = new StreamWriter(outputFile, false, System.Text.Encoding.UTF8);
                Console.SetOut(fileWriter);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error opening output file: {ex.Message}");
                return 1;
            }
        }
        
        try
        {
            switch (command)
            {
                case "analyze":
                case "-a":
                case "--analyze":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: dotnet run -- analyze <file_path> [-o output.txt]");
                        return 1;
                    }
                    await AnalyzeFile(args[1]);
                    break;
                    
                case "batch":
                case "-b":
                case "--batch":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: dotnet run -- batch <folder_path> [-o output.txt]");
                        return 1;
                    }
                    await BatchAnalyze(args[1]);
                    break;
                    
                case "help":
                case "-h":
                case "--help":
                    ShowHelp();
                    break;
                    
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Use 'help' for usage information.");
                    return 1;
            }
            
            return 0;
        }
        finally
        {
            if (fileWriter != null)
            {
                Console.SetOut(originalOut);
                await fileWriter.FlushAsync();
                fileWriter.Close();
                Console.WriteLine($"✅ Output written to: {outputFile}");
            }
        }
    }
    
    static void ShowHelp()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  🛡️  PHISHING DETECTOR - AI-POWERED EMAIL SECURITY  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("  dotnet run -- <command> [options]");
        Console.WriteLine();
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  analyze <file>              Analyze a single email (.txt, .eml)");
        Console.WriteLine("  batch <folder>              Analyze all emails in a folder");
        Console.WriteLine("  help                        Show this help message");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  -o, --output [file]         Save output to file (auto-generates filename if not provided)");
        Console.WriteLine("                              Results saved to: data/results/");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  Basic analysis:");
        Console.WriteLine("    dotnet run -- analyze ../../data/email/sample-1.eml");
        Console.WriteLine();
        Console.WriteLine("  Save results (auto-generated filename):");
        Console.WriteLine("    dotnet run -- analyze email.txt -o");
        Console.WriteLine("    → Saves to: data/results/analysis_20251004_143052.txt");
        Console.WriteLine();
        Console.WriteLine("  Custom output filename:");
        Console.WriteLine("    dotnet run -- analyze email.txt -o my_analysis.txt");
        Console.WriteLine("    → Saves to: data/results/my_analysis.txt");
        Console.WriteLine();
        Console.WriteLine("  Batch processing:");
        Console.WriteLine("    dotnet run -- batch ../../data/email_sample -o");
        Console.WriteLine("    → Saves to: data/results/batch_analysis_20251004_143052.txt");
        Console.WriteLine();
        Console.WriteLine("SUPPORTED FILE TYPES:");
        Console.WriteLine("  • .txt  - Plain text emails");
        Console.WriteLine("  • .eml  - Standard email format (headers + body)");
        Console.WriteLine();
        Console.WriteLine("NOTE: All results are automatically saved to data/results/ folder");
        Console.WriteLine();
    }
    
    static async Task AnalyzeFile(string filePath)
    {
        // Write headers to stderr so they appear in terminal
        Console.Error.WriteLine("══════════════════════════════════════════════════════");
        Console.Error.WriteLine("  📧 EMAIL FILE ANALYSIS");
        Console.Error.WriteLine("══════════════════════════════════════════════════════");
        Console.Error.WriteLine();
        
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"❌ File not found: {filePath}");
            return;
        }
        
        try
        {
            // Read file content
            string content = await File.ReadAllTextAsync(filePath);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.Error.WriteLine("❌ File is empty");
                return;
            }
            
            Console.Error.WriteLine($"📁 File: {Path.GetFileName(filePath)}");
            Console.Error.WriteLine($"📏 Size: {content.Length} characters");
            Console.Error.WriteLine();
            Console.Error.WriteLine("⏳ Analyzing...");
            Console.Error.WriteLine();
            
            var analyzer = new EmailAnalyzer();
            var result = await analyzer.AnalyzeEmail(content);
            
            if (result != null && string.IsNullOrEmpty(result.Error))
            {
                // Display result
                Console.WriteLine("══════════════════════════════════════════════════════");
                Console.WriteLine("  📊 ANALYSIS RESULT");
                Console.WriteLine("══════════════════════════════════════════════════════");
                Console.WriteLine();
                
                string status = result.IsPhishing ? "🚨 PHISHING DETECTED" : "✅ LEGITIMATE";
                string riskLevel = result.ThreatScore switch
                {
                    >= 8 => "🔴 CRITICAL",
                    >= 6 => "🟠 HIGH",
                    >= 4 => "🟡 MEDIUM",
                    >= 2 => "🔵 LOW",
                    _ => "🟢 SAFE"
                };
                
                Console.WriteLine($"Status: {status}");
                Console.WriteLine($"Risk Level: {riskLevel}");
                Console.WriteLine($"Threat Score: {result.ThreatScore:F2}/10");
                Console.WriteLine($"ML Confidence: {result.Confidence * 100:F1}%");
                Console.WriteLine($"Processing Time: {result.ProcessingTimeMs}ms");
                Console.WriteLine();
                
                if (result.RiskFactors?.Count > 0)
                {
                    Console.WriteLine("⚠️  Risk Factors Detected:");
                    foreach (var factor in result.RiskFactors)
                    {
                        Console.WriteLine($"   • {factor}");
                    }
                    Console.WriteLine();
                }
                
                Console.WriteLine("💭 Analysis:");
                Console.WriteLine($"   {result.Reasoning}");
                Console.WriteLine();
            }
            else
            {
                Console.Error.WriteLine($"❌ Analysis failed: {result?.Error ?? "Unknown error"}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
        }
    }
    
    static async Task BatchAnalyze(string folderPath)
    {
        // Write headers to stderr so they appear in terminal
        Console.Error.WriteLine("══════════════════════════════════════════════════════");
        Console.Error.WriteLine("  📂 BATCH FOLDER ANALYSIS");
        Console.Error.WriteLine("══════════════════════════════════════════════════════");
        Console.Error.WriteLine();
        
        if (!Directory.Exists(folderPath))
        {
            Console.Error.WriteLine($"❌ Folder not found: {folderPath}");
            return;
        }
        
        try
        {
            Console.Error.WriteLine($"📁 Scanning folder: {folderPath}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("⏳ Processing emails...");
            Console.Error.WriteLine();
            
            var analyzer = new EmailAnalyzer();
            var results = await analyzer.AnalyzeBatch(folderPath);
            
            if (results != null)
            {
                Console.WriteLine("══════════════════════════════════════════════════════");
                Console.WriteLine("  📊 BATCH ANALYSIS SUMMARY");
                Console.WriteLine("══════════════════════════════════════════════════════");
                Console.WriteLine();
                Console.WriteLine($"📧 Total Emails: {results.TotalEmails}");
                Console.WriteLine($"🚨 Phishing: {results.PhishingCount} ({(double)results.PhishingCount / results.TotalEmails * 100:F1}%)");
                Console.WriteLine($"✅ Legitimate: {results.SafeCount} ({(double)results.SafeCount / results.TotalEmails * 100:F1}%)");
                Console.WriteLine($"📊 Average Threat Score: {results.AverageScore:F2}/10");
                Console.WriteLine($"⚠️  High Risk (>7): {results.Results?.Count(r => r.ThreatScore > 7) ?? 0}");
                Console.WriteLine($"⏱️  Total Time: {results.TotalTimeMs}ms");
                Console.WriteLine();
                
                Console.WriteLine("──────────────────────────────────────────────────────");
                Console.WriteLine("DETAILED RESULTS:");
                Console.WriteLine("──────────────────────────────────────────────────────");
                Console.WriteLine();
                
                foreach (var result in results.Results ?? new List<AnalysisResult>())
                {
                    string status = result.IsPhishing ? "🚨 PHISHING" : "✅ SAFE";
                    string riskLevel = result.ThreatScore switch
                    {
                        >= 8 => "🔴",
                        >= 6 => "🟠",
                        >= 4 => "🟡",
                        >= 2 => "🔵",
                        _ => "🟢"
                    };
                    
                    Console.WriteLine($"{riskLevel} {result.FileName ?? "Unknown"} - {status}");
                    Console.WriteLine($"   Score: {result.ThreatScore:F1}/10 | Confidence: {result.Confidence * 100:F0}%");
                    
                    if (result.RiskFactors?.Count > 0 && result.RiskFactors.Count <= 3)
                    {
                        Console.WriteLine($"   Risks: {string.Join(", ", result.RiskFactors.Take(3))}");
                    }
                    else if (result.RiskFactors?.Count > 3)
                    {
                        Console.WriteLine($"   Risks: {result.RiskFactors.Count} factors detected");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.Error.WriteLine("❌ Batch analysis failed");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
