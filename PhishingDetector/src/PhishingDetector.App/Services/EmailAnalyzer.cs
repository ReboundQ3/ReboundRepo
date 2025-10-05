using System.Diagnostics;
using System.Text;
using System.Text.Json;
using PhishingDetector.App.Models;

namespace PhishingDetector.App.Services;

/// <summary>
/// Email analyzer that uses Python ML backend
/// </summary>
public class EmailAnalyzer
{
    private readonly string _pythonScriptPath;
    private readonly string _pythonExecutable;
    
    public EmailAnalyzer()
    {
        // Get path to Python script (ml_backend folder)
        var projectRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", ".."
        ));
        
        _pythonScriptPath = Path.Combine(projectRoot, "ml_backend", "analyzer.py");
        
        // Try to find Python executable
        _pythonExecutable = FindPythonExecutable();
        
        if (!File.Exists(_pythonScriptPath))
        {
            throw new FileNotFoundException($"Python script not found at: {_pythonScriptPath}");
        }
        
        // Write info to stderr so it's visible in terminal
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Error.WriteLine($"[INFO] Python script: {_pythonScriptPath}");
        Console.Error.WriteLine($"[INFO] Python executable: {_pythonExecutable}");
        Console.ResetColor();
    }
    
    private string FindPythonExecutable()
    {
        // Try common Python executable names
        var pythonNames = new[] { "python", "python3", "py" };
        
        foreach (var name in pythonNames)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = name,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        return name;
                    }
                }
            }
            catch
            {
                // Try next
            }
        }
        
        return "python"; // Default fallback
    }
    
    public async Task<AnalysisResult> AnalyzeEmail(string emailContent)
    {
        if (string.IsNullOrWhiteSpace(emailContent))
        {
            return new AnalysisResult
            {
                Error = "Email content is empty",
                IsPhishing = false,
                ThreatScore = 0,
                Reasoning = "No content to analyze"
            };
        }
        
        try
        {
            // Call Python script
            var jsonResult = await RunPythonScript(emailContent);
            
            // Parse JSON result
            var result = JsonSerializer.Deserialize<AnalysisResult>(jsonResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (result == null)
            {
                throw new Exception("Failed to deserialize analysis result");
            }
            
            result.AnalyzedAt = DateTime.Now;
            
            return result;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[ERROR] Analysis failed: {ex.Message}");
            Console.ResetColor();
            
            return new AnalysisResult
            {
                Error = ex.Message,
                IsPhishing = false,
                ThreatScore = 0,
                Reasoning = $"Analysis failed: {ex.Message}"
            };
        }
    }
    
    private async Task<string> RunPythonScript(string emailContent)
    {
        // Escape the email content for command line
        var escapedContent = EscapeForCommandLine(emailContent);
        
        var psi = new ProcessStartInfo
        {
            FileName = _pythonExecutable,
            Arguments = $"\"{_pythonScriptPath}\" \"{escapedContent}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        
        using var process = new Process { StartInfo = psi };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };
        
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await process.WaitForExitAsync();
        
        var error = errorBuilder.ToString();
        if (!string.IsNullOrEmpty(error))
        {
            // Print Python stderr to terminal (includes model loading messages)
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Error.WriteLine($"[Python] {error.Trim()}");
            Console.ResetColor();
        }
        
        if (process.ExitCode != 0)
        {
            throw new Exception($"Python script failed with exit code {process.ExitCode}: {error}");
        }
        
        var output = outputBuilder.ToString().Trim();
        
        if (string.IsNullOrEmpty(output))
        {
            throw new Exception("Python script returned empty output");
        }
        
        return output;
    }
    
    private string EscapeForCommandLine(string text)
    {
        // Replace problematic characters
        return text
            .Replace("\"", "\\\"")
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }
    
    public async Task<BatchResults> AnalyzeBatch(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
        }
        
        var results = new List<AnalysisResult>();
        
        // Support both .txt and .eml files
        var txtFiles = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);
        var emlFiles = Directory.GetFiles(folderPath, "*.eml", SearchOption.AllDirectories);
        var files = txtFiles.Concat(emlFiles).ToArray();
        
        if (files.Length == 0)
        {
            Console.Error.WriteLine("⚠️  No .txt or .eml files found in folder");
            return new BatchResults();
        }
        
        // Write progress info to stderr so it shows in terminal even when output redirected
        Console.Error.WriteLine($"\n📁 Processing {files.Length} emails...");
        Console.Error.WriteLine();
        
        var startTime = DateTime.Now;
        var progressBar = 0;
        
        foreach (var file in files)
        {
            progressBar++;
            var fileName = Path.GetFileName(file);
            
            // Show progress bar (to terminal/stderr)
            DrawProgressBar(progressBar, files.Length, fileName);
            
            try
            {
                var content = await File.ReadAllTextAsync(file);
                
                // Extract body from .eml files
                if (file.EndsWith(".eml", StringComparison.OrdinalIgnoreCase))
                {
                    content = ExtractEmailBody(content);
                }
                
                var result = await AnalyzeEmail(content);
                result.FileName = fileName;
                
                results.Add(result);
                
                // Update progress bar with result (to terminal/stderr)
                Console.Error.Write(" ");
                if (result.IsPhishing)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write($"🚨 PHISHING ({result.ThreatScore:F1}/10)");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Error.Write($"✅ SAFE ({result.ThreatScore:F1}/10)");
                }
                Console.ResetColor();
                Console.Error.WriteLine();
            }
            catch (Exception ex)
            {
                Console.Error.Write(" ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.Write($"❌ ERROR: {ex.Message}");
                Console.ResetColor();
                Console.Error.WriteLine();
            }
        }
        
        var elapsed = DateTime.Now - startTime;
        Console.Error.WriteLine();
        Console.Error.WriteLine($"✅ Completed in {elapsed.TotalSeconds:F1}s");
        Console.Error.WriteLine();
        
        return new BatchResults
        {
            Results = results
        };
    }
    
    private void DrawProgressBar(int current, int total, string fileName)
    {
        const int barWidth = 30;
        var percentage = (double)current / total;
        var filledWidth = (int)(barWidth * percentage);
        
        Console.Error.Write($"[{current}/{total}] ");
        
        // Draw progress bar
        Console.Error.Write("[");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Error.Write(new string('█', filledWidth));
        Console.ResetColor();
        Console.Error.Write(new string('░', barWidth - filledWidth));
        Console.Error.Write("]");
        
        Console.Error.Write($" {percentage * 100:F0}% ");
        
        // Truncate filename if too long
        var displayName = fileName.Length > 30 ? fileName.Substring(0, 27) + "..." : fileName;
        Console.Error.Write($"{displayName}");
    }
    
    private string ExtractEmailBody(string emlContent)
    {
        // Parse EML - keep IMPORTANT headers + body for proper analysis
        // Headers like SPF, DKIM, DMARC are CRITICAL for phishing detection!
        var lines = emlContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        bool inBody = false;
        var importantHeaders = new List<string>();
        var bodyLines = new List<string>();
        
        // List of header prefixes we want to keep
        var keepHeaders = new[]
        {
            "authentication-results:",
            "received-spf:",
            "dkim-signature:",
            "from:",
            "subject:",
            "x-mailer:",
            "list-unsubscribe:",
            "return-path:",
            "sender:"
        };
        
        foreach (var line in lines)
        {
            if (inBody)
            {
                // Stop if we hit base64 encoded content (images, attachments)
                if (line.Length > 1000 || (line.Length > 100 && line.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=')))
                {
                    continue;
                }
                bodyLines.Add(line);
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                // Empty line marks end of headers
                inBody = true;
            }
            else
            {
                // Check if this is an important header
                var lineLower = line.ToLowerInvariant();
                if (keepHeaders.Any(h => lineLower.StartsWith(h)) || 
                    (line.StartsWith(" ") && importantHeaders.Count > 0)) // Continuation line
                {
                    importantHeaders.Add(line);
                }
            }
        }
        
        // Combine: important headers + body
        var headerSection = string.Join("\n", importantHeaders);
        var bodySection = string.Join("\n", bodyLines).Trim();
        
        // Limit total size to avoid huge emails
        var combined = headerSection + "\n\n" + bodySection;
        if (combined.Length > 15000)
        {
            // Keep headers, limit body
            if (headerSection.Length > 5000)
            {
                headerSection = headerSection.Substring(0, 5000);
            }
            var remainingSpace = 15000 - headerSection.Length;
            if (bodySection.Length > remainingSpace)
            {
                bodySection = bodySection.Substring(0, remainingSpace);
            }
            combined = headerSection + "\n\n" + bodySection;
        }
        
        return combined;
    }
}
