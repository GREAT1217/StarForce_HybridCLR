using System.IO;
using GameFramework;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Game.Editor
{
    public class HuaTuoGenerator
    {
        private const string HotfixDllBuildPath = "Temp/HuaTuo/Build";
        private const string HotfixDllBuildName = "Game.Hotfix.dll";
        private const string HotfixDllPath = "Assets/Game/Hotfix";
        private const string HotfixDllName = "Game.Hotfix.dll.bytes";

        [MenuItem("Generator/Hotfix Dll/Win64")]
        public static void CompileDllWin64()
        {
            BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
            string path = Utility.Text.Format("{0}/{1}", HotfixDllBuildPath, buildTarget);
            BuildDll(path, buildTarget);
        }

        private static void BuildDll(string path, BuildTarget buildTarget)
        {
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(buildTarget);
            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = buildTarget;
            scriptCompilationSettings.options = ScriptCompilationOptions.DevelopmentBuild;

            IOUtility.CreateDirectoryIfNotExists(path);
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, path);
            foreach (var ass in scriptCompilationResult.assemblies)
            {
                Debug.LogFormat("Build assemblies : {0}", ass);
            }

            IOUtility.CreateDirectoryIfNotExists(HotfixDllPath);
            string oriFileName = Utility.Text.Format("{0}/{1}", path, HotfixDllBuildName);
            string desFileName = Utility.Text.Format("{0}/{1}", HotfixDllPath, HotfixDllName);
            File.Copy(oriFileName, desFileName, true);
            Debug.Log("Hotfix dll build complete.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
