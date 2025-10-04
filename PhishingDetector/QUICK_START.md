# 🛡️ Phishing Detector - Quick Start Guide

**CSC Security Project 2025**

## 🎯 Wat Doet Deze Tool?

Deze AI-powered phishing detector analyseert emails en detecteert phishing pogingen met:
- **Machine Learning**: BERT-tiny model getraind op spam/phishing detectie
- **Rule-based heuristics**: Extra checks voor URLs, urgency, threats, etc.
- **Threat scoring**: 0-10 schaal (🟢 SAFE → 🔴 CRITICAL)

## 🚀 Snelle Start

### 1. Installatie

```powershell
# Navigeer naar project folder
cd PhishingDetector

# Installeer Python dependencies
cd ml_backend
pip install transformers torch
cd ..

# Test of alles werkt
cd src/PhishingDetector.App
dotnet run -- help
```

### 2. Gebruik

#### Analyseer Eén Email
```powershell
dotnet run -- analyze "path/to/email.txt"
dotnet run -- analyze "path/to/email.eml"
```

#### Batch Analyse (Meerdere Emails)
```powershell
dotnet run -- batch "path/to/folder"
```

#### Save Output naar Bestand
```powershell
dotnet run -- analyze email.txt -o results.txt
dotnet run -- batch ./emails -o report.txt
```

## 📁 Project Structuur

```
PhishingDetector/
├── ml_backend/
│   ├── analyzer.py              # Python ML backend
│   └── requirements.txt         # Python dependencies
├── src/PhishingDetector.App/
│   ├── Models/                  # Data models
│   ├── Services/                # Email analyzer service
│   └── ProgramCLI.cs            # Command-line interface
├── data/
│   ├── test_emails/             # Kleine test set (10 emails)
│   ├── email/                   # GROTE dataset (2000+ phishing emails)
│   └── email_sample/            # Sample subset (50 emails)
└── bert-tiny-finetunes-sms-spam-detection/  # Lokaal ML model
```

## 📊 Output Voorbeeld

```
══════════════════════════════════════════════════════
  📧 EMAIL FILE ANALYSIS
══════════════════════════════════════════════════════

Status: 🚨 PHISHING DETECTED
Risk Level: 🔴 CRITICAL
Threat Score: 9.56/10
ML Confidence: 90.6%
Processing Time: 9ms

⚠️  Risk Factors Detected:
   • ⚠️ Suspicious URL detected
   • ⚠️ Urgency language (pressure tactics)
   • ⚠️ Threatening language
   • 🚨 Requests personal information

💭 Analysis:
   This email is highly likely a phishing attempt...
```

## 🧪 Testing

### Test Data Beschikbaar:
1. **`data/test_emails/`**: Kleine handgemaakte set
2. **`data/email/`**: 2000+ ECHTE phishing emails van honeypot
3. **`data/email_sample/`**: 50 samples voor snelle tests

### Run Tests:
```powershell
# Test single email
dotnet run -- analyze "../../data/email/sample-1.eml"

# Test batch (50 emails, ~30 seconden)
dotnet run -- batch "../../data/email_sample" -o results.txt

# Test hele dataset (2000+ emails, ~15 minuten)
dotnet run -- batch "../../data/email" -o full_results.txt
```

## 📈 Metrics Verzamelen

Na batch analyse krijg je:
- ✅ Total emails processed
- ✅ Phishing count & percentage
- ✅ Average threat score
- ✅ High risk count (score > 7)
- ✅ Processing time

**Voor rapport**: Save output en gebruik Python script om te analyseren:
```powershell
python tools/analyze_results.py phishing_sample_50_results.txt
```

## 🎓 Voor Peer Review (Week 6)

### Demo Scenario:
1. **Setup** (2 min):
   - Laat project structure zien
   - Leg hybride approach uit (ML + rules)

2. **Live Demo** (5 min):
   - Analyseer 1 phishing email → show high threat score
   - Batch analyse op 10 emails → show summary
   - Toon output bestand → show risk factors

3. **Code Walkthrough** (5 min):
   - `analyzer.py`: ML model loading + feature extraction
   - `EmailAnalyzer.cs`: C# → Python integration
   - `ProgramCLI.cs`: Command-line interface

4. **Discussion** (3 min):
   - Ethical implications (dual-use AI)
   - Limitations (false positives, model accuracy)
   - Future improvements

## 💡 Tips & Tricks

### Performance
- **Eerste run is langzaam**: Model moet laden (~10s)
- **Daarna sneller**: ~500ms per email
- **Batch is efficiënter**: Laadt model 1x voor alle emails

### Troubleshooting
```powershell
# Python not found?
python --version
# Of probeer:
py --version

# Model not found?
# Check of folder bestaat:
ls bert-tiny-finetunes-sms-spam-detection

# .NET build errors?
dotnet build
# Check for compile errors
```

### Best Practices
1. **Test altijd met kleine batch eerst** (10-50 emails)
2. **Save output naar bestand** voor analyse achteraf
3. **Backup je resultaten** voordat je opnieuw test
4. **Use .eml files** voor meest realistische testing

## 📝 Voor Je Rapport

### Wat Te Documenteren:

1. **Technische Details**:
   - Model: BERT-tiny (mrm8488/bert-tiny-finetuned-sms-spam-detection)
   - Framework: .NET 9 + Python 3.11
   - Integration: Subprocess execution + JSON serialization

2. **Resultaten**:
   - Detection rate: ___%
   - Average threat score: ___/10
   - Processing speed: ___ms/email
   - False positive rate: ___%

3. **Prompts Gebruikt**:
   - "Build phishing detector with ML"
   - "Use Hugging Face BERT model"
   - "Add command-line batch processing"
   - "Support .eml file format"

4. **Security Considerations**:
   - Dual-use AI (detection vs generation)
   - Privacy (email content)
   - Model limitations (adversarial attacks)

## 🔗 Resources

- **Model**: [Hugging Face - BERT-tiny spam](https://huggingface.co/mrm8488/bert-tiny-finetuned-sms-spam-detection)
- **Dataset**: Real phishing emails from honeypot
- **Framework**: .NET 9 + Transformers library
- **Documentation**: See `README.md` and `PROJECT_PLAN.md`

---

## 🆘 Help Nodig?

Check `PROJECT_STATUS.md` voor:
- ✅ Wat werkt
- ⚠️  Bekende issues
- 📊 Test resultaten
- 💡 Volgende stappen

**Status**: ✅ **PRODUCTION READY**

*Laatste update: 2025-10-04*
