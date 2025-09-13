using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeQuality.Common
{
    /// <summary>
    /// 代码质量分析配置
    /// </summary>
    [Serializable]
    public class CodeQualityConfig
    {
        [Header("分析设置")]
        public bool verbose = false;
        public int topFiles = 5;
        public int maxIssues = 5;
        public bool summaryOnly = false;
        public bool markdownOutput = false;
        public LanguageType reportLanguage = LanguageType.CSharp;
        
        [Header("排除设置")]
        public List<string> excludePatterns = new List<string>();
        public bool skipIndexFiles = false;
        
        [Header("指标权重")]
        [Range(0f, 1f)]
        public float cyclomaticComplexityWeight = 0.2f;
        [Range(0f, 1f)]
        public float functionLengthWeight = 0.15f;
        [Range(0f, 1f)]
        public float commentRatioWeight = 0.1f;
        [Range(0f, 1f)]
        public float errorHandlingWeight = 0.1f;
        [Range(0f, 1f)]
        public float namingConventionWeight = 0.15f;
        [Range(0f, 1f)]
        public float codeDuplicationWeight = 0.15f;
        [Range(0f, 1f)]
        public float structureAnalysisWeight = 0.15f;
        
        [Header("阈值设置")]
        public int maxFunctionLength = 50;
        public int maxCyclomaticComplexity = 10;
        public float minCommentRatio = 0.2f;
        
        public CodeQualityConfig()
        {
            // 默认排除模式
            excludePatterns.AddRange(new[]
            {
                "**/node_modules/**",
                "**/dist/**",
                "**/build/**",
                "**/.git/**",
                "**/Library/**",
                "**/Temp/**",
                "**/obj/**",
                "**/bin/**",
                "**/*.meta",
                "**/*.asset",
                "**/*.unity",
                "**/*.prefab",
                "**/*.mat",
                "**/*.shader",
                "**/*.anim",
                "**/*.controller",
                "**/*.overrideController",
                "**/*.playable",
                "**/*.signal",
                "**/*.timeline",
                "**/*.renderTexture",
                "**/*.cubemap",
                "**/*.lightmap",
                "**/*.reflectionProbe",
                "**/*.occlusionCullingData",
                "**/*.physicsMaterial2D",
                "**/*.physicsMaterial",
                "**/*.terrainData",
                "**/*.spriteAtlas",
                "**/*.spriteAtlasV2",
                "**/*.brush",
                "**/*.flare",
                "**/*.fontsettings",
                "**/*.guiskin",
                "**/*.inputactions",
                "**/*.preset",
                "**/*.asmdef",
                "**/*.asmref",
                "**/*.asmref.meta",
                "**/*.asmdef.meta",
                "**/*.dll",
                "**/*.pdb",
                "**/*.mdb",
                "**/*.exe",
                "**/*.so",
                "**/*.dylib",
                "**/*.a",
                "**/*.lib",
                "**/*.exp",
                "**/*.ilk",
                "**/*.pch",
                "**/*.idb",
                "**/*.ipdb",
                "**/*.pgc",
                "**/*.pgd",
                "**/*.rsp",
                "**/*.sbr",
                "**/*.tlb",
                "**/*.tli",
                "**/*.tlh",
                "**/*.tmp",
                "**/*.tmp_proj",
                "**/*.log",
                "**/*.vspscc",
                "**/*.vssscc",
                "**/*.scc",
                "**/*.shproj",
                "**/*.shproj.user",
                "**/*.suo",
                "**/*.user",
                "**/*.userosscache",
                "**/*.sln.docstates",
                "**/*.userprefs",
                "**/*.pidb",
                "**/*.booproj",
                "**/*.svd",
                "**/*.pdb",
                "**/*.opendb",
                "**/*.opensdf",
                "**/*.sdf",
                "**/*.cachefile",
                "**/*.VC.db",
                "**/*.VC.VC.opendb",
                "**/*.psess",
                "**/*.vsp",
                "**/*.vspx",
                "**/*.sap",
                "**/*.VC.db",
                "**/*.VC.VC.opendb",
                "**/*.psess",
                "**/*.vsp",
                "**/*.vspx",
                "**/*.sap",
                "**/*.VC.db",
                "**/*.VC.VC.opendb",
                "**/*.psess",
                "**/*.vsp",
                "**/*.vspx",
                "**/*.sap"
            });
        }
        
        /// <summary>
        /// 获取所有指标权重
        /// </summary>
        public Dictionary<string, float> GetMetricWeights()
        {
            return new Dictionary<string, float>
            {
                { "cyclomatic_complexity", cyclomaticComplexityWeight },
                { "function_length", functionLengthWeight },
                { "comment_ratio", commentRatioWeight },
                { "error_handling", errorHandlingWeight },
                { "naming_convention", namingConventionWeight },
                { "code_duplication", codeDuplicationWeight },
                { "structure_analysis", structureAnalysisWeight }
            };
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        public bool Validate()
        {
            var weights = GetMetricWeights();
            float totalWeight = 0f;
            
            foreach (var weight in weights.Values)
            {
                totalWeight += weight;
            }
            
            return Math.Abs(totalWeight - 1.0f) < 0.01f;
        }
    }
}
