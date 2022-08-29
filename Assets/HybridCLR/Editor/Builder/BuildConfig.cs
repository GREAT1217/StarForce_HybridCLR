using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Builder
{
    public static class BuildConfig
    {
        private static readonly string LocalIl2CppDir = Application.dataPath + "/../HybridCLRData/LocalIl2CppData/il2cpp";
        private static readonly string HotFixDllsOutputDir = Application.dataPath + "/../HybridCLRData/HotFixDlls";
        public static readonly string AssembliesPostIl2CppStripDir = Application.dataPath + "/../HybridCLRData/AssembliesPostIl2CppStrip";
        public static readonly string MethodBridgeCppDir = Application.dataPath + "/../HybridCLRData/LocalIl2CppData/il2cpp/libil2cpp/hybridclr/interpreter";

#if !UNITY_IOS
        [InitializeOnLoadMethod]
        private static void Setup()
        {
            // unity允许使用UNITY_IL2CPP_PATH环境变量指定il2cpp的位置，因此我们不再直接修改安装位置的il2cpp， 而是在本地目录
            if (!Directory.Exists(LocalIl2CppDir))
            {
                Debug.LogErrorFormat("本地il2cpp目录:{0} 不存在，未安装本地il2cpp。请使用'HybridCLR/HybridCLR Builder'安装。", LocalIl2CppDir);
                return;
            }
            Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", LocalIl2CppDir);
        }
#endif

        /// <summary>
        /// 需要在Prefab上挂脚本的热更dll名称列表，不需要挂到Prefab上的脚本可以不放在这里
        /// 但放在这里的dll即使勾选了 AnyPlatform 也会在打包过程中被排除
        /// 
        /// 另外请务必注意： 需要挂脚本的dll的名字最好别改，因为这个列表无法热更（上线后删除或添加某些非挂脚本dll没问题）。
        /// 
        /// 注意：多热更新dll不是必须的！大多数项目完全可以只有HotFix.dll这一个热更新模块,纯粹出于演示才故意设计了两个热更新模块。
        /// 另外，是否热更新跟dll名毫无关系，凡是不打包到主工程的，都可以是热更新dll。
        /// </summary>
        public static List<string> MonoHotUpdateDllNames
        {
            get
            {
                return new List<string>()
                {
                    // "HotFix.dll",
                    "Game.Hotfix.dll",
                };
            }
        }

        /// <summary>
        /// 所有热更新dll列表。放到此列表中的dll在打包时OnFilterAssemblies回调中被过滤。
        /// 这里放除了MonoHotUpdateDllNames以外的脚本不需要挂到资源上的dll列表。
        /// </summary>
        public static List<string> AllHotUpdateDllNames
        {
            get
            {
                return MonoHotUpdateDllNames.Concat(new List<string>
                {
                    // "HotFix2.dll",
                }).ToList();
            }
        }

        public static string GetHotFixDllsOutputDirByTarget(BuildTarget target)
        {
            return string.Format("{0}/{1}", HotFixDllsOutputDir, target);
        }

        public static string GetAssembliesPostIl2CppStripDir(BuildTarget target)
        {
            return string.Format("{0}/{1}", AssembliesPostIl2CppStripDir, target);
        }

        public static string GetOriginBuildStripAssembliesDir(BuildTarget target)
        {
#if UNITY_2021_1_OR_NEWER
#if UNITY_STANDALONE_WIN
            return Application.dataPath + "/../Library/Bee/artifacts/WinPlayerBuildProgram/ManagedStripped";
#elif UNITY_ANDROID
            return Application.dataPath + "/../Library/Bee/artifacts/Android/ManagedStripped";
#elif UNITY_IOS
            return Application.dataPath + "/../Library/PlayerDataCache/iOS/Data/Managed";
#elif UNITY_WEBGL
            return Application.dataPath + "/../Library/Bee/artifacts/WebGL/ManagedStripped";
#else
            throw new NotSupportedException("GetOriginBuildStripAssembliesDir");
#endif
#else
            return Application.dataPath + (target == BuildTarget.Android ? "/../Temp/StagingArea/assets/bin/Data/Managed" : "/../Temp/StagingArea/Data/Managed/");
#endif
        }
    }
}
