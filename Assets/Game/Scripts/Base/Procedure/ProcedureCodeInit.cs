using System.Linq;
using System.Reflection;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ProcedureCodeInit : ProcedureBase
    {
        private Assembly m_HotfixAssembly;
        private bool m_IsLoadedDll = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

#if UNITY_EDITOR
            m_HotfixAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "Game.Hotfix");
            m_IsLoadedDll = true;
#else
            m_IsLoadedDll = false;
            GameEntry.Resource.LoadAsset("Assets/Game/Hotfix/Game.Hotfix.dll.bytes", new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFail));
#endif
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            m_IsLoadedDll = false;

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_IsLoadedDll)
            {
                return;
            }

            var hotfixEntry = m_HotfixAssembly.GetType("Game.Hotfix.GameHotfixEntry");
            var start = hotfixEntry.GetMethod("Start");
            start?.Invoke(null, null);
        }

        private void OnLoadAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            TextAsset dll = (TextAsset)asset;
            m_HotfixAssembly = Assembly.Load(dll.bytes);
            Log.Info("Load hotfix dll OK.");
            m_IsLoadedDll = true;
        }

        private void OnLoadAssetFail(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load hotfix dll failed. " + errorMessage);
        }
    }
}
