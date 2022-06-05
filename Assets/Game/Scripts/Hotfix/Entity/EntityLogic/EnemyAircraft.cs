using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Hotfix
{
    public class EnemyAircraft : Aircraft
    {
        [SerializeField]
        private AircraftData m_AircraftData = null;

        private Vector3 m_TargetPosition;

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_AircraftData = userData as AircraftData;
            if (m_AircraftData == null)
            {
                Log.Error("Enemy aircraft data is invalid.");
            }

            m_TargetPosition = CachedTransform.localPosition;
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_AircraftData == null)
            {
                return;
            }

            Move(elapseSeconds);
            
            Attack();
        }

        public void MoveTo(Vector3 position)
        {
            m_TargetPosition = position;
        }

        private void Move(float elapseSeconds)
        {
            CachedTransform.localPosition = Vector3.LerpUnclamped(CachedTransform.localPosition, m_TargetPosition, m_AircraftData.Speed * elapseSeconds);
        }

        private void Attack()
        {
            for (int i = 0; i < m_Weapons.Count; i++)
            {
                m_Weapons[i].TryAttack();
            }
        }
    }
}
