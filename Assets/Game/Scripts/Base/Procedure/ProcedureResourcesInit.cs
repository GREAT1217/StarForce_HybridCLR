using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ProcedureResourcesInit : ProcedureBase
    {
        private bool m_InitResourcesComplete = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_InitResourcesComplete = false;

            // 注意：使用单机模式并初始化资源前，需要先构建 AssetBundle 并复制到 StreamingAssets 中，否则会产生 HTTP 404 错误
            GameEntry.Resource.InitResources(OnInitResourcesComplete);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_InitResourcesComplete)
            {
                // 初始化资源未完成则继续等待
                return;
            }

            ChangeState<ProcedureCodeInit>(procedureOwner);
        }

        private void OnInitResourcesComplete()
        {
            m_InitResourcesComplete = true;
            Log.Info("Init resources complete.");
        }
    }
}
