using System;
using System.Collections.Generic;
using System.Linq;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 代码结构分析指标
    /// </summary>
    public class StructureAnalysisMetric : BaseMetric
    {
        public override string Name => "代码结构分析";
        public override string Description => "分析代码的整体结构质量";
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
            var score = 0f;
            
            // 分析类结构
            var classScore = AnalyzeClassStructure(parseResult.classes, issues);
            
            // 分析函数结构
            var functionScore = AnalyzeFunctionStructure(parseResult.functions, issues);
            
            // 分析文件结构
            var fileScore = AnalyzeFileStructure(parseResult, issues);
            
            // 计算总体分数
            score = (classScore + functionScore + fileScore) / 3f;
            
            var result = CreateResult(Name, score, Description, Weight);
            result.issues.AddRange(issues);
            
            return result;
        }
        
        /// <summary>
        /// 分析类结构
        /// </summary>
        private float AnalyzeClassStructure(List<ClassInfo> classes, List<string> issues)
        {
            if (classes.Count == 0)
                return 0f;
                
            var score = 0f;
            var totalClasses = classes.Count;
            var validClasses = 0;
            
            foreach (var classInfo in classes)
            {
                var classScore = 1f;
                
                // 检查类长度
                var classLength = classInfo.endLine - classInfo.startLine;
                if (classLength > 500)
                {
                    classScore -= 0.3f;
                    issues.Add($"类过长: {classInfo.name} ({classLength} 行)");
                }
                
                // 检查是否有基类或接口
                if (classInfo.baseClasses.Count == 0 && classInfo.interfaces.Count == 0)
                {
                    classScore -= 0.1f;
                }
                
                // 检查访问修饰符
                if (classInfo.accessModifier == AccessModifier.None)
                {
                    classScore -= 0.2f;
                    issues.Add($"类缺少访问修饰符: {classInfo.name}");
                }
                
                score += classScore;
                if (classScore > 0.7f)
                    validClasses++;
            }
            
            return score / totalClasses;
        }
        
        /// <summary>
        /// 分析函数结构
        /// </summary>
        private float AnalyzeFunctionStructure(List<FunctionInfo> functions, List<string> issues)
        {
            if (functions.Count == 0)
                return 0f;
                
            var score = 0f;
            var totalFunctions = functions.Count;
            var validFunctions = 0;
            
            foreach (var function in functions)
            {
                var functionScore = 1f;
                
                // 检查函数长度
                var functionLength = function.endLine - function.startLine;
                if (functionLength > 100)
                {
                    functionScore -= 0.4f;
                    issues.Add($"函数过长: {function.name} ({functionLength} 行)");
                }
                
                // 检查参数数量
                if (function.parameterCount > 5)
                {
                    functionScore -= 0.3f;
                    issues.Add($"函数参数过多: {function.name} ({function.parameterCount} 个参数)");
                }
                
                // 检查返回类型
                if (string.IsNullOrEmpty(function.returnType))
                {
                    functionScore -= 0.1f;
                }
                
                // 检查访问修饰符
                if (function.accessModifier == AccessModifier.None)
                {
                    functionScore -= 0.2f;
                    issues.Add($"函数缺少访问修饰符: {function.name}");
                }
                
                score += functionScore;
                if (functionScore > 0.7f)
                    validFunctions++;
            }
            
            return score / totalFunctions;
        }
        
        /// <summary>
        /// 分析文件结构
        /// </summary>
        private float AnalyzeFileStructure(ParseResult parseResult, List<string> issues)
        {
            var score = 1f;
            
            // 检查文件长度
            if (parseResult.totalLines > 1000)
            {
                score -= 0.3f;
                issues.Add($"文件过长: {parseResult.totalLines} 行");
            }
            
            // 检查类数量
            if (parseResult.classes.Count > 10)
            {
                score -= 0.2f;
                issues.Add($"文件中的类过多: {parseResult.classes.Count} 个");
            }
            
            // 检查函数数量
            if (parseResult.functions.Count > 50)
            {
                score -= 0.2f;
                issues.Add($"文件中的函数过多: {parseResult.functions.Count} 个");
            }
            
            // 检查注释比例
            var commentRatio = (float)parseResult.commentLines / parseResult.totalLines;
            if (commentRatio < 0.1f)
            {
                score -= 0.1f;
                issues.Add($"文件注释不足: {commentRatio:P1}");
            }
            
            // 检查空行比例
            var blankRatio = (float)parseResult.blankLines / parseResult.totalLines;
            if (blankRatio < 0.05f)
            {
                score -= 0.1f;
                issues.Add($"文件空行不足: {blankRatio:P1}");
            }
            
            return Math.Max(score, 0f);
        }
    }
}
