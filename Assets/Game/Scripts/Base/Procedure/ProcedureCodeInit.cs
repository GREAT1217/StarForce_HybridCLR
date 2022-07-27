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
            Assembly hotfixAssembly  = Assembly.Load(dll.bytes);
            Log.Info("Load hotfix dll OK.");
            StartHotfix(hotfixAssembly);
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
