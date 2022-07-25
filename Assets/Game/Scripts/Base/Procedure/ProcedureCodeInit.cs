using System;
using System.Linq;
using System.Reflection;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ProcedureCodeInit : ProcedureBase
    {
        public static readonly string[] AOTDllNames =
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll", // 如果使用了Linq，需要这个
            "UnityGameFramework.Runtime.dll"
            // "Newtonsoft.Json.dll",
            // "protobuf-net.dll",
            // "Google.Protobuf.dll",
            // "MongoDB.Bson.dll",
            // "DOTween.Modules.dll",
            // "UniTask.dll",
        };

        private int AOTFlag;
        private int AOTLoadFlag;

        private Assembly m_HotfixAssembly;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

#if UNITY_EDITOR
            Assembly hotfixAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "Game.Hotfix");
            StartHotfix(hotfixAssembly);
#else
            GameEntry.Resource.LoadAsset("Assets/Game/Hotfix/Game.Hotfix.dll.bytes", new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFail));
#endif
        }

        private void OnLoadAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            TextAsset dll = (TextAsset)asset;
            m_HotfixAssembly = Assembly.Load(dll.bytes);
            Log.Info("Load hotfix dll OK.");

            AOTFlag = AOTDllNames.Length;
            AOTLoadFlag = 0;
            for (int i = 0; i < AOTFlag; i++)
            {
                string dllAssetName = Utility.Text.Format("Assets/Game/Hotfix/{0}.bytes", AOTDllNames[i]);
                GameEntry.Resource.LoadAsset(dllAssetName, new LoadAssetCallbacks(OnLoadAOTDllSuccess, OnLoadAssetFail));
            }
        }

        private unsafe void OnLoadAOTDllSuccess(string assetName, object asset, float duration, object userdata)
        {
            TextAsset dll = (TextAsset)asset;
            byte[] dllBytes = dll.bytes;
            fixed (byte* ptr = dllBytes)
            {
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                int err = HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly((IntPtr)ptr, dllBytes.Length);
                Log.Info($"LoadMetadataForAOTAssembly:{assetName}. ret:{err}");
            }
            AOTLoadFlag++;
            if (AOTLoadFlag == AOTFlag)
            {
                StartHotfix(m_HotfixAssembly);
            }
        }

        private void OnLoadAssetFail(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load hotfix dll failed. " + errorMessage);
        }

        private void StartHotfix(Assembly hotfixAssembly)
        {
            var hotfixEntry = hotfixAssembly.GetType("Game.Hotfix.GameHotfixEntry");
            var start = hotfixEntry.GetMethod("Start");
            start?.Invoke(null, null);
        }
    }
}
