using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 注释覆盖率指标
    /// </summary>
    public class CommentRatioMetric : BaseMetric
    {
        public override string Name => "注释覆盖率";
        public override string Description => "测量代码中注释的比例，适当的注释有助于代码理解";
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
            
            var totalLines = parseResult.totalLines;
            var commentLines = CountCommentLines(parseResult.content, parseResult.language);
            var codeLines = totalLines - commentLines;
            
            // 计算注释比例
            float commentRatio = 0f;
            if (codeLines > 0)
            {
                commentRatio = (float)commentLines / (codeLines + commentLines);
            }
            
            // 计算分数 (0-1，越高越差)
            // 注释比例过低或过高都不好
            float score = 0f;
            if (commentRatio < 0.1f) // 注释太少
            {
                score = 0.8f - commentRatio * 4f; // 0.1 -> 0.4, 0 -> 0.8
            }
            else if (commentRatio > 0.5f) // 注释太多
            {
                score = (commentRatio - 0.5f) * 2f; // 0.5 -> 0, 1.0 -> 1.0
            }
            else // 注释比例适中
            {
                score = Math.Abs(commentRatio - 0.3f) * 2f; // 0.3 为最佳比例
            }
            
            var issues = new List<string>();
            if (commentRatio < 0.1f)
            {
                issues.Add($"注释覆盖率过低: {commentRatio:P1}");
            }
            else if (commentRatio > 0.5f)
            {
                issues.Add($"注释覆盖率过高: {commentRatio:P1}");
            }
            
            var result = CreateResult(Name, score, Description, Weight);
            result.issues.AddRange(issues);
            
            return result;
        }
        
        /// <summary>
        /// 计算注释行数
        /// </summary>
        private int CountCommentLines(string content, LanguageType language)
        {
            if (string.IsNullOrEmpty(content))
                return 0;
                
            var lines = content.Split('\n');
            var commentLines = 0;
            var inBlockComment = false;
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;
                    
                // 根据语言类型判断注释
                switch (language)
                {
                    case LanguageType.CSharp:
                    case LanguageType.Java:
                    case LanguageType.CPlusPlus:
                    case LanguageType.C:
                    case LanguageType.JavaScript:
                    case LanguageType.TypeScript:
                        if (inBlockComment)
                        {
                            commentLines++;
                            if (trimmed.Contains("*/"))
                            {
                                inBlockComment = false;
                            }
                        }
                        else if (trimmed.StartsWith("//"))
                        {
                            commentLines++;
                        }
                        else if (trimmed.StartsWith("/*"))
                        {
                            commentLines++;
                            if (!trimmed.Contains("*/"))
                            {
                                inBlockComment = true;
                            }
                        }
                        break;
                        
                    case LanguageType.Python:
                        if (trimmed.StartsWith("#"))
                        {
                            commentLines++;
                        }
                        break;
                        
                    case LanguageType.Go:
                        if (inBlockComment)
                        {
                            commentLines++;
                            if (trimmed.Contains("*/"))
                            {
                                inBlockComment = false;
                            }
                        }
                        else if (trimmed.StartsWith("//"))
                        {
                            commentLines++;
                        }
                        else if (trimmed.StartsWith("/*"))
                        {
                            commentLines++;
                            if (!trimmed.Contains("*/"))
                            {
                                inBlockComment = true;
                            }
                        }
                        break;
                }
            }
            
            return commentLines;
        }
    }
}
