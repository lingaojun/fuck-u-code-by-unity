using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 命名规范指标
    /// </summary>
    public class NamingConventionMetric : BaseMetric
    {
        public override string Name => "命名规范";
        public override string Description => "检查代码中的命名是否符合规范";
        public override float Weight => 0.15f;
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
            
            var issues = new List<string>();
            var totalItems = 0;
            var validItems = 0;
            
            // 检查函数命名
            foreach (var function in parseResult.functions)
            {
                totalItems++;
                if (IsValidFunctionName(function.name, parseResult.language))
                {
                    validItems++;
                }
                else
                {
                    issues.Add($"函数命名不规范: {function.name}");
                }
            }
            
            // 检查类命名
            foreach (var classInfo in parseResult.classes)
            {
                totalItems++;
                if (IsValidClassName(classInfo.name, parseResult.language))
                {
                    validItems++;
                }
                else
                {
                    issues.Add($"类命名不规范: {classInfo.name}");
                }
            }
            
            // 检查变量命名
            foreach (var variable in parseResult.variables)
            {
                totalItems++;
                if (IsValidVariableName(variable.name, parseResult.language))
                {
                    validItems++;
                }
                else
                {
                    issues.Add($"变量命名不规范: {variable.name}");
                }
            }
            
            // 计算分数 (0-1，越高越差)
            float score = 0f;
            if (totalItems > 0)
            {
                var validRatio = (float)validItems / totalItems;
                score = 1f - validRatio; // 规范命名越多，分数越低
            }
            
            var result = CreateResult(Name, score, Description, Weight);
            result.issues.AddRange(issues);
            
            return result;
        }
        
        /// <summary>
        /// 检查函数命名是否规范
        /// </summary>
        private bool IsValidFunctionName(string name, LanguageType language)
        {
            if (string.IsNullOrEmpty(name))
                return false;
                
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                    // PascalCase
                    return Regex.IsMatch(name, @"^[A-Z][a-zA-Z0-9]*$");
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    // camelCase
                    return Regex.IsMatch(name, @"^[a-z][a-zA-Z0-9]*$");
                    
                case LanguageType.Python:
                    // snake_case
                    return Regex.IsMatch(name, @"^[a-z][a-z0-9_]*$");
                    
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    // snake_case 或 camelCase
                    return Regex.IsMatch(name, @"^[a-z][a-zA-Z0-9_]*$");
                    
                case LanguageType.Go:
                    // PascalCase (public) 或 camelCase (private)
                    return Regex.IsMatch(name, @"^[A-Z][a-zA-Z0-9]*$") || 
                           Regex.IsMatch(name, @"^[a-z][a-zA-Z0-9]*$");
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 检查类命名是否规范
        /// </summary>
        private bool IsValidClassName(string name, LanguageType language)
        {
            if (string.IsNullOrEmpty(name))
                return false;
                
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                case LanguageType.Go:
                    // PascalCase
                    return Regex.IsMatch(name, @"^[A-Z][a-zA-Z0-9]*$");
                    
                case LanguageType.Python:
                    // PascalCase
                    return Regex.IsMatch(name, @"^[A-Z][a-zA-Z0-9]*$");
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    // PascalCase
                    return Regex.IsMatch(name, @"^[A-Z][a-zA-Z0-9]*$");
                    
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    // PascalCase 或 snake_case
                    return Regex.IsMatch(name, @"^[A-Z][a-zA-Z0-9]*$") || 
                           Regex.IsMatch(name, @"^[a-z][a-z0-9_]*$");
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 检查变量命名是否规范
        /// </summary>
        private bool IsValidVariableName(string name, LanguageType language)
        {
            if (string.IsNullOrEmpty(name))
                return false;
                
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                    // camelCase
                    return Regex.IsMatch(name, @"^[a-z][a-zA-Z0-9]*$");
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    // camelCase
                    return Regex.IsMatch(name, @"^[a-z][a-zA-Z0-9]*$");
                    
                case LanguageType.Python:
                    // snake_case
                    return Regex.IsMatch(name, @"^[a-z][a-z0-9_]*$");
                    
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    // snake_case 或 camelCase
                    return Regex.IsMatch(name, @"^[a-z][a-zA-Z0-9_]*$");
                    
                case LanguageType.Go:
                    // camelCase (private) 或 PascalCase (public)
                    return Regex.IsMatch(name, @"^[a-z][a-zA-Z0-9]*$") || 
                           Regex.IsMatch(name, @"^[A-Z][a-zA-Z0-9]*$");
                    
                default:
                    return true;
            }
        }
    }
}
