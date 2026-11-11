using UnityEngine;

namespace TurnGame
{
    public class TurnGameCharacter : MonoBehaviour
    {
        public CharacterData Data { get; private set; }
        public int Level { get; private set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public float NextActionTime { get; set; }
        public bool Guarding { get; set; }
        public bool IsAlive => HP > 0;
        public int MaxHP => Data.HPAtLevel(Level);
        public int Attack => Data.AttackAtLevel(Level);
        public int Defense => Data.DefenseAtLevel(Level);
        public int Speed => Data.SpeedAtLevel(Level);
        public Vector3 HomePosition { get; set; }

        public void Setup(CharacterData data, int level, Vector3 home)
        {
            Data = data;
            Level = Mathf.Max(1, level);
            HP = MaxHP;
            MP = data.baseMP + (Level - 1) * 3;
            HomePosition = home;
            transform.position = home + Vector3.up * 0.1f;
            BuildModel();
            RefreshVisual();
        }

        void BuildModel()
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = Vector3.up * 1.05f;
            body.transform.localScale = new Vector3(0.72f, 0.9f, 0.72f);
            body.GetComponent<Renderer>().material.color = Data.bodyColor;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(transform, false);
            head.transform.localPosition = Vector3.up * 2.35f;
            head.transform.localScale = Vector3.one * 0.78f;
            head.GetComponent<Renderer>().material.color = new Color(1f, 0.78f, 0.62f);

            var weapon = GameObject.CreatePrimitive(Data.team == BattleTeam.Heroes ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            weapon.name = "Weapon";
            weapon.transform.SetParent(transform, false);
            weapon.transform.localPosition = new Vector3(Data.team == BattleTeam.Heroes ? 0.65f : -0.65f, 1.15f, 0f);
            weapon.transform.localScale = new Vector3(0.14f, 0.75f, 0.14f);
            weapon.GetComponent<Renderer>().material.color = Color.white * 0.8f;

        }

        public void RefreshVisual()
        {
            // 상태 표시는 TurnGameController의 화면 고정 2D UI가 담당합니다.
        }

        public BattleSnapshot Snapshot() => new BattleSnapshot
        {
            Id = Data.characterId, Speed = Speed, NextActionTime = NextActionTime,
            IsAlive = IsAlive, HP = HP, MaxHP = MaxHP, Team = Data.team
        };
    }
}
