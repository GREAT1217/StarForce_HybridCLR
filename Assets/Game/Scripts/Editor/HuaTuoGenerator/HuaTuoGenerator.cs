using System.IO;
using Game.Hotfix;
using GameFramework;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Game.Editor
{
    public class HuaTuoGenerator
    {
        private const string AssembliesPostIl2CppStripDir = "HuatuoData/AssembliesPostIl2CppStrip";
        private const string HotfixDllBuildPath = "Temp/HuaTuo/Build";
        private const string HotfixDllPath = "Assets/Game/Hotfix";
        private const string HotfixDllName = "Game.Hotfix.dll";

        [MenuItem("Generator/Hotfix Dll/Win64")]
        public static void CompileDllWin64()
        {
            BuildDll(BuildTarget.StandaloneWindows64);
        }

        private static void BuildDll(BuildTarget buildTarget)
        {
            // Build Hotfix Dll
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(buildTarget);
            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = buildTarget;
            // scriptCompilationSettings.options = ScriptCompilationOptions.DevelopmentBuild;

            string buildPath = Utility.Text.Format("{0}/{1}", HotfixDllBuildPath, buildTarget);
            IOUtility.CreateDirectoryIfNotExists(buildPath);
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildPath);
            foreach (var ass in scriptCompilationResult.assemblies)
            {
                Debug.LogFormat("Build assemblies : {0}", ass);
            }

            // Copy Hotfix Dll
            IOUtility.CreateDirectoryIfNotExists(HotfixDllPath);
            string oriFileName = Utility.Text.Format("{0}/{1}", buildPath, HotfixDllName);
            string desFileName = Utility.Text.Format("{0}/{1}.bytes", HotfixDllPath, HotfixDllName);
            File.Copy(oriFileName, desFileName, true);

            // Copy AOT Dll
            string aotDllPath = Utility.Text.Format("{0}/{1}", AssembliesPostIl2CppStripDir, buildTarget);
            foreach (var dllName in GameHotfixEntry.AOTDllNames)
            {
                oriFileName = Utility.Text.Format("{0}/{1}", aotDllPath, dllName);
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOT dll.");
                    continue;
                }
                desFileName = Utility.Text.Format("{0}/{1}.bytes", HotfixDllPath, dllName);
                File.Copy(oriFileName, desFileName, true);
            }

            Debug.Log("Hotfix dll build complete.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
