# 🚀 Quick Setup Guide

## Step-by-Step Installation (10 minutes)

### Step 1: Check Prerequisites (2 min)

Open PowerShell and verify:

```powershell
# Check Python
python --version
# Should show: Python 3.8 or higher

# Check .NET
dotnet --version
# Should show: 8.0 or higher
```

❌ **If either is missing:**
- Python: https://www.python.org/downloads/
- .NET 8: https://dotnet.microsoft.com/download

---

### Step 2: Setup Python Backend (5 min)

```powershell
# Navigate to project
cd "D:\Visual Studio Code\ReboundRepo\PhishingDetector\ml_backend"

# Install Python packages
pip install transformers torch

# Test Python script
python analyzer.py "This is a test email"
```

**Expected output:**
```
🔄 Loading spam detection model...
✅ Model loaded successfully!
{
  "is_phishing": false,
  "confidence": 0.XX,
  ...
}
```

⚠️ **First run downloads model (~17MB)** - Be patient!

---

### Step 3: Build C# Application (2 min)

```powershell
# Navigate to C# project
cd ..\src\PhishingDetector.App

# Build
dotnet build
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### Step 4: Run the Application (1 min)

```powershell
# Run
dotnet run
```

**Expected output:**
```
╔══════════════════════════════════════════════════════╗
║     🛡️  AI-POWERED PHISHING DETECTOR 🛡️             ║
╚══════════════════════════════════════════════════════╝

🔄 Initializing AI-Powered Phishing Detector...
✅ System ready!

  📋 MAIN MENU
  [1] 📧 Analyze Single Email
  ...
```

---

## 🧪 First Test

### Test Single Email

1. Choose option `[1]`
2. Paste this phishing email:
   ```
   URGENT! Your account will be suspended.
   Click here immediately: http://evil-site.tk
   ```
3. Type `END` and press Enter

**Expected result:**
```
Status: ⚠️  PHISHING DETECTED
Threat Score: 8.5/10.0
```

### Test Batch Processing

1. Choose option `[2]`
2. Enter path: `..\..\..\..\data\test_emails`
3. Wait for processing
4. Save report when asked

**Expected result:**
```
Total Emails: 10
Phishing Detected: 5
Safe Emails: 5
```

---

## ❌ Troubleshooting

### Problem: "Python not found"
**Solution:**
```powershell
# Use full path
py --version  # Try 'py' instead of 'python'
```

### Problem: "'pip' is not recognized"
**Solution:**
```powershell
python -m pip install transformers torch
```

### Problem: "Access denied" or "Execution policy"
**Solution:**
```powershell
# Run PowerShell as Administrator
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

### Problem: Model download fails
**Solution:**
- Check internet connection
- Try again (download might have interrupted)
- Check firewall/antivirus settings

### Problem: "Build failed" in C#
**Solution:**
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## ✅ Success Checklist

- [ ] Python installed and working
- [ ] .NET installed and working
- [ ] Python packages installed (transformers, torch)
- [ ] Model downloaded successfully (~17MB)
- [ ] C# project builds without errors
- [ ] Application runs and shows menu
- [ ] Single email analysis works
- [ ] Batch processing works with test emails

---

## 🎯 Next Steps

Once everything works:

1. **Collect More Test Data**
   - Add more emails to `data/test_emails/`
   - Try real phishing examples from PhishTank.org

2. **Run Full Tests**
   - Test all menu options
   - Generate phishing demos
   - Test vulnerabilities
   - Generate HTML reports

3. **Start Documentation**
   - Take screenshots of results
   - Document interesting findings
   - Note accuracy metrics

4. **Begin Report Writing**
   - Use the results for your 3-page report
   - Document prompts used
   - Reflect on AI limitations

---

## 💡 Tips

- **First run is slow** - Model loading takes time
- **Subsequent runs are fast** - Model stays in memory
- **Save your work** - Git commit regularly
- **Test often** - Don't wait until the last day!
- **Document everything** - Screenshots, metrics, findings

---

## 🆘 Still Having Issues?

1. Check the main [README.md](README.md) for detailed docs
2. Review [PROJECT_PLAN.md](../CSC/PROJECT_PLAN.md) for architecture
3. Check [Voortgang.md](../CSC/Documentatie/Voortgang.md) for notes
4. Ask your teammates or instructor

---

**You're ready to go! 🚀**

Start with option [1] to analyze your first email!
