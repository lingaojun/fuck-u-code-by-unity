using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using CodeQuality.Analyzer;
using CodeQuality.Common;

namespace CodeQuality.Editor
{
    /// <summary>
    /// 代码质量分析窗口
    /// </summary>
    public class CodeQualityWindow : EditorWindow
    {
        private CodeQualityConfig config;
        private AnalysisResult analysisResult;
        private Vector2 scrollPosition;
        private bool isAnalyzing = false;
        private string analysisPath = "Assets";
        
        [MenuItem("Tools/Code Quality Analysis")]
        public static void ShowWindow()
        {
            var window = GetWindow<CodeQualityWindow>("Code Quality Analysis");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        void OnEnable()
        {
            config = new CodeQualityConfig();
        }
        
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            DrawHeader();
            DrawConfig();
            DrawAnalysisButton();
            DrawResults();
            
            EditorGUILayout.EndVertical();
        }
        
        void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("代码质量分析工具", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("分析项目代码质量，发现潜在问题", EditorStyles.helpBox);
            EditorGUILayout.Space(10);
        }
        
        void DrawConfig()
        {
            EditorGUILayout.LabelField("分析配置", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            // 分析路径
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("分析路径:", GUILayout.Width(80));
            analysisPath = EditorGUILayout.TextField(analysisPath);
            if (GUILayout.Button("选择", GUILayout.Width(50)))
            {
                var path = EditorUtility.OpenFolderPanel("选择分析目录", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    analysisPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // 基本设置
            config.verbose = EditorGUILayout.Toggle("详细模式", config.verbose);
            config.summaryOnly = EditorGUILayout.Toggle("仅显示摘要", config.summaryOnly);
            config.markdownOutput = EditorGUILayout.Toggle("Markdown 输出", config.markdownOutput);
            
            // 显示设置
            config.topFiles = EditorGUILayout.IntSlider("显示文件数", config.topFiles, 1, 20);
            config.maxIssues = EditorGUILayout.IntSlider("每文件问题数", config.maxIssues, 1, 20);
            
            // 语言设置
            config.reportLanguage = (LanguageType)EditorGUILayout.EnumPopup("报告语言", config.reportLanguage);
            
            // 排除设置
            config.skipIndexFiles = EditorGUILayout.Toggle("跳过 Index 文件", config.skipIndexFiles);
            
            // 排除模式部分已隐藏
            
            //EditorGUILayout.Space(5);
            //EditorGUILayout.LabelField("排除模式:", EditorStyles.boldLabel);
            //EditorGUILayout.BeginVertical("box");
            
            //for (int i = 0; i < config.excludePatterns.Count; i++)
            //{
            //    EditorGUILayout.BeginHorizontal();
            //    config.excludePatterns[i] = EditorGUILayout.TextField(config.excludePatterns[i]);
            //    if (GUILayout.Button("删除", GUILayout.Width(50)))
            //    {
            //        config.excludePatterns.RemoveAt(i);
            //        i--;
            //    }
            //    EditorGUILayout.EndHorizontal();
            //}
            
            //if (GUILayout.Button("添加排除模式"))
            //{
            //    config.excludePatterns.Add("**/new_pattern/**");
            //}
            
            //EditorGUILayout.EndVertical();
            
            
            // 指标权重
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("指标权重:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            config.cyclomaticComplexityWeight = EditorGUILayout.Slider("循环复杂度", config.cyclomaticComplexityWeight, 0f, 1f);
            config.functionLengthWeight = EditorGUILayout.Slider("函数长度", config.functionLengthWeight, 0f, 1f);
            config.commentRatioWeight = EditorGUILayout.Slider("注释覆盖率", config.commentRatioWeight, 0f, 1f);
            config.errorHandlingWeight = EditorGUILayout.Slider("错误处理", config.errorHandlingWeight, 0f, 1f);
            config.namingConventionWeight = EditorGUILayout.Slider("命名规范", config.namingConventionWeight, 0f, 1f);
            config.codeDuplicationWeight = EditorGUILayout.Slider("代码重复度", config.codeDuplicationWeight, 0f, 1f);
            config.structureAnalysisWeight = EditorGUILayout.Slider("代码结构", config.structureAnalysisWeight, 0f, 1f);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }
        
        void DrawAnalysisButton()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !isAnalyzing;
            if (GUILayout.Button(isAnalyzing ? "分析中..." : "开始分析", GUILayout.Height(30)))
            {
                StartAnalysis();
            }
            
            GUI.enabled = analysisResult != null;
            if (GUILayout.Button("导出报告", GUILayout.Height(30), GUILayout.Width(100)))
            {
                ExportReport();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }
        
        void DrawResults()
        {
            if (analysisResult == null)
                return;
                
            EditorGUILayout.LabelField("分析结果", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 总体评分
            DrawOverallScore();
            
            // 指标详情
            DrawMetrics();
            
            // 文件分析
            DrawFileAnalysis();
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        void DrawOverallScore()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("总体评分", EditorStyles.boldLabel);
            
            var score = analysisResult.codeQualityScore;
            var scoreColor = GetScoreColor(score);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("代码质量分数:", GUILayout.Width(120));
            var rect = GUILayoutUtility.GetRect(200, 20);
            EditorGUI.ProgressBar(rect, score, $"{score:P1}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("分析文件数:", GUILayout.Width(120));
            EditorGUILayout.LabelField(analysisResult.totalFiles.ToString());
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("总代码行数:", GUILayout.Width(120));
            EditorGUILayout.LabelField(analysisResult.totalLines.ToString());
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("分析耗时:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{analysisResult.analysisTime:F2} 秒");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        void DrawMetrics()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("指标详情", EditorStyles.boldLabel);
            
            foreach (var metric in analysisResult.metrics.Values)
            {
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(metric.name, EditorStyles.boldLabel);
                var rect = GUILayoutUtility.GetRect(200, 20);
                EditorGUI.ProgressBar(rect, metric.score, $"{metric.score:P1}");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField(metric.description, EditorStyles.helpBox);
                
                if (metric.issues.Count > 0)
                {
                    EditorGUILayout.LabelField("问题:", EditorStyles.boldLabel);
                    foreach (var issue in metric.issues)
                    {
                        EditorGUILayout.LabelField($"• {issue}", EditorStyles.wordWrappedLabel);
                    }
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        void DrawFileAnalysis()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("文件分析", EditorStyles.boldLabel);
            
            var files = analysisResult.filesAnalyzed;
            files.Sort((a, b) => b.fileScore.CompareTo(a.fileScore)); // 按分数降序排列
            
            var displayCount = Math.Min(config.topFiles, files.Count);
            
            for (int i = 0; i < displayCount; i++)
            {
                var file = files[i];
                
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(Path.GetFileName(file.filePath), EditorStyles.boldLabel);
                var rect = GUILayoutUtility.GetRect(200, 20);
                EditorGUI.ProgressBar(rect, file.fileScore, $"{file.fileScore:P1}");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField(file.filePath, EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"代码行: {file.codeLines}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"注释行: {file.commentLines}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"问题数: {file.issues.Count}", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                if (file.issues.Count > 0)
                {
                    EditorGUILayout.LabelField("问题:", EditorStyles.boldLabel);
                    var issueCount = Math.Min(config.maxIssues, file.issues.Count);
                    for (int j = 0; j < issueCount; j++)
                    {
                        EditorGUILayout.LabelField($"• {file.issues[j]}", EditorStyles.wordWrappedLabel);
                    }
                    
                    if (file.issues.Count > config.maxIssues)
                    {
                        EditorGUILayout.LabelField($"... 还有 {file.issues.Count - config.maxIssues} 个问题", EditorStyles.miniLabel);
                    }
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        void StartAnalysis()
        {
            isAnalyzing = true;
            
            try
            {
                var analyzer = new CodeAnalyzer();
                analysisResult = analyzer.AnalyzeDirectory(analysisPath, config);
                
                Debug.Log($"代码质量分析完成！总体评分: {analysisResult.codeQualityScore:P1}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"分析失败: {ex.Message}");
            }
            finally
            {
                isAnalyzing = false;
            }
        }
        
        void ExportReport()
        {
            var path = EditorUtility.SaveFilePanel("导出报告", "", "code_quality_report", "md");
            if (!string.IsNullOrEmpty(path))
            {
                var report = GenerateMarkdownReport();
                File.WriteAllText(path, report);
                Debug.Log($"报告已导出到: {path}");
            }
        }
        
        string GenerateMarkdownReport()
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("# 代码质量分析报告");
            report.AppendLine();
            report.AppendLine($"**分析时间:** {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**分析路径:** {analysisPath}");
            report.AppendLine($"**分析文件数:** {analysisResult.totalFiles}");
            report.AppendLine($"**总代码行数:** {analysisResult.totalLines}");
            report.AppendLine($"**分析耗时:** {analysisResult.analysisTime:F2} 秒");
            report.AppendLine();
            
            // 总体评分
            report.AppendLine("## 总体评分");
            report.AppendLine();
            report.AppendLine($"**代码质量分数:** {analysisResult.codeQualityScore:P1}");
            report.AppendLine();
            
            // 指标详情
            report.AppendLine("## 指标详情");
            report.AppendLine();
            foreach (var metric in analysisResult.metrics.Values)
            {
                report.AppendLine($"### {metric.name}");
                report.AppendLine();
                report.AppendLine($"**分数:** {metric.score:P1}");
                report.AppendLine($"**描述:** {metric.description}");
                report.AppendLine();
                
                if (metric.issues.Count > 0)
                {
                    report.AppendLine("**问题:**");
                    foreach (var issue in metric.issues)
                    {
                        report.AppendLine($"- {issue}");
                    }
                    report.AppendLine();
                }
            }
            
            // 文件分析
            report.AppendLine("## 文件分析");
            report.AppendLine();
            var files = analysisResult.filesAnalyzed;
            files.Sort((a, b) => b.fileScore.CompareTo(a.fileScore));
            
            var displayCount = Math.Min(config.topFiles, files.Count);
            for (int i = 0; i < displayCount; i++)
            {
                var file = files[i];
                report.AppendLine($"### {Path.GetFileName(file.filePath)}");
                report.AppendLine();
                report.AppendLine($"**文件路径:** {file.filePath}");
                report.AppendLine($"**质量分数:** {file.fileScore:P1}");
                report.AppendLine($"**代码行数:** {file.codeLines}");
                report.AppendLine($"**注释行数:** {file.commentLines}");
                report.AppendLine($"**问题数量:** {file.issues.Count}");
                report.AppendLine();
                
                if (file.issues.Count > 0)
                {
                    report.AppendLine("**问题列表:**");
                    var issueCount = Math.Min(config.maxIssues, file.issues.Count);
                    for (int j = 0; j < issueCount; j++)
                    {
                        report.AppendLine($"- {file.issues[j]}");
                    }
                    
                    if (file.issues.Count > config.maxIssues)
                    {
                        report.AppendLine($"- ... 还有 {file.issues.Count - config.maxIssues} 个问题");
                    }
                    report.AppendLine();
                }
            }
            
            return report.ToString();
        }
        
        Color GetScoreColor(float score)
        {
            if (score < 0.3f)
                return Color.green;
            else if (score < 0.6f)
                return Color.yellow;
            else
                return Color.red;
        }
    }
}
