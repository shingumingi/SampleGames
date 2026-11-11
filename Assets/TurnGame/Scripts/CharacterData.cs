using UnityEngine;

namespace TurnGame
{
    [CreateAssetMenu(menuName = "AlgoLab/Turn Game/Character Data")]
    public class CharacterData : ScriptableObject
    {
        public string characterId;
        public string displayName;
        public BattleTeam team;
        public int baseHP = 100;
        public int baseMP = 20;
        public int attack = 20;
        public int defense = 10;
        public int speed = 10;
        public int hpGrowth = 15;
        public int attackGrowth = 3;
        public int defenseGrowth = 2;
        public int speedGrowth = 1;
        public int experienceReward = 25;
        public Color bodyColor = Color.white;
        public BattleElement weakness;
        public BattleElement resistance;
        public SkillData[] skills;

        public int HPAtLevel(int level) => baseHP + hpGrowth * Mathf.Max(0, level - 1);
        public int AttackAtLevel(int level) => attack + attackGrowth * Mathf.Max(0, level - 1);
        public int DefenseAtLevel(int level) => defense + defenseGrowth * Mathf.Max(0, level - 1);
        public int SpeedAtLevel(int level) => speed + speedGrowth * Mathf.Max(0, level - 1);
    }
}
