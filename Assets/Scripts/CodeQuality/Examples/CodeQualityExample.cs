using UnityEngine;
using UnityEditor;
using CodeQuality.Analyzer;
using CodeQuality.Common;
using System;

namespace CodeQuality.Examples
{
    /// <summary>
    /// 代码质量分析示例
    /// </summary>
    public class CodeQualityExample : MonoBehaviour
    {
        [Header("示例配置")]
        public string analysisPath = "Assets/Scripts";
        public bool runAnalysisOnStart = false;
        
        void Start()
        {
            if (runAnalysisOnStart)
            {
                RunAnalysis();
            }
        }
        
        /// <summary>
        /// 运行代码质量分析
        /// </summary>
        [ContextMenu("运行代码质量分析")]
        public void RunAnalysis()
        {
            Debug.Log("开始代码质量分析...");
            
            try
            {
                var config = new CodeQualityConfig();
                var analyzer = new CodeAnalyzer();
                var result = analyzer.AnalyzeDirectory(analysisPath, config);
                
                Debug.Log($"分析完成！");
                Debug.Log($"总体评分: {result.codeQualityScore:P1}");
                Debug.Log($"分析文件数: {result.totalFiles}");
                Debug.Log($"总代码行数: {result.totalLines}");
                Debug.Log($"分析耗时: {result.analysisTime:F2} 秒");
                
                // 显示指标详情
                foreach (var metric in result.metrics.Values)
                {
                    Debug.Log($"{metric.name}: {metric.score:P1}");
                }
                
                // 显示问题最多的文件
                var files = result.filesAnalyzed;
                files.Sort((a, b) => b.fileScore.CompareTo(a.fileScore));
                
                Debug.Log("问题最多的文件:");
                for (int i = 0; i < Math.Min(3, files.Count); i++)
                {
                    var file = files[i];
                    Debug.Log($"  {file.filePath}: {file.fileScore:P1} ({file.issues.Count} 个问题)");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"分析失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 分析单个文件
        /// </summary>
        [ContextMenu("分析当前脚本")]
        public void AnalyzeCurrentScript()
        {
            var scriptPath = GetCurrentScriptPath();
            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogWarning("无法获取当前脚本路径");
                return;
            }
            
            Debug.Log($"分析脚本: {scriptPath}");
            
            try
            {
                var analyzer = new CodeAnalyzer();
                var result = analyzer.AnalyzeFile(scriptPath);
                
                Debug.Log($"脚本分析完成！");
                Debug.Log($"总体评分: {result.codeQualityScore:P1}");
                Debug.Log($"代码行数: {result.totalLines}");
                
                // 显示指标详情
                foreach (var metric in result.metrics.Values)
                {
                    Debug.Log($"{metric.name}: {metric.score:P1}");
                    if (metric.issues.Count > 0)
                    {
                        foreach (var issue in metric.issues)
                        {
                            Debug.Log($"  - {issue}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"分析失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取当前脚本路径
        /// </summary>
        private string GetCurrentScriptPath()
        {
            var script = MonoScript.FromMonoBehaviour(this);
            if (script != null)
            {
                return AssetDatabase.GetAssetPath(script);
            }
            return null;
        }
    }
}
