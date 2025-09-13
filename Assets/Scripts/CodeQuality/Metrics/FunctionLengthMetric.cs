using System;
using System.Collections.Generic;
using System.Linq;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 函数长度指标
    /// </summary>
    public class FunctionLengthMetric : BaseMetric
    {
        public override string Name => "函数长度";
        public override string Description => "测量函数的长度，过长的函数难以理解和维护";
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
            
            var functions = parseResult.functions;
            var totalLines = 0;
            var longFunctions = new List<string>();
            var maxLength = 0;
            var functionCount = functions.Count;
            
            foreach (var function in functions)
            {
                var lineCount = CountLines(function.body);
                totalLines += lineCount;
                
                if (lineCount > 50) // 阈值
                {
                    longFunctions.Add($"{function.name} ({lineCount} 行)");
                }
                
                maxLength = Math.Max(maxLength, lineCount);
            }
            
            // 计算分数 (0-1，越高越差)
            float score = 0f;
            if (functionCount > 0)
            {
                var avgLength = (float)totalLines / functionCount;
                score = Math.Min(avgLength / 100f, 1f); // 100 行作为满分阈值
            }
            
            var issues = new List<string>();
            if (maxLength > 100)
            {
                issues.Add($"发现极长函数: {maxLength} 行");
            }
            if (longFunctions.Count > 0)
            {
                issues.Add($"发现 {longFunctions.Count} 个过长函数 (>50行)");
            }
            if (functionCount > 0)
            {
                var avgLength = (float)totalLines / functionCount;
                if (avgLength > 30)
                {
                    issues.Add($"平均函数长度过高: {avgLength:F1} 行");
                }
            }
            
            var result = CreateResult(Name, score, Description, Weight);
            result.issues.AddRange(issues);
            
            return result;
        }
        
        /// <summary>
        /// 计算代码行数
        /// </summary>
        private int CountLines(string code)
        {
            if (string.IsNullOrEmpty(code))
                return 0;
                
            var lines = code.Split('\n');
            var count = 0;
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("//") && !trimmed.StartsWith("/*"))
                {
                    count++;
                }
            }
            
            return count;
        }
    }
}
