using UnityEngine;

namespace TurnGame
{
    [CreateAssetMenu(menuName = "AlgoLab/Turn Game/Skill Data")]
    public class SkillData : ScriptableObject
    {
        public string skillId;
        public string displayName;
        [TextArea] public string description;
        public int mpCost;
        public int power = 100;
        public int speedBonus;
        public SkillTarget target = SkillTarget.OneEnemy;
        public Color effectColor = Color.white;
        public BattleElement element;
        public TurnEffectType turnEffect;
        public float effectAmount;
    }
}
