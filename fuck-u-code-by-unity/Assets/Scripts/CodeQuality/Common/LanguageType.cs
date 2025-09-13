using System;

namespace CodeQuality.Common
{
    /// <summary>
    /// 编程语言类型枚举
    /// </summary>
    public enum LanguageType
    {
        CSharp,
        JavaScript,
        TypeScript,
        Python,
        Java,
        CPlusPlus,
        C,
        Go,
        Unsupported
    }

    /// <summary>
    /// 语言检测器接口
    /// </summary>
    public interface ILanguageDetector
    {
        /// <summary>
        /// 根据文件路径检测语言类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>语言类型</returns>
        LanguageType DetectLanguage(string filePath);

        /// <summary>
        /// 判断文件是否为支持的类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否支持</returns>
        bool IsSupportedFile(string filePath);
    }

    /// <summary>
    /// 默认语言检测器实现
    /// </summary>
    public class DefaultLanguageDetector : ILanguageDetector
    {
        private static readonly LanguageType[] SupportedLanguages = 
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

        public LanguageType DetectLanguage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return LanguageType.Unsupported;

            string extension = System.IO.Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".cs":
                case ".razor":
                    return LanguageType.CSharp;
                case ".js":
                case ".jsx":
                    return LanguageType.JavaScript;
                case ".ts":
                case ".tsx":
                    return LanguageType.TypeScript;
                case ".py":
                    return LanguageType.Python;
                case ".java":
                    return LanguageType.Java;
                case ".cpp":
                case ".cc":
                case ".cxx":
                case ".hpp":
                    return LanguageType.CPlusPlus;
                case ".c":
                case ".h":
                    return LanguageType.C;
                case ".go":
                    return LanguageType.Go;
                default:
                    return LanguageType.Unsupported;
            }
        }

        public bool IsSupportedFile(string filePath)
        {
            var language = DetectLanguage(filePath);
            return Array.Exists(SupportedLanguages, l => l == language);
        }
    }
}
