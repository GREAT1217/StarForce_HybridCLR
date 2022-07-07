using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Game.Hotfix;
using GameFramework;
using Huatuo.Generators;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using Debug = UnityEngine.Debug;

namespace Game.Editor
{
    public class HuatuoBuilderController
    {
        private const string HotfixDllPath = "Assets/Game/Hotfix";
        private const string HotfixDllName = "Game.Hotfix.dll";
        private const string HotfixDllBuildDir = "Temp/HuaTuo/Build";
        private const string AssembliesPostIl2CppStripDir = "HuatuoData/AssembliesPostIl2CppStrip";
        private const string MethodBridgeCppDir = "HuatuoData/LocalIl2CppData/il2cpp/libil2cpp/huatuo/interpreter";

        private readonly string m_InitBat;
        private readonly string m_SetVersionDat;
        private readonly string m_InitLocalBat;

        public string UnityInstallDirectory
        {
            get;
            set;
        }

        public string[] UnityVersionNames
        {
            get;
        }

        public string[] PlatformNames
        {
            get;
        }

        public HuatuoBuilderController()
        {
            m_InitBat = Application.dataPath + "/../HuatuoData/init_huatuo_repos.bat";
            m_SetVersionDat = Application.dataPath + "/../HuatuoData/set_version.bat";
            m_InitLocalBat = Application.dataPath + "/../HuatuoData/init_local_il2cpp_data.bat";

            UnityInstallDirectory = "C:/Software/UnityEditor/Unity 2020.3.33f1c2/Editor/Data/il2cpp";
            UnityVersionNames = new[]
            {
                "2020.3.33",
                "2021.3.1"
            };
            PlatformNames = Enum.GetNames(typeof(Platform));
        }

        public void InitHuatuoRepositories()
        {
            if (!File.Exists(m_InitBat))
            {
                Debug.LogErrorFormat("File not Exit : {0}", m_InitBat);
                return;
            }

            RunProcess(m_InitBat);
        }

        public void SetVersion(int versionIndex)
        {
            if (!File.Exists(m_SetVersionDat))
            {
                Debug.LogErrorFormat("File not Exit : {0}", m_SetVersionDat);
                return;
            }
            Debug.LogFormat(m_SetVersionDat);

            var fileLines = File.ReadAllLines(m_SetVersionDat);
            string version = UnityVersionNames[versionIndex];
            fileLines[1] = string.Format("set BRANCH={0}", version);
            File.WriteAllLines(m_SetVersionDat, fileLines, Encoding.UTF8);

            RunProcess(m_SetVersionDat);
        }

        public void InitLocalHuatuo()
        {
            if (!File.Exists(m_InitLocalBat))
            {
                Debug.LogErrorFormat("File not Exit : {0}", m_InitLocalBat);
                return;
            }
            Debug.LogFormat(m_InitLocalBat);

            var fileLines = File.ReadAllLines(m_InitLocalBat);
            Debug.LogFormat(fileLines[3]);
            string path = UnityInstallDirectory.Replace('/', '\\');
            fileLines[3] = string.Format("set IL2CPP_PATH={0}", path);
            File.WriteAllLines(m_InitLocalBat, fileLines, Encoding.UTF8);

            RunProcess(m_InitLocalBat);
        }

        public void BuildHotfixDll(int platformIndex)
        {
            Platform platform = (Platform)Enum.Parse(typeof(Platform), PlatformNames[platformIndex]);
            BuildTarget buildTarget = GetBuildTarget(platform);

            // Build Hotfix Dll
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(buildTarget);
            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = buildTarget;
            // scriptCompilationSettings.options = ScriptCompilationOptions.DevelopmentBuild;

            string buildPath = string.Format("{0}/{1}", HotfixDllBuildDir, buildTarget);
            IOUtility.CreateDirectoryIfNotExists(buildPath);
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildPath);
            foreach (var ass in scriptCompilationResult.assemblies)
            {
                Debug.LogFormat("Build assemblies : {0}", ass);
            }

            // Copy Hotfix Dll
            IOUtility.CreateDirectoryIfNotExists(HotfixDllPath);
            string oriFileName = string.Format("{0}/{1}", buildPath, HotfixDllName);
            string desFileName = string.Format("{0}/{1}.bytes", HotfixDllPath, HotfixDllName);
            File.Copy(oriFileName, desFileName, true);

            // Copy AOT Dll
            string aotDllPath = string.Format("{0}/{1}", AssembliesPostIl2CppStripDir, buildTarget);
            foreach (var dllName in GameHotfixEntry.AOTDllNames)
            {
                oriFileName = string.Format("{0}/{1}", aotDllPath, dllName);
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOT dll.");
                    continue;
                }
                desFileName = string.Format("{0}/{1}.bytes", HotfixDllPath, dllName);
                File.Copy(oriFileName, desFileName, true);
            }

            Debug.Log("Hotfix dll build complete.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void MethodBridge_X86_64()
        {
            string outputFile = string.Format("{0}/MethodBridge_x64.cpp", MethodBridgeCppDir);
            var g = new MethodBridgeGenerator(new MethodBridgeGeneratorOptions()
            {
                CallConvention = CallConventionType.X64,
                Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList(),
                OutputFile = outputFile,
            });

            g.PrepareMethods();
            g.Generate();
            Debug.LogFormat("== output:{0} ==", outputFile);
            CleanIl2CppBuildCache();
        }

        public void MethodBridge_Arm64()
        {
            string outputFile = string.Format("{0}/MethodBridge_arm64.cpp", MethodBridgeCppDir);
            var g = new MethodBridgeGenerator(new MethodBridgeGeneratorOptions()
            {
                CallConvention = CallConventionType.Arm64,
                Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList(),
                OutputFile = outputFile,
            });

            g.PrepareMethods();
            g.Generate();
            Debug.LogFormat("== output:{0} ==", outputFile);
            CleanIl2CppBuildCache();
        }

        private void CleanIl2CppBuildCache()
        {
            string il2cppBuildCachePath = "Library/Il2cppBuildCache";
            if (!Directory.Exists(il2cppBuildCachePath))
            {
                return;
            }
            Debug.Log($"clean il2cpp build cache:{il2cppBuildCachePath}");
            Directory.Delete(il2cppBuildCachePath, true);
        }

        private void RunProcess(string fileName)
        {
            using (Process p = new Process())
            {
                p.StartInfo.WorkingDirectory = Application.dataPath + "/../HuatuoData";
                p.StartInfo.FileName = fileName;
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();
            }
        }

        private BuildTarget GetBuildTarget(Platform platform)
        {
            switch (platform)
            {
                case Platform.Windows:
                    return BuildTarget.StandaloneWindows;

                case Platform.Windows64:
                    return BuildTarget.StandaloneWindows64;

                case Platform.MacOS:
#if UNITY_2017_3_OR_NEWER
                    return BuildTarget.StandaloneOSX;
#else
                    return BuildTarget.StandaloneOSXUniversal;
#endif
                case Platform.Linux:
                    return BuildTarget.StandaloneLinux64;

                case Platform.IOS:
                    return BuildTarget.iOS;

                case Platform.Android:
                    return BuildTarget.Android;

                case Platform.WindowsStore:
                    return BuildTarget.WSAPlayer;

                case Platform.WebGL:
                    return BuildTarget.WebGL;

                default:
                    throw new GameFrameworkException("Platform is invalid.");
            }
        }
    }
}
