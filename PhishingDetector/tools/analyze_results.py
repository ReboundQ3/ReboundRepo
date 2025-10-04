# Email Dataset Analysis Tool
# Analyzes the results from phishing detection

import json
import sys
from pathlib import Path
from collections import Counter

def analyze_results(results_file):
    """Analyze the batch analysis results"""
    
    # Parse the results file (it's text, not JSON, so we'll count patterns)
    with open(results_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Count emails
    total_emails = content.count('📧')
    phishing_detected = content.count('🚨 PHISHING')
    safe_detected = content.count('✅ SAFE')
    
    # Extract threat scores
    import re
    scores = re.findall(r'Score: ([\d,]+)/10', content)
    threat_scores = [float(s.replace(',', '.')) for s in scores]
    
    print("=" * 60)
    print("  📊 PHISHING DETECTION ANALYSIS REPORT")
    print("=" * 60)
    print()
    print(f"📧 Total Emails Analyzed: {total_emails}")
    print(f"🚨 Phishing Detected: {phishing_detected} ({phishing_detected/total_emails*100:.1f}%)")
    print(f"✅ Safe Classified: {safe_detected} ({safe_detected/total_emails*100:.1f}%)")
    print()
    
    if threat_scores:
        avg_score = sum(threat_scores) / len(threat_scores)
        min_score = min(threat_scores)
        max_score = max(threat_scores)
        
        print(f"📊 Threat Score Statistics:")
        print(f"   Average: {avg_score:.2f}/10")
        print(f"   Minimum: {min_score:.2f}/10")
        print(f"   Maximum: {max_score:.2f}/10")
        print()
        
        # Score distribution
        critical = sum(1 for s in threat_scores if s >= 8)
        high = sum(1 for s in threat_scores if 6 <= s < 8)
        medium = sum(1 for s in threat_scores if 4 <= s < 6)
        low = sum(1 for s in threat_scores if 2 <= s < 4)
        safe = sum(1 for s in threat_scores if s < 2)
        
        print(f"📈 Risk Level Distribution:")
        print(f"   🔴 Critical (8-10): {critical} ({critical/len(threat_scores)*100:.1f}%)")
        print(f"   🟠 High (6-8):      {high} ({high/len(threat_scores)*100:.1f}%)")
        print(f"   🟡 Medium (4-6):    {medium} ({medium/len(threat_scores)*100:.1f}%)")
        print(f"   🔵 Low (2-4):       {low} ({low/len(threat_scores)*100:.1f}%)")
        print(f"   🟢 Safe (0-2):      {safe} ({safe/len(threat_scores)*100:.1f}%)")
        print()
    
    # Since these are all from a phishing honeypot, they should ALL be phishing
    # Calculate accuracy (assuming all emails are actually phishing)
    print("=" * 60)
    print("  🎯 ACCURACY ASSESSMENT")
    print("=" * 60)
    print()
    print("📌 NOTE: All emails are from a phishing honeypot")
    print("   (Expected: 100% should be classified as phishing)")
    print()
    
    if total_emails > 0:
        accuracy = (phishing_detected / total_emails) * 100
        print(f"✅ Detection Accuracy: {accuracy:.1f}%")
        print(f"❌ False Negatives: {safe_detected} ({safe_detected/total_emails*100:.1f}%)")
        print()
        
        if accuracy >= 95:
            print("🎉 EXCELLENT: Detection rate is very high!")
        elif accuracy >= 85:
            print("👍 GOOD: Detection rate is acceptable")
        elif accuracy >= 70:
            print("⚠️  MODERATE: Detection needs improvement")
        else:
            print("❌ POOR: Detection rate is too low")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python analyze_results.py <results_file>")
        sys.exit(1)
    
    results_file = sys.argv[1]
    if not Path(results_file).exists():
        print(f"Error: File not found: {results_file}")
        sys.exit(1)
    
    analyze_results(results_file)
