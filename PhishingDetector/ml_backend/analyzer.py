#!/usr/bin/env python3
"""
Phishing Email Analyzer - ML Backend
Uses Hugging Face BERT-tiny model fine-tuned for spam detection
Model: mrm8488/bert-tiny-finetuned-sms-spam-detection
"""

import sys
import json
import time
import re
from transformers import AutoTokenizer, AutoModelForSequenceClassification, pipeline
import torch

class PhishingAnalyzer:
    def __init__(self):
        """Initialize the analyzer with spam detection model"""
        print("🔄 Loading spam detection model...", file=sys.stderr)
        
        # Try local model first, then fall back to HuggingFace
        import os
        script_dir = os.path.dirname(os.path.abspath(__file__))
        local_model_path = os.path.join(script_dir, "..", "bert-tiny-finetunes-sms-spam-detection")
        
        # Check if local model exists
        if os.path.exists(local_model_path):
            model_name = local_model_path
            print(f"📁 Using local model: {local_model_path}", file=sys.stderr)
        else:
            model_name = "mrm8488/bert-tiny-finetuned-sms-spam-detection"
            print(f"🌐 Downloading model from HuggingFace...", file=sys.stderr)
        
        try:
            # Load model and tokenizer
            self.tokenizer = AutoTokenizer.from_pretrained(model_name)
            self.model = AutoModelForSequenceClassification.from_pretrained(model_name)
            
            # Create classification pipeline
            self.classifier = pipeline(
                "text-classification",
                model=self.model,
                tokenizer=self.tokenizer,
                device=-1  # CPU
            )
            
            print("✅ Model loaded successfully!", file=sys.stderr)
            
        except Exception as e:
            print(f"❌ Error loading model: {e}", file=sys.stderr)
            print("💡 Make sure you have internet connection for first-time download", file=sys.stderr)
            sys.exit(1)
    
    def extract_features(self, email_text):
        """Extract manual features from email for enhanced detection"""
        
        # Convert to lowercase for pattern matching
        text_lower = email_text.lower()
        
        features = {
            # URL analysis
            'has_urls': bool(re.search(r'http[s]?://', email_text)),
            'has_suspicious_urls': bool(re.search(r'http[s]?://(?!www\.(google|microsoft|amazon|apple|facebook|linkedin|github)\.com)', email_text)),
            'suspicious_domain': bool(re.search(r'\.(tk|ml|ga|cf|gq|buzz|top)(/|$|\s)', email_text, re.IGNORECASE)),
            'url_count': len(re.findall(r'http[s]?://', email_text)),
            
            # Phishing indicators
            'has_urgency': bool(re.search(
                r'\b(urgent|immediately|act now|right now|asap|within \d+|expire|expiring|limited time|last chance|hurry|verify now|confirm now|suspended|locked|blocked|unusual activity|unauthorized)\b',
                text_lower
            )),
            'has_money_words': bool(re.search(
                r'\b(free money|cash|prize|winner|lottery|million|thousand dollars|claim|reward|refund|payment|transfer)\b',
                text_lower
            )),
            'generic_greeting': bool(re.search(
                r'\b(dear (customer|user|member|sir|madam)|hello there|greetings)\b',
                text_lower
            )),
            
            # Suspicious patterns
            'has_typos': self._detect_typos(text_lower),
            'has_threats': bool(re.search(
                r'\b(suspend|close|terminate|restrict|disable|lock|legal action|account.*close|permanent.*closure)\b',
                text_lower
            )),
            'requests_click': bool(re.search(
                r'\b(click here|click below|click this|follow this|visit this|go to|confirm your|verify your|update your|validate your)\b',
                text_lower
            )),
            'requests_info': bool(re.search(
                r'\b(enter your|provide your|confirm your|verify your|send us|give us|password|ssn|social security|credit card|bank account|pin)\b',
                text_lower
            )),
            
            # Email structure
            'short_message': len(email_text.split()) < 30,
            'no_signature': not bool(re.search(r'\b(regards|sincerely|best|thanks|cheers)\b', text_lower)),
        }
        
        return features
    
    def _detect_typos(self, text):
        """Detect leetspeak and common obfuscation"""
        # Leetspeak patterns
        leetspeak = ['cl1ck', 'f0ll0w', 'acc0unt', 'l0gin', 'em@il', 'p@ssword', 'w1n', 'fr33', 'm0ney']
        return any(typo in text for typo in leetspeak)
    
    def calculate_threat_score(self, ml_label, ml_confidence, features):
        """
        Calculate threat score (0-10) based on ML prediction + manual features
        
        ML model gives us spam/ham classification
        We enhance it with feature analysis
        """
        
        # Base score from ML model
        if ml_label.upper() == "SPAM" or ml_label == "LABEL_1":
            base_score = ml_confidence * 10
        else:
            base_score = (1 - ml_confidence) * 10
        
        # Boost based on features (max +3 points)
        boost = 0
        
        if features['has_suspicious_urls']:
            boost += 1.5
        if features['has_urgency']:
            boost += 1.0
        if features['has_threats']:
            boost += 1.0
        if features['requests_info']:
            boost += 1.5
        if features['generic_greeting']:
            boost += 0.5
        if features['suspicious_domain']:
            boost += 2.0
        if features['has_typos']:
            boost += 0.8
        if features['requests_click'] and features['has_urls']:
            boost += 0.7
        if features['has_money_words']:
            boost += 0.5
        
        # Calculate final score
        final_score = min(base_score + boost, 10.0)
        
        return final_score
    
    def generate_risk_factors(self, features):
        """Generate human-readable list of risk factors"""
        risk_factors = []
        
        if features['has_urgency']:
            risk_factors.append("⚠️ Urgency language detected (pressure tactics)")
        if features['has_threats']:
            risk_factors.append("⚠️ Threatening language (account suspension/closure)")
        if features['has_suspicious_urls']:
            risk_factors.append("⚠️ Suspicious URL detected")
        if features['suspicious_domain']:
            risk_factors.append("⚠️ Suspicious domain extension (.tk, .ml, etc.)")
        if features['generic_greeting']:
            risk_factors.append("⚠️ Generic greeting (not personalized)")
        if features['requests_info']:
            risk_factors.append("🚨 Requests personal information")
        if features['requests_click']:
            risk_factors.append("⚠️ Requests user to click link")
        if features['has_typos']:
            risk_factors.append("⚠️ Suspicious character substitution (leetspeak)")
        if features['has_money_words']:
            risk_factors.append("⚠️ Money/prize-related content")
        if features['url_count'] > 3:
            risk_factors.append(f"⚠️ Multiple URLs ({features['url_count']})")
        if features['no_signature']:
            risk_factors.append("⚠️ Missing professional signature")
        
        if not risk_factors:
            risk_factors.append("✓ No obvious red flags detected")
        
        return risk_factors
    
    def generate_reasoning(self, is_phishing, threat_score, features, ml_confidence):
        """Generate AI reasoning explanation"""
        
        if is_phishing:
            # Count high-risk features
            high_risk_count = sum([
                features['has_threats'],
                features['requests_info'],
                features['suspicious_domain'],
                features['has_suspicious_urls']
            ])
            
            if high_risk_count >= 2:
                severity = "highly likely"
            elif threat_score >= 7:
                severity = "likely"
            else:
                severity = "possibly"
            
            reasons = []
            if features['has_urgency']:
                reasons.append("uses urgent/pressuring language")
            if features['has_threats']:
                reasons.append("contains threats or warnings")
            if features['requests_info']:
                reasons.append("requests sensitive personal information")
            if features['has_suspicious_urls']:
                reasons.append("contains suspicious URLs")
            if features['generic_greeting']:
                reasons.append("uses generic greetings instead of personalization")
            
            if reasons:
                reason_text = ", ".join(reasons[:3])  # Top 3 reasons
                return f"This email is {severity} a phishing attempt because it {reason_text}. ML model confidence: {ml_confidence:.1%}."
            else:
                return f"This email exhibits characteristics typical of phishing attempts (ML confidence: {ml_confidence:.1%})."
        
        else:
            return f"This email appears to be legitimate. No significant phishing indicators detected. ML model confidence: {ml_confidence:.1%}."
    
    def analyze(self, email_text):
        """Main analysis function"""
        start_time = time.time()
        
        if not email_text or len(email_text.strip()) < 10:
            return {
                'error': 'Email text too short or empty',
                'is_phishing': False,
                'confidence': 0.0,
                'threat_score': 0.0,
                'risk_factors': [],
                'reasoning': 'Invalid input',
                'processing_time_ms': 0
            }
        
        try:
            # Extract manual features
            features = self.extract_features(email_text)
            
            # ML classification (limit to 512 characters for speed)
            ml_input = email_text[:512]
            ml_result = self.classifier(ml_input)[0]
            
            ml_label = ml_result['label']
            ml_confidence = ml_result['score']
            
            # Determine if phishing
            # Model outputs SPAM or HAM (or LABEL_0/LABEL_1)
            is_spam = ml_label.upper() in ["SPAM", "LABEL_1"]
            
            # Calculate threat score
            threat_score = self.calculate_threat_score(ml_label, ml_confidence, features)
            
            # If threat score is high but ML said ham, override
            if threat_score >= 6.0:
                is_phishing = True
            else:
                is_phishing = is_spam
            
            # Generate risk factors
            risk_factors = self.generate_risk_factors(features)
            
            # Generate reasoning
            reasoning = self.generate_reasoning(is_phishing, threat_score, features, ml_confidence)
            
            # Calculate processing time
            processing_time = int((time.time() - start_time) * 1000)
            
            # Build result
            result = {
                'is_phishing': bool(is_phishing),
                'confidence': float(ml_confidence),
                'threat_score': float(threat_score),
                'risk_factors': risk_factors,
                'reasoning': reasoning,
                'processing_time_ms': processing_time,
                'ml_label': ml_label,
                'features': features
            }
            
            return result
            
        except Exception as e:
            print(f"❌ Analysis error: {e}", file=sys.stderr)
            return {
                'error': str(e),
                'is_phishing': False,
                'confidence': 0.0,
                'threat_score': 0.0,
                'risk_factors': ['Error during analysis'],
                'reasoning': f'Analysis failed: {str(e)}',
                'processing_time_ms': 0
            }


def main():
    """CLI entry point"""
    if len(sys.argv) < 2:
        print(json.dumps({
            'error': 'No input provided',
            'usage': 'python analyzer.py "<email_text>"'
        }))
        sys.exit(1)
    
    email_text = sys.argv[1]
    
    # Initialize analyzer (loads model)
    analyzer = PhishingAnalyzer()
    
    # Analyze
    result = analyzer.analyze(email_text)
    
    # Output as JSON to stdout
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
