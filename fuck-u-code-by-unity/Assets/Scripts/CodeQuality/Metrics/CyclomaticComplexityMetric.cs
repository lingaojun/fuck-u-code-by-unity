using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 循环复杂度指标
    /// </summary>
    public class CyclomaticComplexityMetric : BaseMetric
    {
        public override string Name => "循环复杂度";
        public override string Description => "测量代码的复杂程度，基于控制流语句的数量";
        public override float Weight => 0.2f;
        public override LanguageType[] SupportedLanguages => new[] 
        { 
            LanguageType.CSharp, 
            LanguageType.JavaScript, 
            LanguageType.TypeScript, 
            LanguageType.Python, 
            LanguageType.Java, 
            LanguageType.CPlusPlus, 
            LanguageType.C, 
            LanguageType.Go 
        };
        
        public override MetricResult Analyze(ParseResult parseResult)
        {
            if (!IsLanguageSupported(parseResult.language))
            {
                return CreateResult(Name, 0f, Description, Weight);
            }
            
            var functions = parseResult.functions;
            var totalComplexity = 0;
            var complexFunctions = new List<string>();
            var maxComplexity = 0;
            
            foreach (var function in functions)
            {
                var complexity = CalculateComplexity(function, parseResult.language);
                totalComplexity += complexity;
                
                if (complexity > 10) // 阈值
                {
                    complexFunctions.Add($"{function.name} (复杂度: {complexity})");
                }
                
                maxComplexity = Math.Max(maxComplexity, complexity);
            }
            
            // 计算分数 (0-1，越高越差)
            float score = 0f;
            if (functions.Count > 0)
            {
                var avgComplexity = (float)totalComplexity / functions.Count;
                score = Math.Min(avgComplexity / 15f, 1f); // 15 作为满分阈值
            }
            
            var issues = new List<string>();
            if (maxComplexity > 20)
            {
                issues.Add($"发现极高复杂度函数: {maxComplexity}");
            }
            if (complexFunctions.Count > 0)
            {
                issues.Add($"发现 {complexFunctions.Count} 个高复杂度函数");
            }
            
            var result = CreateResult(Name, score, Description, Weight);
            result.issues.AddRange(issues);
            
            return result;
        }
        
        /// <summary>
        /// 计算函数的循环复杂度
        /// </summary>
        private int CalculateComplexity(FunctionInfo function, LanguageType language)
        {
            var code = function.body;
            var complexity = 1; // 基础复杂度
            
            // 根据语言类型使用不同的正则表达式
            var patterns = GetComplexityPatterns(language);
            
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(code, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                complexity += matches.Count;
            }
            
            return complexity;
        }
        
        /// <summary>
        /// 获取复杂度计算的正则表达式模式
        /// </summary>
        private string[] GetComplexityPatterns(LanguageType language)
        {
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    return new[]
                    {
                        @"\bif\s*\(", @"\belse\s+if\s*\(", @"\bwhile\s*\(", @"\bfor\s*\(",
                        @"\bforeach\s*\(", @"\bswitch\s*\(", @"\bcase\s+", @"\bdefault\s*:",
                        @"\bcatch\s*\(", @"\b&&\b", @"\b\|\|\b", @"\?\s*.*\s*:"
                    };
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    return new[]
                    {
                        @"\bif\s*\(", @"\belse\s+if\s*\(", @"\bwhile\s*\(", @"\bfor\s*\(",
                        @"\bswitch\s*\(", @"\bcase\s+", @"\bdefault\s*:", @"\bcatch\s*\(",
                        @"\b&&\b", @"\b\|\|\b", @"\?\s*.*\s*:", @"\bdo\s*\{"
                    };
                    
                case LanguageType.Python:
                    return new[]
                    {
                        @"\bif\s+", @"\belif\s+", @"\bwhile\s+", @"\bfor\s+",
                        @"\btry\s*:", @"\bexcept\s+", @"\belse\s*:", @"\bfinally\s*:",
                        @"\band\b", @"\bor\b", @"\?\s*.*\s*:"
                    };
                    
                case LanguageType.Go:
                    return new[]
                    {
                        @"\bif\s+", @"\belse\s+if\s+", @"\bfor\s+", @"\bswitch\s+",
                        @"\bcase\s+", @"\bdefault\s*:", @"\bselect\s+", @"\bgo\s+",
                        @"\bdefer\s+", @"\b&&\b", @"\b\|\|\b", @"\?\s*.*\s*:"
                    };
                    
                default:
                    return new[] { @"\bif\s*", @"\bwhile\s*", @"\bfor\s*", @"\bswitch\s*" };
            }
        }
    }
}
