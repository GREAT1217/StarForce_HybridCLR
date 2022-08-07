using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Editor;
using Game.Hotfix;
using GameFramework;
using HybridCLR.Generators;
using HybridCLR.Generators.MethodBridge;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using Debug = UnityEngine.Debug;

namespace HybridCLR.Editor.Builder
{
    public class BuilderController
    {
        private const string HotfixDllPath = "Assets/Game/Hotfix";
        private const string HotfixDllName = "Game.Hotfix.dll";

        private readonly string m_InitBatTemplate;
        private readonly string m_InitBatTemp;
        private string m_UnityInstallDirectory;

        public string UnityInstallDirectory
        {
            get
            {
                return m_UnityInstallDirectory;
            }
            set
            {
                m_UnityInstallDirectory = value;
                if (!string.IsNullOrEmpty(m_UnityInstallDirectory))
                {
                    EditorPrefs.SetString("UnityInstallDirectory", m_UnityInstallDirectory);
                }
            }
        }

        public string[] VersionNames
        {
            get;
        }

        public string[] VersionValues
        {
            get;
        }

        public string[] PlatformNames
        {
            get;
        }

        public BuilderController()
        {
            m_InitBatTemplate = Application.dataPath + "/../HybridCLRData/init_local_il2cpp_data.bat";
            m_InitBatTemp = Application.dataPath + "/../HybridCLRData/init_local_il2cpp_data_temp.bat";

            m_UnityInstallDirectory = EditorPrefs.GetString("UnityInstallDirectory");

            VersionNames = new[]
            {
                "2020.3.x",
                "2021.3.x"
            };

            VersionValues = new[]
            {
                "2020.3.33",
                "2021.3.1"
            };

            PlatformNames = Enum.GetNames(typeof(Platform));
        }

        #region InitHybridCLR

        public void InitHybridCLR(int versionIndex)
        {
            if (!File.Exists(m_InitBatTemplate))
            {
                Debug.LogErrorFormat("File not Exit : {0}", m_InitBatTemplate);
                return;
            }

            string command = File.ReadAllText(m_InitBatTemplate);
            command = command.Replace("__VERSION__", VersionValues[versionIndex]);
            command = command.Replace("__PATH__", UnityInstallDirectory);
            File.WriteAllText(m_InitBatTemp, command);

            RunProcess(m_InitBatTemp);
        }

        private void RunProcess(string fileName)
        {
            using (Process p = new Process())
            {
                p.StartInfo.WorkingDirectory = Application.dataPath + "/../HybridCLRData";
                p.StartInfo.FileName = fileName;
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();
            }
        }

        #endregion

        #region CompoileDll

        public void CompileHotfixDll(int platformIndex)
        {
            Platform platform = (Platform)Enum.Parse(typeof(Platform), PlatformNames[platformIndex]);
            BuildTarget buildTarget = PlatformUtility.GetBuildTarget(platform);

            // Build Hotfix Dll
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(buildTarget);
            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = buildTarget;
            // scriptCompilationSettings.options = ScriptCompilationOptions.DevelopmentBuild;

            string buildPath = BuildConfig.GetHotFixDllsOutputDirByTarget(buildTarget);
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
            string aotDllPath = string.Format("{0}/{1}", BuildConfig.AssembliesPostIl2CppStripDir, buildTarget);
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

        #endregion

        #region MethodBridge

        public void MethodBridge_Universal32()
        {
            GenerateMethodBridgeCppFile(PlatformABI.Universal32, "MethodBridge_Universal32");
        }

        public void MethodBridge_Universal64()
        {
            GenerateMethodBridgeCppFile(PlatformABI.Universal64, "MethodBridge_Universal64");
        }

        public void MethodBridge_Arm64()
        {
            GenerateMethodBridgeCppFile(PlatformABI.Arm64, "MethodBridge_Arm64");
        }

        private static void CleanIl2CppBuildCache()
        {
            string il2cppBuildCachePath = "Library/Il2cppBuildCache";
            if (!Directory.Exists(il2cppBuildCachePath))
            {
                return;
            }
            Debug.Log($"clean il2cpp build cache:{il2cppBuildCachePath}");
            Directory.Delete(il2cppBuildCachePath, true);
        }

        private static List<Assembly> CollectDependentAssemblies(Dictionary<string, Assembly> allAssByName, List<Assembly> dlls)
        {
            for (int i = 0; i < dlls.Count; i++)
            {
                Assembly ass = dlls[i];
                foreach (var depAssName in ass.GetReferencedAssemblies())
                {
                    if (!allAssByName.ContainsKey(depAssName.Name))
                    {
                        Debug.Log($"ignore ref assembly:{depAssName.Name}");
                        continue;
                    }
                    Assembly depAss = allAssByName[depAssName.Name];
                    if (!dlls.Contains(depAss))
                    {
                        dlls.Add(depAss);
                    }
                }
            }
            return dlls;
        }

        private static List<Assembly> GetScanAssemblies()
        {
            var allAssByName = new Dictionary<string, Assembly>();
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                allAssByName[ass.GetName().Name] = ass;
            }

            var rootAssemblies = BuildConfig.AllHotUpdateDllNames
                .Select(dll => Path.GetFileNameWithoutExtension(dll)).Concat(GeneratorConfig.GetExtraAssembiles())
                .Where(name => allAssByName.ContainsKey(name)).Select(name => allAssByName[name]).ToList();
            CollectDependentAssemblies(allAssByName, rootAssemblies);
            rootAssemblies.Sort((a, b) => a.GetName().Name.CompareTo(b.GetName().Name));
            Debug.Log($"assembly count:{rootAssemblies.Count}");
            foreach (var ass in rootAssemblies)
            {
                Debug.Log($"scan assembly:{ass.GetName().Name}");
            }
            return rootAssemblies;
        }

        private static void GenerateMethodBridgeCppFile(PlatformABI platform, string fileName)
        {
            string outputFile = $"{BuildConfig.MethodBridgeCppDir}/{fileName}.cpp";
            var g = new MethodBridgeGenerator(new MethodBridgeGeneratorOptions()
            {
                CallConvention = platform,
                Assemblies = GetScanAssemblies(),
                OutputFile = outputFile,
            });

            g.PrepareMethods();
            g.Generate();
            Debug.LogFormat("== output:{0} ==", outputFile);
            CleanIl2CppBuildCache();
        }

        #endregion
    }
}
