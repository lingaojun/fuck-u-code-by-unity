using System;
using System.Collections.Generic;
using System.Linq;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 代码重复度指标
    /// </summary>
    public class CodeDuplicationMetric : BaseMetric
    {
        public override string Name => "代码重复度";
        public override string Description => "检查代码中的重复部分";
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
            var totalFunctions = functions.Count;
            var duplicateFunctions = 0;
            var issues = new List<string>();
            
            // 检查函数重复
            for (int i = 0; i < functions.Count; i++)
            {
                for (int j = i + 1; j < functions.Count; j++)
                {
                    var similarity = CalculateSimilarity(functions[i].body, functions[j].body);
                    if (similarity > 0.8f) // 80% 相似度阈值
                    {
                        duplicateFunctions++;
                        issues.Add($"函数重复: {functions[i].name} 和 {functions[j].name} (相似度: {similarity:P1})");
                    }
                }
            }
            
            // 检查代码块重复
            var duplicateBlocks = FindDuplicateBlocks(parseResult.content);
            if (duplicateBlocks.Count > 0)
            {
                issues.Add($"发现 {duplicateBlocks.Count} 个重复代码块");
            }
            
            // 计算分数 (0-1，越高越差)
            float score = 0f;
            if (totalFunctions > 0)
            {
                var duplicationRatio = (float)duplicateFunctions / totalFunctions;
                score = Math.Min(duplicationRatio * 2f, 1f); // 重复函数越多，分数越高
            }
            
            // 添加重复代码块的影响
            if (duplicateBlocks.Count > 0)
            {
                score = Math.Min(score + 0.3f, 1f);
            }
            
            var result = CreateResult(Name, score, Description, Weight);
            result.issues.AddRange(issues);
            
            return result;
        }
        
        /// <summary>
        /// 计算两个字符串的相似度
        /// </summary>
        private float CalculateSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0f;
                
            var longer = str1.Length > str2.Length ? str1 : str2;
            var shorter = str1.Length > str2.Length ? str2 : str1;
            
            if (longer.Length == 0)
                return 1f;
                
            var distance = LevenshteinDistance(longer, shorter);
            return (longer.Length - distance) / (float)longer.Length;
        }
        
        /// <summary>
        /// 计算编辑距离
        /// </summary>
        private int LevenshteinDistance(string str1, string str2)
        {
            var matrix = new int[str1.Length + 1, str2.Length + 1];
            
            for (int i = 0; i <= str1.Length; i++)
            {
                matrix[i, 0] = i;
            }
            
            for (int j = 0; j <= str2.Length; j++)
            {
                matrix[0, j] = j;
            }
            
            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            
            return matrix[str1.Length, str2.Length];
        }
        
        /// <summary>
        /// 查找重复代码块
        /// </summary>
        private List<string> FindDuplicateBlocks(string content)
        {
            var lines = content.Split('\n');
            var blocks = new List<string>();
            var duplicates = new List<string>();
            
            // 查找 3 行以上的重复代码块
            for (int i = 0; i < lines.Length - 2; i++)
            {
                for (int length = 3; length <= Math.Min(10, lines.Length - i); length++)
                {
                    var block = string.Join("\n", lines, i, length);
                    if (blocks.Contains(block))
                    {
                        duplicates.Add(block);
                    }
                    else
                    {
                        blocks.Add(block);
                    }
                }
            }
            
            return duplicates;
        }
    }
}
