using System;
using UnityEngine;

namespace TurnGame
{
    public enum BattleTeam { Heroes, Enemies }
    public enum SkillTarget { OneEnemy, AllEnemies, OneAlly }
    public enum BattleElement { None, Fire, Ice, Lightning }
    public enum TurnEffectType { None, Haste, Slow, Stun, Delay }

    [Serializable]
    public struct BattleSnapshot
    {
        public string Id;
        public int Speed;
        public float NextActionTime;
        public bool IsAlive;
        public int HP;
        public int MaxHP;
        public BattleTeam Team;
        public int Attack;
        public int Defense;
        public int MP;
        public bool Guarding;
        public BattleElement Weakness;
        public BattleElement Resistance;
    }

    [Serializable]
    public struct DamageRequest
    {
        public int Attack;
        public int Defense;
        public int SkillPower;
        public bool Guarding;
        public BattleElement SkillElement;
        public BattleElement Weakness;
        public BattleElement Resistance;
        public float RandomValue;
    }

    [Serializable]
    public struct DamageResult { public int Damage; public bool IsCritical; public float ElementMultiplier; }

    [Serializable]
    public struct BattleRankInput
    {
        public int TurnCount;
        public int SurvivingHeroes;
        public int TotalHeroes;
        public int DamageTaken;
        public int SkillUseCount;
    }

    [Serializable]
    public struct ExperienceResult
    {
        public int Level;
        public int RemainingExperience;
        public int LevelsGained;
    }
}
