using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeQuality.Common
{
    /// <summary>
    /// 代码质量分析结果
    /// </summary>
    [Serializable]
    public class AnalysisResult
    {
        [Header("总体评分")]
        public float codeQualityScore;
        public int totalFiles;
        public int totalLines;
        public float analysisTime;
        
        [Header("指标结果")]
        public Dictionary<string, MetricResult> metrics = new Dictionary<string, MetricResult>();
        
        [Header("文件分析结果")]
        public List<FileAnalysisResult> filesAnalyzed = new List<FileAnalysisResult>();
        
        [Header("问题统计")]
        public int totalIssues;
        public int criticalIssues;
        public int warningIssues;
        public int infoIssues;
        
        public AnalysisResult()
        {
            metrics = new Dictionary<string, MetricResult>();
            filesAnalyzed = new List<FileAnalysisResult>();
        }
        
        /// <summary>
        /// 添加指标结果
        /// </summary>
        public void AddMetricResult(string name, MetricResult result)
        {
            metrics[name] = result;
        }
        
        /// <summary>
        /// 添加文件分析结果
        /// </summary>
        public void AddFileResult(FileAnalysisResult result)
        {
            filesAnalyzed.Add(result);
            totalFiles++;
            totalLines += result.totalLines;
        }
        
        /// <summary>
        /// 计算总体评分
        /// </summary>
        public void CalculateOverallScore()
        {
            if (filesAnalyzed.Count == 0)
            {
                codeQualityScore = 0f;
                return;
            }
            
            float totalScore = 0f;
            foreach (var file in filesAnalyzed)
            {
                totalScore += file.fileScore;
            }
            
            codeQualityScore = totalScore / filesAnalyzed.Count;
        }
        
        /// <summary>
        /// 获取问题统计
        /// </summary>
        public void CalculateIssueStats()
        {
            totalIssues = 0;
            criticalIssues = 0;
            warningIssues = 0;
            infoIssues = 0;
            
            foreach (var file in filesAnalyzed)
            {
                totalIssues += file.issues.Count;
                criticalIssues += file.criticalIssues;
                warningIssues += file.warningIssues;
                infoIssues += file.infoIssues;
            }
        }
    }
    
    /// <summary>
    /// 指标结果
    /// </summary>
    [Serializable]
    public class MetricResult
    {
        public string name;
        public float score; // 0-1，越高越差
        public string description;
        public float weight;
        public List<string> issues = new List<string>();
        
        public MetricResult(string name, float score, string description, float weight)
        {
            this.name = name;
            this.score = score;
            this.description = description;
            this.weight = weight;
            this.issues = new List<string>();
        }
    }
    
    /// <summary>
    /// 文件分析结果
    /// </summary>
    [Serializable]
    public class FileAnalysisResult
    {
        public string filePath;
        public float fileScore;
        public int totalLines;
        public int codeLines;
        public int commentLines;
        public int blankLines;
        public List<string> issues = new List<string>();
        public int criticalIssues;
        public int warningIssues;
        public int infoIssues;
        public Dictionary<string, float> metricScores = new Dictionary<string, float>();
        
        public FileAnalysisResult(string filePath)
        {
            this.filePath = filePath;
            this.issues = new List<string>();
            this.metricScores = new Dictionary<string, float>();
        }
        
        /// <summary>
        /// 添加问题
        /// </summary>
        public void AddIssue(string issue, IssueLevel level = IssueLevel.Info)
        {
            issues.Add(issue);
            
            switch (level)
            {
                case IssueLevel.Critical:
                    criticalIssues++;
                    break;
                case IssueLevel.Warning:
                    warningIssues++;
                    break;
                case IssueLevel.Info:
                    infoIssues++;
                    break;
            }
        }
        
        /// <summary>
        /// 添加指标分数
        /// </summary>
        public void AddMetricScore(string metricName, float score)
        {
            metricScores[metricName] = score;
        }
    }
    
    /// <summary>
    /// 问题级别
    /// </summary>
    public enum IssueLevel
    {
        Info,
        Warning,
        Critical
    }
}
