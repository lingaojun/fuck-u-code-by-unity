using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeQuality.Common;

namespace CodeQuality.Parser
{
    /// <summary>
    /// 通用代码解析器
    /// </summary>
    public class GenericCodeParser : ICodeParser
    {
        public ParseResult Parse(string filePath, string content, LanguageType language)
        {
            var result = new ParseResult(filePath, language, content);
            
            // 计算行数
            CalculateLineCounts(content, result);
            
            // 解析函数
            ParseFunctions(content, result, language);
            
            // 解析类
            ParseClasses(content, result, language);
            
            // 解析变量
            ParseVariables(content, result, language);
            
            return result;
        }
        
        /// <summary>
        /// 计算行数统计
        /// </summary>
        private void CalculateLineCounts(string content, ParseResult result)
        {
            var lines = content.Split('\n');
            result.totalLines = lines.Length;
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    result.blankLines++;
                }
                else if (IsCommentLine(trimmed, result.language))
                {
                    result.commentLines++;
                }
                else
                {
                    result.codeLines++;
                }
            }
        }
        
        /// <summary>
        /// 解析函数
        /// </summary>
        private void ParseFunctions(string content, ParseResult result, LanguageType language)
        {
            var patterns = GetFunctionPatterns(language);
            
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                
                foreach (Match match in matches)
                {
                    var functionName = ExtractFunctionName(match, language);
                    var functionBody = ExtractFunctionBody(content, match, language);
                    var startLine = GetLineNumber(content, match.Index);
                    var endLine = GetLineNumber(content, match.Index + functionBody.Length);
                    
                    if (!string.IsNullOrEmpty(functionName))
                    {
                        var function = new FunctionInfo(functionName, functionBody, startLine, endLine);
                        result.functions.Add(function);
                    }
                }
            }
        }
        
        /// <summary>
        /// 解析类
        /// </summary>
        private void ParseClasses(string content, ParseResult result, LanguageType language)
        {
            var patterns = GetClassPatterns(language);
            
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                
                foreach (Match match in matches)
                {
                    var className = ExtractClassName(match, language);
                    var classBody = ExtractClassBody(content, match, language);
                    var startLine = GetLineNumber(content, match.Index);
                    var endLine = GetLineNumber(content, match.Index + classBody.Length);
                    
                    if (!string.IsNullOrEmpty(className))
                    {
                        var classInfo = new ClassInfo(className, classBody, startLine, endLine);
                        result.classes.Add(classInfo);
                    }
                }
            }
        }
        
        /// <summary>
        /// 解析变量
        /// </summary>
        private void ParseVariables(string content, ParseResult result, LanguageType language)
        {
            var patterns = GetVariablePatterns(language);
            
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                
                foreach (Match match in matches)
                {
                    var variableName = ExtractVariableName(match, language);
                    var variableType = ExtractVariableType(match, language);
                    var line = GetLineNumber(content, match.Index);
                    
                    if (!string.IsNullOrEmpty(variableName))
                    {
                        var variable = new VariableInfo(variableName, variableType, line);
                        result.variables.Add(variable);
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查是否为注释行
        /// </summary>
        private bool IsCommentLine(string line, LanguageType language)
        {
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                case LanguageType.Go:
                    return line.StartsWith("//") || line.StartsWith("/*") || line.StartsWith("*");
                    
                case LanguageType.Python:
                    return line.StartsWith("#");
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 获取函数匹配模式
        /// </summary>
        private string[] GetFunctionPatterns(LanguageType language)
        {
            switch (language)
            {
                case LanguageType.CSharp:
                    return new[]
                    {
                        @"(public|private|protected|internal)?\s*(static)?\s*[a-zA-Z_][a-zA-Z0-9_]*\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\(",
                        @"(public|private|protected|internal)?\s*(static)?\s*void\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\("
                    };
                    
                case LanguageType.Java:
                    return new[]
                    {
                        @"(public|private|protected)?\s*(static)?\s*[a-zA-Z_][a-zA-Z0-9_]*\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\(",
                        @"(public|private|protected)?\s*(static)?\s*void\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\("
                    };
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    return new[]
                    {
                        @"function\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\(",
                        @"[a-zA-Z_][a-zA-Z0-9_]*\s*:\s*function\s*\(",
                        @"[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*function\s*\(",
                        @"[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*\([^)]*\)\s*=>"
                    };
                    
                case LanguageType.Python:
                    return new[]
                    {
                        @"def\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\("
                    };
                    
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    return new[]
                    {
                        @"[a-zA-Z_][a-zA-Z0-9_]*\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\(",
                        @"void\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\("
                    };
                    
                case LanguageType.Go:
                    return new[]
                    {
                        @"func\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\(",
                        @"func\s+\([^)]*\)\s*[a-zA-Z_][a-zA-Z0-9_]*\s*\("
                    };
                    
                default:
                    return new[] { @"[a-zA-Z_][a-zA-Z0-9_]*\s*\(" };
            }
        }
        
        /// <summary>
        /// 获取类匹配模式
        /// </summary>
        private string[] GetClassPatterns(LanguageType language)
        {
            switch (language)
            {
                case LanguageType.CSharp:
                    return new[] { @"(public|private|protected|internal)?\s*class\s+[a-zA-Z_][a-zA-Z0-9_]*" };
                    
                case LanguageType.Java:
                    return new[] { @"(public|private|protected)?\s*class\s+[a-zA-Z_][a-zA-Z0-9_]*" };
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    return new[] { @"class\s+[a-zA-Z_][a-zA-Z0-9_]*" };
                    
                case LanguageType.Python:
                    return new[] { @"class\s+[a-zA-Z_][a-zA-Z0-9_]*" };
                    
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    return new[] { @"class\s+[a-zA-Z_][a-zA-Z0-9_]*" };
                    
                case LanguageType.Go:
                    return new[] { @"type\s+[a-zA-Z_][a-zA-Z0-9_]*\s+struct" };
                    
                default:
                    return new[] { @"class\s+[a-zA-Z_][a-zA-Z0-9_]*" };
            }
        }
        
        /// <summary>
        /// 获取变量匹配模式
        /// </summary>
        private string[] GetVariablePatterns(LanguageType language)
        {
            switch (language)
            {
                case LanguageType.CSharp:
                    return new[] { @"(public|private|protected|internal)?\s*(static)?\s*[a-zA-Z_][a-zA-Z0-9_]*\s+[a-zA-Z_][a-zA-Z0-9_]*\s*[=;]" };
                    
                case LanguageType.Java:
                    return new[] { @"(public|private|protected)?\s*(static)?\s*[a-zA-Z_][a-zA-Z0-9_]*\s+[a-zA-Z_][a-zA-Z0-9_]*\s*[=;]" };
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    return new[] { @"(var|let|const)\s+[a-zA-Z_][a-zA-Z0-9_]*" };
                    
                case LanguageType.Python:
                    return new[] { @"[a-zA-Z_][a-zA-Z0-9_]*\s*=" };
                    
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    return new[] { @"[a-zA-Z_][a-zA-Z0-9_]*\s+[a-zA-Z_][a-zA-Z0-9_]*\s*[=;]" };
                    
                case LanguageType.Go:
                    return new[] { @"var\s+[a-zA-Z_][a-zA-Z0-9_]*", @"[a-zA-Z_][a-zA-Z0-9_]*\s*:=" };
                    
                default:
                    return new[] { @"[a-zA-Z_][a-zA-Z0-9_]*\s*=" };
            }
        }
        
        /// <summary>
        /// 提取函数名
        /// </summary>
        private string ExtractFunctionName(Match match, LanguageType language)
        {
            var matchValue = match.Value;
            
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    var parts = matchValue.Split(' ');
                    return parts[parts.Length - 1].Split('(')[0];
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    if (matchValue.StartsWith("function"))
                    {
                        return matchValue.Split(' ')[1].Split('(')[0];
                    }
                    else
                    {
                        return matchValue.Split(' ')[0].Split('(')[0];
                    }
                    
                case LanguageType.Python:
                    return matchValue.Split(' ')[1].Split('(')[0];
                    
                case LanguageType.Go:
                    if (matchValue.Contains("func"))
                    {
                        var funcIndex = matchValue.IndexOf("func") + 4;
                        var afterFunc = matchValue.Substring(funcIndex).Trim();
                        return afterFunc.Split(' ')[0].Split('(')[0];
                    }
                    break;
            }
            
            return "";
        }
        
        /// <summary>
        /// 提取类名
        /// </summary>
        private string ExtractClassName(Match match, LanguageType language)
        {
            var matchValue = match.Value;
            
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                case LanguageType.Python:
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    var parts = matchValue.Split(' ');
                    return parts[parts.Length - 1];
                    
                case LanguageType.Go:
                    if (matchValue.Contains("type"))
                    {
                        var typeIndex = matchValue.IndexOf("type") + 4;
                        var afterType = matchValue.Substring(typeIndex).Trim();
                        return afterType.Split(' ')[0];
                    }
                    break;
            }
            
            return "";
        }
        
        /// <summary>
        /// 提取变量名
        /// </summary>
        private string ExtractVariableName(Match match, LanguageType language)
        {
            var matchValue = match.Value;
            
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    var parts = matchValue.Split(' ');
                    return parts[parts.Length - 1].Split('=')[0].Split(';')[0];
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    if (matchValue.StartsWith("var") || matchValue.StartsWith("let") || matchValue.StartsWith("const"))
                    {
                        return matchValue.Split(' ')[1];
                    }
                    else
                    {
                        return matchValue.Split(' ')[0].Split('=')[0];
                    }
                    
                case LanguageType.Python:
                    return matchValue.Split(' ')[0].Split('=')[0];
                    
                case LanguageType.Go:
                    if (matchValue.StartsWith("var"))
                    {
                        return matchValue.Split(' ')[1];
                    }
                    else
                    {
                        return matchValue.Split(' ')[0].Split(':')[0];
                    }
            }
            
            return "";
        }
        
        /// <summary>
        /// 提取变量类型
        /// </summary>
        private string ExtractVariableType(Match match, LanguageType language)
        {
            var matchValue = match.Value;
            
            switch (language)
            {
                case LanguageType.CSharp:
                case LanguageType.Java:
                case LanguageType.CPlusPlus:
                case LanguageType.C:
                    var parts = matchValue.Split(' ');
                    return parts[parts.Length - 2];
                    
                case LanguageType.JavaScript:
                case LanguageType.TypeScript:
                    return "var"; // JavaScript 是动态类型
                    
                case LanguageType.Python:
                    return "var"; // Python 是动态类型
                    
                case LanguageType.Go:
                    if (matchValue.StartsWith("var"))
                    {
                        var goParts = matchValue.Split(' ');
                        return goParts.Length > 2 ? goParts[1] : "var";
                    }
                    else
                    {
                        return "var"; // Go 可以推断类型
                    }
            }
            
            return "unknown";
        }
        
        /// <summary>
        /// 提取函数体
        /// </summary>
        private string ExtractFunctionBody(string content, Match match, LanguageType language)
        {
            // 简化实现，实际应该解析大括号匹配
            var startIndex = match.Index;
            var braceCount = 0;
            var inFunction = false;
            
            for (int i = startIndex; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    braceCount++;
                    inFunction = true;
                }
                else if (content[i] == '}')
                {
                    braceCount--;
                    if (inFunction && braceCount == 0)
                    {
                        return content.Substring(startIndex, i - startIndex + 1);
                    }
                }
            }
            
            return "";
        }
        
        /// <summary>
        /// 提取类体
        /// </summary>
        private string ExtractClassBody(string content, Match match, LanguageType language)
        {
            // 简化实现，实际应该解析大括号匹配
            var startIndex = match.Index;
            var braceCount = 0;
            var inClass = false;
            
            for (int i = startIndex; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    braceCount++;
                    inClass = true;
                }
                else if (content[i] == '}')
                {
                    braceCount--;
                    if (inClass && braceCount == 0)
                    {
                        return content.Substring(startIndex, i - startIndex + 1);
                    }
                }
            }
            
            return "";
        }
        
        /// <summary>
        /// 获取行号
        /// </summary>
        private int GetLineNumber(string content, int index)
        {
            var lines = content.Substring(0, index).Split('\n');
            return lines.Length;
        }
    }
}
