using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeQuality.Common;
using CodeQuality.Metrics;
using CodeQuality.Parser;

namespace CodeQuality.Analyzer
{
    /// <summary>
    /// 代码分析器
    /// </summary>
    public class CodeAnalyzer
    {
        private readonly List<IMetric> metrics;
        private readonly ITranslator translator;
        private readonly ICodeParser parser;
        
        public CodeAnalyzer(ITranslator translator = null)
        {
            this.translator = translator ?? new DefaultTranslator();
            this.parser = new GenericCodeParser();
            this.metrics = CreateMetrics();
        }
        
        /// <summary>
        /// 分析单个文件
        /// </summary>
        public AnalysisResult AnalyzeFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"文件未找到: {filePath}");
            }
            
            var content = File.ReadAllText(filePath);
            var language = DetectLanguage(filePath);
            var parseResult = parser.Parse(filePath, content, language);
            
            return AnalyzeParseResult(parseResult);
        }
        
        /// <summary>
        /// 分析目录
        /// </summary>
        public AnalysisResult AnalyzeDirectory(string directoryPath, CodeQualityConfig config = null)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"目录未找到: {directoryPath}");
            }
            
            config = config ?? new CodeQualityConfig();
            var files = FindSourceFiles(directoryPath, config);
            
            var result = new AnalysisResult();
            var startTime = DateTime.Now;
            
            foreach (var file in files)
            {
                try
                {
                    var fileResult = AnalyzeFile(file);
                    result.AddFileResult(fileResult.filesAnalyzed[0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"分析文件失败 {file}: {ex.Message}");
                }
            }
            
            result.analysisTime = (float)(DateTime.Now - startTime).TotalSeconds;
            result.CalculateOverallScore();
            result.CalculateIssueStats();
            
            return result;
        }
        
        /// <summary>
        /// 分析解析结果
        /// </summary>
        private AnalysisResult AnalyzeParseResult(ParseResult parseResult)
        {
            var result = new AnalysisResult();
            
            // 添加文件信息
            var fileResult = new FileAnalysisResult(parseResult.filePath)
            {
                totalLines = parseResult.totalLines,
                codeLines = parseResult.codeLines,
                commentLines = parseResult.commentLines,
                blankLines = parseResult.blankLines
            };
            
            // 应用每个指标
            foreach (var metric in metrics)
            {
                if (IsLanguageSupported(metric, parseResult.language))
                {
                    var metricResult = metric.Analyze(parseResult);
                    result.AddMetricResult(metric.Name, metricResult);
                    fileResult.AddMetricScore(metric.Name, metricResult.score);
                    
                    // 添加问题到文件结果
                    foreach (var issue in metricResult.issues)
                    {
                        fileResult.AddIssue(issue, DetermineIssueLevel(metricResult.score));
                    }
                }
            }
            
            // 计算文件评分
            fileResult.fileScore = CalculateFileScore(fileResult.metricScores);
            
            result.AddFileResult(fileResult);
            result.CalculateOverallScore();
            result.CalculateIssueStats();
            
            return result;
        }
        
        /// <summary>
        /// 检测文件语言类型
        /// </summary>
        private LanguageType DetectLanguage(string filePath)
        {
            var detector = new DefaultLanguageDetector();
            return detector.DetectLanguage(filePath);
        }
        
        /// <summary>
        /// 查找源码文件
        /// </summary>
        private List<string> FindSourceFiles(string directoryPath, CodeQualityConfig config)
        {
            var files = new List<string>();
            var detector = new DefaultLanguageDetector();
            
            var allFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            
            foreach (var file in allFiles)
            {
                if (detector.IsSupportedFile(file) && !ShouldExcludeFile(file, config))
                {
                    files.Add(file);
                }
            }
            
            return files;
        }
        
        /// <summary>
        /// 检查是否应该排除文件
        /// </summary>
        private bool ShouldExcludeFile(string filePath, CodeQualityConfig config)
        {
            foreach (var pattern in config.excludePatterns)
            {
                if (MatchesPattern(filePath, pattern))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查文件路径是否匹配模式
        /// </summary>
        private bool MatchesPattern(string filePath, string pattern)
        {
            // 简单的通配符匹配实现
            var normalizedPath = filePath.Replace("\\", "/");
            var normalizedPattern = pattern.Replace("\\", "/");
            
            if (normalizedPattern.Contains("**"))
            {
                var parts = normalizedPattern.Split(new[] { "**" }, StringSplitOptions.None);
                return normalizedPath.Contains(parts[0]) && normalizedPath.Contains(parts[1]);
            }
            
            return normalizedPath.Contains(normalizedPattern);
        }
        
        /// <summary>
        /// 检查指标是否支持指定语言
        /// </summary>
        private bool IsLanguageSupported(IMetric metric, LanguageType language)
        {
            var supportedLanguages = metric.SupportedLanguages;
            if (supportedLanguages == null || supportedLanguages.Length == 0)
                return true;
                
            return supportedLanguages.Contains(language);
        }
        
        /// <summary>
        /// 确定问题级别
        /// </summary>
        private IssueLevel DetermineIssueLevel(float score)
        {
            if (score >= 0.8f)
                return IssueLevel.Critical;
            else if (score >= 0.5f)
                return IssueLevel.Warning;
            else
                return IssueLevel.Info;
        }
        
        /// <summary>
        /// 计算文件评分
        /// </summary>
        private float CalculateFileScore(Dictionary<string, float> metricScores)
        {
            if (metricScores.Count == 0)
                return 0f;
                
            var totalScore = 0f;
            var totalWeight = 0f;
            
            foreach (var kvp in metricScores)
            {
                var weight = GetMetricWeight(kvp.Key);
                totalScore += kvp.Value * weight;
                totalWeight += weight;
            }
            
            return totalWeight > 0 ? totalScore / totalWeight : 0f;
        }
        
        /// <summary>
        /// 获取指标权重
        /// </summary>
        private float GetMetricWeight(string metricName)
        {
            // 默认权重，实际应该从配置中获取
            var weights = new Dictionary<string, float>
            {
                { "循环复杂度", 0.2f },
                { "函数长度", 0.15f },
                { "注释覆盖率", 0.1f },
                { "错误处理", 0.1f },
                { "命名规范", 0.15f },
                { "代码重复度", 0.15f },
                { "代码结构分析", 0.15f }
            };
            
            return weights.TryGetValue(metricName, out var weight) ? weight : 0.1f;
        }
        
        /// <summary>
        /// 创建所有指标
        /// </summary>
        private List<IMetric> CreateMetrics()
        {
            var metrics = new List<IMetric>
            {
                new CyclomaticComplexityMetric(),
                new FunctionLengthMetric(),
                new CommentRatioMetric(),
                new ErrorHandlingMetric(),
                new NamingConventionMetric(),
                new CodeDuplicationMetric(),
                new StructureAnalysisMetric()
            };
            
            // 设置翻译器
            foreach (var metric in metrics)
            {
                metric.SetTranslator(translator);
            }
            
            return metrics;
        }
    }
}
