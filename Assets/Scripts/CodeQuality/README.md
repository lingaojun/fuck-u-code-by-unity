# Unity 代码质量分析工具

这是一个基于 Unity 的代码质量分析工具，从 Go 语言的 `fuck-u-code` 项目转换而来。

## 功能特性

- **七维度代码质量检测**：
  - 循环复杂度分析
  - 函数长度检查
  - 注释覆盖率统计
  - 错误处理检查
  - 命名规范验证
  - 代码重复度检测
  - 代码结构分析

- **多语言支持**：
  - C#、JavaScript、TypeScript
  - Python、Java、C/C++、Go

- **Unity 集成**：
  - 可视化分析窗口
  - 实时分析进度
  - 配置界面
  - Markdown 报告导出

## 使用方法

### 1. 打开分析窗口

在 Unity 编辑器中：
```
Tools → Code Quality Analysis
```

### 2. 配置分析参数

- **分析路径**：选择要分析的目录
- **详细模式**：显示详细的分析过程
- **仅显示摘要**：只显示分析结果摘要
- **Markdown 输出**：生成 Markdown 格式报告
- **显示文件数**：设置显示的文件数量
- **每文件问题数**：设置每个文件显示的问题数量
- **报告语言**：选择报告语言
- **排除模式**：设置要排除的文件或目录

### 3. 指标权重配置

可以调整各个指标的权重：
- 循环复杂度 (默认: 0.2)
- 函数长度 (默认: 0.15)
- 注释覆盖率 (默认: 0.1)
- 错误处理 (默认: 0.1)
- 命名规范 (默认: 0.15)
- 代码重复度 (默认: 0.15)
- 代码结构分析 (默认: 0.15)

### 4. 开始分析

点击"开始分析"按钮，工具会：
1. 扫描指定目录下的所有源码文件
2. 根据语言类型解析代码
3. 应用七维度指标进行分析
4. 生成分析结果和报告

### 5. 查看结果

分析完成后，可以查看：
- **总体评分**：0-100 分，分数越高表示代码质量越差
- **指标详情**：每个维度的具体评分和问题
- **文件分析**：每个文件的具体问题和评分

### 6. 导出报告

点击"导出报告"按钮，可以生成 Markdown 格式的详细报告。

## 代码示例

### 基本使用

```csharp
using CodeQuality.Analyzer;
using CodeQuality.Common;

// 创建分析器
var analyzer = new CodeAnalyzer();

// 分析目录
var config = new CodeQualityConfig();
var result = analyzer.AnalyzeDirectory("Assets/Scripts", config);

// 查看结果
Debug.Log($"总体评分: {result.codeQualityScore:P1}");
Debug.Log($"分析文件数: {result.totalFiles}");
```

### 分析单个文件

```csharp
// 分析单个文件
var result = analyzer.AnalyzeFile("Assets/Scripts/MyScript.cs");

// 查看文件结果
foreach (var file in result.filesAnalyzed)
{
    Debug.Log($"文件: {file.filePath}");
    Debug.Log($"评分: {file.fileScore:P1}");
    Debug.Log($"问题数: {file.issues.Count}");
}
```

### 自定义配置

```csharp
var config = new CodeQualityConfig();
config.verbose = true;
config.topFiles = 10;
config.cyclomaticComplexityWeight = 0.3f;
config.functionLengthWeight = 0.2f;

// 添加排除模式
config.excludePatterns.Add("**/Test/**");
config.excludePatterns.Add("**/*.generated.cs");
```

## 项目结构

```
Assets/Scripts/CodeQuality/
├── Common/           # 通用类型和接口
│   ├── AnalysisResult.cs      # 分析结果
│   ├── CodeQualityConfig.cs   # 配置类
│   ├── LanguageType.cs        # 语言类型
│   ├── ParseResult.cs         # 解析结果
│   └── ITranslator.cs         # 翻译器接口
├── Metrics/          # 七维度指标实现
│   ├── IMetric.cs                    # 指标接口
│   ├── CyclomaticComplexityMetric.cs # 循环复杂度
│   ├── FunctionLengthMetric.cs       # 函数长度
│   ├── CommentRatioMetric.cs         # 注释覆盖率
│   ├── ErrorHandlingMetric.cs        # 错误处理
│   ├── NamingConventionMetric.cs     # 命名规范
│   ├── CodeDuplicationMetric.cs      # 代码重复度
│   └── StructureAnalysisMetric.cs    # 代码结构分析
├── Parser/           # 多语言代码解析器
│   ├── ICodeParser.cs        # 解析器接口
│   └── GenericCodeParser.cs  # 通用解析器
├── Analyzer/         # 分析器核心逻辑
│   └── CodeAnalyzer.cs       # 代码分析器
├── Editor/           # Unity 编辑器界面
│   └── CodeQualityWindow.cs  # 分析窗口
└── Examples/         # 使用示例
    └── CodeQualityExample.cs # 示例脚本
```

## 评分说明

- **0-30 分**：优秀，代码质量很好
- **30-60 分**：良好，有少量问题需要改进
- **60-80 分**：一般，存在较多问题需要关注
- **80-100 分**：较差，存在严重问题需要重构

## 注意事项

1. 分析大型项目可能需要较长时间
2. 建议定期运行分析以跟踪代码质量变化
3. 可以根据项目特点调整指标权重
4. 排除不必要的文件可以提高分析效率

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request 来改进这个工具！
