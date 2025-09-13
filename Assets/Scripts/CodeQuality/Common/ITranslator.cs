using System;

namespace CodeQuality.Common
{
    /// <summary>
    /// 翻译器接口
    /// </summary>
    public interface ITranslator
    {
        /// <summary>
        /// 翻译文本
        /// </summary>
        /// <param name="key">翻译键</param>
        /// <param name="args">参数</param>
        /// <returns>翻译后的文本</returns>
        string Translate(string key, params object[] args);
        
        /// <summary>
        /// 获取当前语言
        /// </summary>
        LanguageType GetLanguage();
        
        /// <summary>
        /// 设置语言
        /// </summary>
        /// <param name="language">语言类型</param>
        void SetLanguage(LanguageType language);
    }
    
    /// <summary>
    /// 默认翻译器实现
    /// </summary>
    public class DefaultTranslator : ITranslator
    {
        private LanguageType currentLanguage = LanguageType.CSharp;
        
        public string Translate(string key, params object[] args)
        {
            // 简单的翻译实现，实际项目中应该从资源文件加载
            var translations = GetTranslations();
            
            if (translations.TryGetValue(key, out var text))
            {
                if (args != null && args.Length > 0)
                {
                    return string.Format(text, args);
                }
                return text;
            }
            
            return key; // 如果找不到翻译，返回原键
        }
        
        public LanguageType GetLanguage()
        {
            return currentLanguage;
        }
        
        public void SetLanguage(LanguageType language)
        {
            currentLanguage = language;
        }
        
        private System.Collections.Generic.Dictionary<string, string> GetTranslations()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
                // 通用翻译
                { "analyzer.start_analyzing", "开始分析代码质量..." },
                { "analyzer.files_found", "找到文件" },
                { "analyzer.analyzing_files", "正在分析文件" },
                { "analyzer.analysis_complete", "分析完成" },
                
                // 指标翻译
                { "metric.cyclomatic_complexity", "循环复杂度" },
                { "metric.cyclomatic_complexity.description", "测量代码的复杂程度，基于控制流语句的数量" },
                { "metric.function_length", "函数长度" },
                { "metric.function_length.description", "测量函数的长度，过长的函数难以理解和维护" },
                { "metric.comment_ratio", "注释覆盖率" },
                { "metric.comment_ratio.description", "测量代码中注释的比例，适当的注释有助于代码理解" },
                { "metric.error_handling", "错误处理" },
                { "metric.error_handling.description", "检查代码中的错误处理机制" },
                { "metric.naming_convention", "命名规范" },
                { "metric.naming_convention.description", "检查代码中的命名是否符合规范" },
                { "metric.code_duplication", "代码重复度" },
                { "metric.code_duplication.description", "检查代码中的重复部分" },
                { "metric.structure_analysis", "代码结构分析" },
                { "metric.structure_analysis.description", "分析代码的整体结构质量" },
                
                // 错误信息
                { "error.file_not_found", "文件未找到: {0}" },
                { "error.parse_failed", "解析失败: {0}" },
                { "error.analysis_failed", "分析失败: {0}" },
                
                // 报告翻译
                { "report.title", "代码质量分析报告" },
                { "report.overall_score", "总体评分" },
                { "report.metrics", "指标详情" },
                { "report.files", "文件分析" },
                { "report.issues", "问题列表" },
                { "report.recommendations", "改进建议" }
            };
        }
    }
}
