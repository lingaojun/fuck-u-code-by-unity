using System;
using CodeQuality.Common;

namespace CodeQuality.Metrics
{
    /// <summary>
    /// 代码质量指标接口
    /// </summary>
    public interface IMetric
    {
        /// <summary>
        /// 指标名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 指标描述
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 指标权重
        /// </summary>
        float Weight { get; }
        
        /// <summary>
        /// 支持的语言类型
        /// </summary>
        LanguageType[] SupportedLanguages { get; }
        
        /// <summary>
        /// 分析代码并返回结果
        /// </summary>
        /// <param name="parseResult">解析结果</param>
        /// <returns>指标结果</returns>
        MetricResult Analyze(ParseResult parseResult);
        
        /// <summary>
        /// 设置翻译器
        /// </summary>
        /// <param name="translator">翻译器</param>
        void SetTranslator(ITranslator translator);
    }
    
    /// <summary>
    /// 基础指标实现
    /// </summary>
    public abstract class BaseMetric : IMetric
    {
        protected ITranslator translator;
        
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract float Weight { get; }
        public abstract LanguageType[] SupportedLanguages { get; }
        
        public virtual void SetTranslator(ITranslator translator)
        {
            this.translator = translator;
        }
        
        public abstract MetricResult Analyze(ParseResult parseResult);
        
        /// <summary>
        /// 检查是否支持指定语言
        /// </summary>
        protected bool IsLanguageSupported(LanguageType language)
        {
            if (SupportedLanguages == null || SupportedLanguages.Length == 0)
                return true;
                
            foreach (var supportedLang in SupportedLanguages)
            {
                if (supportedLang == language)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 创建指标结果
        /// </summary>
        protected MetricResult CreateResult(string name, float score, string description, float weight)
        {
            return new MetricResult(name, score, description, weight);
        }
    }
}
