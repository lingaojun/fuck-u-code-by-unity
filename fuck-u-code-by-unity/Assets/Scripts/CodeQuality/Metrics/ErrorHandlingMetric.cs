using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 错误处理指标
    /// </summary>
    public class ErrorHandlingMetric : BaseMetric
    {
        public override string Name => "错误处理";
        public override string Description => "检查代码中的错误处理机制";
        public override float Weight => 0.1f;
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
            var totalFunctions = functions.Count;
            var functionsWithErrorHandling = 0;
            var issues = new List<string>();
            
            foreach (var function in functions)
            {
                if (HasErrorHandling(function, parseResult.language))
                {
                    functionsWithErrorHandling++;
                }
            }
            
            // 计算分数 (0-1，越高越差)
            float score = 0f;
            if (totalFunctions > 0)
            {
                var errorHandlingRatio = (float)functionsWithErrorHandling / totalFunctions;
                score = 1f - errorHandlingRatio; // 错误处理越多，分数越低
            }
            
            // 添加问题
            if (totalFunctions > 0)
            {
                var errorHandlingRatio = (float)functionsWithErrorHandling / totalFunctions;
                if (errorHandlingRatio < 0.3f)
                {
                    issues.Add($"错误处理覆盖率过低: {errorHandlingRatio:P1}");
                }
            }
            
            var result = CreateResult(Name, score, Description, Weight);
            result.issues.AddRange(issues);
            
            return result;
        }
        
        /// <summary>
        /// 检查函数是否有错误处理
        /// </summary>
        private bool HasErrorHandling(FunctionInfo function, LanguageType language)
        {
            var body = function.body;
            var patterns = GetErrorHandlingPatterns(language);
            
            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(body, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取错误处理模式
        /// </summary>
        private string[] GetErrorHandlingPatterns(LanguageType language)
        {
            switch (language)
            {
                case LanguageType.CSharp:
                    return new[]
                    {
                        @"\btry\s*\{", @"\bcatch\s*\(", @"\bfinally\s*\{",
                        @"\bthrow\s+", @"\breturn\s+.*\?.*:", @"\bif\s*\(.*==\s*null\)"
                    };
                    
                case LanguageType.Java:
                    return new[]
                    {
                        @"\btry\s*\{", @"\bcatch\s*\(", @"\bfinally\s*\{",
                        @"\bthrow\s+", @"\bthrows\s+", @"\bif\s*\(.*==\s*null\)"
                    };
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    return new[]
                    {
                        @"\btry\s*\{", @"\bcatch\s*\(", @"\bfinally\s*\{",
                        @"\bthrow\s+", @"\bif\s*\(.*==\s*null\)", @"\bif\s*\(.*===\s*undefined\)"
                    };
                    
                case LanguageType.Python:
                    return new[]
                    {
                        @"\btry\s*:", @"\bexcept\s+", @"\bfinally\s*:",
                        @"\braise\s+", @"\bif\s+.*\s+is\s+None", @"\bif\s+.*\s+is\s+not\s+None"
                    };
                    
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    return new[]
                    {
                        @"\btry\s*\{", @"\bcatch\s*\(", @"\bif\s*\(.*==\s*NULL\)",
                        @"\bif\s*\(.*==\s*nullptr\)", @"\breturn\s+.*\?.*:"
                    };
                    
                case LanguageType.Go:
                    return new[]
                    {
                        @"\bif\s+.*\s+!=\s+nil", @"\bdefer\s+", @"\bpanic\s*\(",
                        @"\brecover\s*\(", @"\bif\s+.*\s+==\s+nil"
                    };
                    
                default:
                    return new[] { @"\btry\s*", @"\bcatch\s*", @"\bif\s*\(.*==\s*null\)" };
            }
        }
    }
}
