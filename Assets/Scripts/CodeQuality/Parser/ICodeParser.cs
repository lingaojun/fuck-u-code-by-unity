using CodeQuality.Common;

namespace CodeQuality.Parser
{
    /// <summary>
    /// 代码解析器接口
    /// </summary>
    public interface ICodeParser
    {
        /// <summary>
        /// 解析代码
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">文件内容</param>
        /// <param name="language">语言类型</param>
        /// <returns>解析结果</returns>
        ParseResult Parse(string filePath, string content, LanguageType language);
    }
}
