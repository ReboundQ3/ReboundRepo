#!/usr/bin/env python3
"""
Phishing Detection Results Analyzer
Analyzes batch analysis results and generates statistics
"""

import sys
import re
from pathlib import Path
from collections import Counter

def parse_results_file(filepath):
    """Parse the batch analysis results file"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    results = {
        'total_emails': 0,
        'phishing_count': 0,
        'safe_count': 0,
        'threat_scores': [],
        'confidences': [],
        'processing_time': 0,
        'emails': []
    }
    
    # Extract totals from summary
    if match := re.search(r'📧 Total Emails: (\d+)', content):
        results['total_emails'] = int(match.group(1))
    
    if match := re.search(r'🚨 Phishing: (\d+)', content):
        results['phishing_count'] = int(match.group(1))
    
    if match := re.search(r'✅ Legitimate: (\d+)', content):
        results['safe_count'] = int(match.group(1))
    
    if match := re.search(r'⏱️  Total Time: (\d+)ms', content):
        results['processing_time'] = int(match.group(1))
    elif match := re.search(r'✅ Completed in ([\d,]+)s', content):
        results['processing_time'] = float(match.group(1).replace(',', '.')) * 1000
    
    # Extract individual scores
    for match in re.finditer(r'Score: ([\d,]+)/10', content):
        score = float(match.group(1).replace(',', '.'))
        results['threat_scores'].append(score)
    
    # Extract confidences
    for match in re.finditer(r'Confidence: (\d+)%', content):
        conf = int(match.group(1))
        results['confidences'].append(conf)
    
    return results

def print_report(results):
    """Print comprehensive analysis report"""
    print("=" * 70)
    print("  📊 PHISHING DETECTION ANALYSIS REPORT")
    print("=" * 70)
    print()
    
    # Basic stats
    print("📧 EMAIL STATISTICS:")
    print(f"   Total Analyzed: {results['total_emails']}")
    print(f"   Phishing Detected: {results['phishing_count']} ({results['phishing_count']/results['total_emails']*100:.1f}%)")
    print(f"   Safe: {results['safe_count']} ({results['safe_count']/results['total_emails']*100:.1f}%)")
    print()
    
    # Threat scores
    if results['threat_scores']:
        scores = results['threat_scores']
        avg_score = sum(scores) / len(scores)
        min_score = min(scores)
        max_score = max(scores)
        
        print("🎯 THREAT SCORE ANALYSIS:")
        print(f"   Average: {avg_score:.2f}/10")
        print(f"   Minimum: {min_score:.2f}/10")
        print(f"   Maximum: {max_score:.2f}/10")
        print()
        
        # Risk distribution
        critical = sum(1 for s in scores if s >= 8)
        high = sum(1 for s in scores if 6 <= s < 8)
        medium = sum(1 for s in scores if 4 <= s < 6)
        low = sum(1 for s in scores if 2 <= s < 4)
        safe = sum(1 for s in scores if s < 2)
        
        print("📈 RISK LEVEL DISTRIBUTION:")
        print(f"   🔴 Critical (8-10): {critical} ({critical/len(scores)*100:.1f}%)")
        print(f"   🟠 High (6-8):      {high} ({high/len(scores)*100:.1f}%)")
        print(f"   🟡 Medium (4-6):    {medium} ({medium/len(scores)*100:.1f}%)")
        print(f"   🔵 Low (2-4):       {low} ({low/len(scores)*100:.1f}%)")
        print(f"   🟢 Safe (0-2):      {safe} ({safe/len(scores)*100:.1f}%)")
        print()
    
    # ML Confidence
    if results['confidences']:
        confs = results['confidences']
        avg_conf = sum(confs) / len(confs)
        print("🤖 ML MODEL CONFIDENCE:")
        print(f"   Average: {avg_conf:.1f}%")
        print(f"   Range: {min(confs)}% - {max(confs)}%")
        print()
    
    # Performance
    if results['processing_time'] > 0:
        total_time_s = results['processing_time'] / 1000
        per_email_ms = results['processing_time'] / results['total_emails'] if results['total_emails'] > 0 else 0
        
        print("⚡ PERFORMANCE:")
        print(f"   Total Time: {total_time_s:.1f}s")
        print(f"   Per Email: {per_email_ms:.0f}ms")
        print(f"   Throughput: {results['total_emails']/total_time_s:.1f} emails/sec")
        print()
    
    # Accuracy assessment (assuming all are phishing from honeypot)
    print("=" * 70)
    print("  🎯 ACCURACY ASSESSMENT")
    print("=" * 70)
    print()
    print("📌 ASSUMPTION: All emails are from phishing honeypot")
    print("   (Expected: 100% should be classified as phishing)")
    print()
    
    if results['total_emails'] > 0:
        accuracy = (results['phishing_count'] / results['total_emails']) * 100
        false_negatives = results['safe_count']
        
        print(f"✅ Detection Accuracy: {accuracy:.1f}%")
        print(f"❌ False Negatives: {false_negatives} ({false_negatives/results['total_emails']*100:.1f}%)")
        print()
        
        if accuracy >= 95:
            grade = "🎉 EXCELLENT"
            comment = "Detection rate is very high!"
        elif accuracy >= 85:
            grade = "👍 GOOD"
            comment = "Detection rate is acceptable for production use"
        elif accuracy >= 70:
            grade = "⚠️  MODERATE"
            comment = "Detection needs improvement"
        else:
            grade = "❌ POOR"
            comment = "Detection rate is too low for practical use"
        
        print(f"Grade: {grade}")
        print(f"Comment: {comment}")
        print()
    
    # Recommendations
    print("=" * 70)
    print("  💡 RECOMMENDATIONS FOR REPORT")
    print("=" * 70)
    print()
    print("1. STRENGTHS TO HIGHLIGHT:")
    if results['phishing_count'] / results['total_emails'] >= 0.85:
        print("   ✅ High detection rate on real-world phishing emails")
    if results['threat_scores'] and sum(results['threat_scores']) / len(results['threat_scores']) >= 7:
        print("   ✅ Accurate threat scoring (high scores for phishing)")
    if results['processing_time'] / results['total_emails'] < 1000:
        print("   ✅ Fast processing speed (< 1s per email)")
    print()
    
    print("2. LIMITATIONS TO DISCUSS:")
    if results['safe_count'] > 0:
        print(f"   ⚠️  {results['safe_count']} false negatives (missed phishing emails)")
        print("      → Some phishing techniques are subtle and hard to detect")
    if results['confidences'] and min(results['confidences']) < 60:
        print("   ⚠️  Some emails have low ML confidence scores")
        print("      → Hybrid approach (ML + rules) helps compensate")
    print()
    
    print("3. FOR YOUR PRESENTATION:")
    print("   • Show this summary slide")
    print("   • Demonstrate live analysis on 1-2 emails")
    print("   • Discuss the balance between false positives/negatives")
    print("   • Explain ethical implications (dual-use AI)")
    print()

def main():
    if len(sys.argv) < 2:
        print("Usage: python analyze_batch_results.py <results_file.txt>")
        print()
        print("Example:")
        print("  python analyze_batch_results.py data/results/batch_analysis_20251004_181915.txt")
        sys.exit(1)
    
    filepath = sys.argv[1]
    
    if not Path(filepath).exists():
        print(f"❌ Error: File not found: {filepath}")
        sys.exit(1)
    
    print(f"📄 Analyzing: {Path(filepath).name}")
    print()
    
    results = parse_results_file(filepath)
    print_report(results)
    
    print("=" * 70)
    print("✅ Analysis complete!")
    print()

if __name__ == "__main__":
    main()
