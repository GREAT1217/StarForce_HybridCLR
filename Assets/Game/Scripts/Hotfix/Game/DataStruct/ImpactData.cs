namespace Game.Hotfix
{
    public class ImpactData
    {
        private CampType m_Camp;
        private int m_HP;
        private int m_Attack;
        private int m_Defense;

        public ImpactData(CampType camp, int hp, int attack, int defense)
        {
            m_Camp = camp;
            m_HP = hp;
            m_Attack = attack;
            m_Defense = defense;
        }

        public CampType Camp
        {
            get
            {
                return m_Camp;
            }
        }

        public int HP
        {
            get
            {
                return m_HP;
            }
        }

        public int Attack
        {
            get
            {
                return m_Attack;
            }
        }

        public int Defense
        {
            get
            {
                return m_Defense;
            }
        }
    }
}
