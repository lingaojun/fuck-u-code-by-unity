using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeQuality.Common
{
    /// <summary>
    /// 代码解析结果
    /// </summary>
    [Serializable]
    public class ParseResult
    {
        public string filePath;
        public LanguageType language;
        public string content;
        public int totalLines;
        public int codeLines;
        public int commentLines;
        public int blankLines;
        public List<FunctionInfo> functions = new List<FunctionInfo>();
        public List<ClassInfo> classes = new List<ClassInfo>();
        public List<VariableInfo> variables = new List<VariableInfo>();
        
        public ParseResult(string filePath, LanguageType language, string content)
        {
            this.filePath = filePath;
            this.language = language;
            this.content = content;
            this.functions = new List<FunctionInfo>();
            this.classes = new List<ClassInfo>();
            this.variables = new List<VariableInfo>();
        }
    }
    
    /// <summary>
    /// 函数信息
    /// </summary>
    [Serializable]
    public class FunctionInfo
    {
        public string name;
        public string body;
        public int startLine;
        public int endLine;
        public int parameterCount;
        public List<string> parameters = new List<string>();
        public string returnType;
        public AccessModifier accessModifier;
        
        public FunctionInfo(string name, string body, int startLine, int endLine)
        {
            this.name = name;
            this.body = body;
            this.startLine = startLine;
            this.endLine = endLine;
            this.parameters = new List<string>();
        }
    }
    
    /// <summary>
    /// 类信息
    /// </summary>
    [Serializable]
    public class ClassInfo
    {
        public string name;
        public string body;
        public int startLine;
        public int endLine;
        public AccessModifier accessModifier;
        public List<string> baseClasses = new List<string>();
        public List<string> interfaces = new List<string>();
        
        public ClassInfo(string name, string body, int startLine, int endLine)
        {
            this.name = name;
            this.body = body;
            this.startLine = startLine;
            this.endLine = endLine;
            this.baseClasses = new List<string>();
            this.interfaces = new List<string>();
        }
    }
    
    /// <summary>
    /// 变量信息
    /// </summary>
    [Serializable]
    public class VariableInfo
    {
        public string name;
        public string type;
        public string value;
        public int line;
        public AccessModifier accessModifier;
        public bool isConst;
        public bool isStatic;
        
        public VariableInfo(string name, string type, int line)
        {
            this.name = name;
            this.type = type;
            this.line = line;
        }
    }
    
    /// <summary>
    /// 访问修饰符
    /// </summary>
    public enum AccessModifier
    {
        Public,
        Private,
        Protected,
        Internal,
        None
    }
}
