using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TurnGame
{
    public class TurnGameController : MonoBehaviour
    {
        public CharacterData[] heroData;
        public CharacterData[] enemyData;
        public CharacterData reserveHero;
        public CharacterData reserveEnemy;

        readonly List<TurnGameCharacter> units = new();
        ITurnScheduler scheduler;
        ITargetSelector targetSelector;
        IExperienceCalculator experienceCalculator;
        IDamageCalculator damageCalculator;
        ITurnEffectProcessor turnEffectProcessor;
        IEnemyDecisionAI enemyAI;
        IBattleRankCalculator rankCalculator;
        TurnGameCharacter active;
        bool waitingForPlayer;
        bool animating;
        bool selectingTarget;
        int stage = 1;
        string message = "전투 준비";
        int selectedCommand = -1;
        int selectedTargetIndex = -1;
        int turnCount;
        int damageTaken;
        int skillUseCount;

        void Start()
        {
            scheduler = TurnGameAlgorithmFactory.Create<ITurnScheduler>(new DefaultTurnScheduler());
            targetSelector = TurnGameAlgorithmFactory.Create<ITargetSelector>(new DefaultTargetSelector());
            experienceCalculator = TurnGameAlgorithmFactory.Create<IExperienceCalculator>(new DefaultExperienceCalculator());
            damageCalculator = TurnGameAlgorithmFactory.Create<IDamageCalculator>(new DefaultDamageCalculator());
            turnEffectProcessor = TurnGameAlgorithmFactory.Create<ITurnEffectProcessor>(new DefaultTurnEffectProcessor());
            enemyAI = TurnGameAlgorithmFactory.Create<IEnemyDecisionAI>(new DefaultEnemyAI());
            rankCalculator = TurnGameAlgorithmFactory.Create<IBattleRankCalculator>(new DefaultRankCalculator());
            stage = PlayerPrefs.GetInt("TG_STAGE", 1);
            StartBattle();
        }

        void StartBattle()
        {
            foreach (var unit in units) if (unit) Destroy(unit.gameObject);
            units.Clear(); active = null; waitingForPlayer = false; animating = false;
            selectingTarget = false; selectedCommand = -1; selectedTargetIndex = -1;
            turnCount = 0; damageTaken = 0; skillUseCount = 0;
            for (int i = 0; i < heroData.Length; i++) Spawn(heroData[i], HeroLevel(heroData[i]), new Vector3(-5.2f, 0f, -2.4f + i * 2.4f), true);
            for (int i = 0; i < enemyData.Length; i++) Spawn(enemyData[i], Mathf.Max(1, stage), new Vector3(5.2f, 0f, -2.4f + i * 2.4f), true);
            message = $"STAGE {stage}  전투 시작";
            StartCoroutine(BattleLoop());
        }

        void Spawn(CharacterData data, int level, Vector3 home, bool inbound)
        {
            var go = new GameObject(data.displayName);
            var unit = go.AddComponent<TurnGameCharacter>();
            unit.Setup(data, level, home);
            unit.NextActionTime = scheduler.CalculateDelay(unit.Speed, 0);
            units.Add(unit);
            if (inbound) StartCoroutine(Inbound(unit));
        }

        IEnumerator Inbound(TurnGameCharacter unit)
        {
            Vector3 target = unit.HomePosition;
            Vector3 start = target + new Vector3(unit.Data.team == BattleTeam.Heroes ? -6f : 6f, 0f, 0f);
            unit.transform.position = start;
            float t = 0f;
            while (t < 1f) { t += Time.deltaTime * 1.6f; unit.transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t)); yield return null; }
        }

        IEnumerator BattleLoop()
        {
            yield return new WaitForSeconds(1.2f);
            while (HeroesAlive() && EnemiesAlive())
            {
                while (animating) yield return null;
                var snapshots = units.Select(u => u.Snapshot()).ToList();
                int index = scheduler.SelectNext(snapshots);
                if (index < 0 || index >= units.Count) { yield return new WaitForSeconds(0.1f); continue; }
                active = units[index];
                float now = active.NextActionTime;
                foreach (var u in units) u.NextActionTime = Mathf.Max(0f, u.NextActionTime - now);
                if (active.Data.team == BattleTeam.Heroes)
                {
                    waitingForPlayer = true; message = $"{active.Data.displayName}의 행동 선택";
                    while (waitingForPlayer && HeroesAlive() && EnemiesAlive()) yield return null;
                }
                else
                {
                    selectedCommand = 0;
                    yield return new WaitForSeconds(0.35f);
                    int target = enemyAI.SelectTarget(units.Select(u => u.Snapshot()).ToList(), BattleElement.None);
                    if (target >= 0) yield return PerformAction(active, units[target], null);
                }
                active.NextActionTime = scheduler.CalculateDelay(active.Speed, selectedCommand == 1 ? 2 : 0);
                turnCount++;
                yield return new WaitForSeconds(0.25f);
            }
            if (HeroesAlive()) yield return Victory(); else message = "패배  R 키로 다시 도전";
        }

        void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (!waitingForPlayer || animating || active == null) return;
            if (selectingTarget)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame) MoveTarget(-1);
                if (Keyboard.current.rightArrowKey.wasPressedThisFrame) MoveTarget(1);
                if (Keyboard.current.enterKey.wasPressedThisFrame) ConfirmTarget();
                if (Keyboard.current.escapeKey.wasPressedThisFrame) CancelTarget();
                return;
            }
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ChooseCommand(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ChooseCommand(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ChooseCommand(2);
        }

        void ChooseCommand(int command)
        {
            selectedCommand = command;
            if (command == 2)
            {
                active.Guarding = true; message = $"{active.Data.displayName} 방어";
                waitingForPlayer = false; selectedCommand = -1; return;
            }
            SkillData skill = command == 1 && active.Data.skills != null && active.Data.skills.Length > 0 ? active.Data.skills[0] : null;
            if (skill != null && active.MP < skill.mpCost) { message = "MP가 부족합니다"; return; }
            selectingTarget = true;
            selectedTargetIndex = targetSelector.SelectTarget(units.Select(u => u.Snapshot()).ToList(), BattleTeam.Enemies);
            if (selectedTargetIndex < 0) { selectingTarget = false; return; }
            message = "← → 대상 선택   ENTER 확정   ESC 취소";
        }

        void MoveTarget(int direction)
        {
            List<int> targets = new();
            for (int i = 0; i < units.Count; i++)
                if (units[i] != null && units[i].IsAlive && units[i].Data.team == BattleTeam.Enemies)
                    targets.Add(i);
            if (targets.Count == 0) { selectedTargetIndex = -1; return; }
            int current = targets.IndexOf(selectedTargetIndex);
            if (current < 0) current = 0;
            current = (current + direction + targets.Count) % targets.Count;
            selectedTargetIndex = targets[current];
            message = $"대상: {units[selectedTargetIndex].Data.displayName}   ENTER 확정";
        }

        void ConfirmTarget()
        {
            if (selectedTargetIndex < 0 || selectedTargetIndex >= units.Count || !units[selectedTargetIndex].IsAlive) return;
            SkillData skill = selectedCommand == 1 && active.Data.skills != null && active.Data.skills.Length > 0 ? active.Data.skills[0] : null;
            selectingTarget = false;
            waitingForPlayer = false;
            StartCoroutine(PerformAction(active, units[selectedTargetIndex], skill));
            selectedTargetIndex = -1;
        }

        void CancelTarget()
        {
            selectingTarget = false;
            selectedTargetIndex = -1;
            selectedCommand = -1;
            message = $"{active.Data.displayName}의 행동 선택";
        }

        IEnumerator PerformAction(TurnGameCharacter attacker, TurnGameCharacter target, SkillData skill)
        {
            animating = true; attacker.Guarding = false;
            if (skill != null) attacker.MP -= skill.mpCost;
            Vector3 start = attacker.HomePosition;
            Vector3 strike = target.HomePosition + new Vector3(attacker.Data.team == BattleTeam.Heroes ? -1.5f : 1.5f, 0f, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime * 4f) { attacker.transform.position = Vector3.Lerp(start, strike, t); yield return null; }
            if (skill != null) skillUseCount++;
            DamageResult result = damageCalculator.Calculate(new DamageRequest { Attack=attacker.Attack, Defense=target.Defense, SkillPower=skill==null?100:skill.power, Guarding=target.Guarding, SkillElement=skill==null?BattleElement.None:skill.element, Weakness=target.Data.weakness, Resistance=target.Data.resistance, RandomValue=Random.value });
            int damage = result.Damage;
            target.Guarding = false;
            if (target.Data.team == BattleTeam.Heroes) damageTaken += damage;
            if (skill != null && skill.turnEffect != TurnEffectType.None) target.NextActionTime = turnEffectProcessor.Apply(target.NextActionTime, skill.turnEffect, skill.effectAmount);
            target.HP = Mathf.Max(0, target.HP - damage); target.RefreshVisual();
            message = $"{attacker.Data.displayName} {(skill == null ? "공격" : skill.displayName)}  {damage} 피해" + (result.IsCritical ? " CRITICAL!" : result.ElementMultiplier > 1f ? " WEAK!" : result.ElementMultiplier < 1f ? " RESIST" : "");
            yield return new WaitForSeconds(0.25f);
            for (float t = 0f; t < 1f; t += Time.deltaTime * 4f) { attacker.transform.position = Vector3.Lerp(strike, start, t); yield return null; }
            attacker.transform.position = start;
            if (!target.IsAlive) yield return Outbound(target);
            animating = false;
        }

        IEnumerator Outbound(TurnGameCharacter unit)
        {
            message = $"{unit.Data.displayName} 전투 불능";
            Vector3 start = unit.transform.position;
            Vector3 end = start + new Vector3(unit.Data.team == BattleTeam.Heroes ? -7f : 7f, -2f, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime * 1.8f) { unit.transform.position = Vector3.Lerp(start, end, t); yield return null; }
            TryReinforcement(unit.Data.team, unit.HomePosition);
        }

        void TryReinforcement(BattleTeam team, Vector3 home)
        {
            CharacterData reserve = team == BattleTeam.Heroes ? reserveHero : reserveEnemy;
            if (reserve == null || units.Any(u => u.Data == reserve)) return;
            Spawn(reserve, team == BattleTeam.Heroes ? HeroLevel(reserve) : stage + 1, home, true);
            message = $"{reserve.displayName} 증원 등장";
        }

        IEnumerator Victory()
        {
            int reward = units.Where(u => u.Data.team == BattleTeam.Enemies).Sum(u => u.Data.experienceReward);
            foreach (var hero in units.Where(u => u.Data.team == BattleTeam.Heroes && u.IsAlive))
            {
                int level = HeroLevel(hero.Data); int exp = HeroExperience(hero.Data);
                ExperienceResult result = experienceCalculator.AddExperience(level, exp, reward);
                PlayerPrefs.SetInt("TG_LV_" + hero.Data.characterId, result.Level);
                PlayerPrefs.SetInt("TG_XP_" + hero.Data.characterId, result.RemainingExperience);
                if (result.LevelsGained > 0) message = $"{hero.Data.displayName} LEVEL UP!  Lv.{result.Level}";
            }
            stage++; PlayerPrefs.SetInt("TG_STAGE", stage); PlayerPrefs.Save();
            yield return new WaitForSeconds(1.4f);
            int survivors = units.Count(u => u.Data.team == BattleTeam.Heroes && u.IsAlive);
            int totalHeroes = units.Count(u => u.Data.team == BattleTeam.Heroes);
            string rank = rankCalculator.CalculateRank(new BattleRankInput { TurnCount=turnCount, SurvivingHeroes=survivors, TotalHeroes=totalHeroes, DamageTaken=damageTaken, SkillUseCount=skillUseCount });
            message = $"승리! RANK {rank}  경험치 {reward} 획득  N 키로 STAGE {stage}";
            while (Keyboard.current == null || !Keyboard.current.nKey.wasPressedThisFrame) yield return null;
            StartBattle();
        }

        int HeroLevel(CharacterData data) => PlayerPrefs.GetInt("TG_LV_" + data.characterId, 1);
        int HeroExperience(CharacterData data) => PlayerPrefs.GetInt("TG_XP_" + data.characterId, 0);
        bool HeroesAlive() => units.Any(u => u.Data.team == BattleTeam.Heroes && u.IsAlive);
        bool EnemiesAlive() => units.Any(u => u.Data.team == BattleTeam.Enemies && u.IsAlive);

        void OnGUI()
        {
            DrawTurnTimeline();
            DrawUnitStatusPanels();
            DrawBattleMessage();
            if (waitingForPlayer && active != null) DrawCommandMenu();
        }

        void DrawTurnTimeline()
        {
            List<TurnGameCharacter> alive = units.Where(u => u != null && u.IsAlive).ToList();
            if (alive.Count == 0) return;
            float left = 130f;
            float actionX = Screen.width - 92f;
            float queueRight = actionX - 12f;
            float width = Mathf.Max(300f, queueRight - left);
            float lineY = 88f;
            DrawRect(new Rect(left, lineY, width, 7f), new Color(0.12f, 0.17f, 0.24f, 0.96f));
            DrawRect(new Rect(actionX, 40f, 5f, 58f), new Color(1f, 0.82f, 0.2f, 1f));
            GUI.Label(new Rect(actionX - 30f, 12f, 95f, 28f), "ACTION", CenterStyle(14, Color.yellow));
            GUI.Label(new Rect(24f, 23f, 120f, 28f), $"STAGE {stage}", CenterStyle(18, Color.white));

            var ordered = alive.OrderBy(u => u.NextActionTime).ThenByDescending(u => u.Speed).ToList();
            float gap = 6f;
            float cardWidth = Mathf.Clamp((width - gap * Mathf.Max(0, ordered.Count - 1)) / Mathf.Max(1, ordered.Count), 62f, 104f);
            float totalWidth = cardWidth * ordered.Count + gap * Mathf.Max(0, ordered.Count - 1);
            float startX = queueRight - totalWidth;
            for (int i = 0; i < ordered.Count; i++)
            {
                TurnGameCharacter unit = ordered[i];
                int visualSlot = ordered.Count - 1 - i;
                float x = startX + visualSlot * (cardWidth + gap);
                float y = 43f;
                Color color = unit.Data.team == BattleTeam.Heroes
                    ? new Color(0.2f, 0.65f, 1f, 0.97f)
                    : new Color(1f, 0.28f, 0.32f, 0.97f);
                DrawRect(new Rect(x, y, cardWidth, 43f), color);
                GUI.Label(new Rect(x, y, cardWidth, 43f), $"{i + 1} {unit.Data.displayName}\nSPD {unit.Speed}", CenterStyle(cardWidth < 75f ? 9 : 11, Color.white));
                if (unit.Data.team == BattleTeam.Enemies && GUI.Button(new Rect(x, y, cardWidth, 43f), GUIContent.none, GUIStyle.none))
                    ClickAttackTarget(unit);
            }
        }

        void DrawUnitStatusPanels()
        {
            Camera camera = Camera.main;
            if (camera == null) return;
            foreach (TurnGameCharacter unit in units)
            {
                if (unit == null || !unit.IsAlive) continue;
                Vector3 screen = camera.WorldToScreenPoint(unit.transform.position + Vector3.up * 3.25f);
                if (screen.z <= 0f) continue;
                float x = screen.x - 72f;
                float y = Screen.height - screen.y - 58f;
                int unitIndex = units.IndexOf(unit);
                bool selected = selectingTarget && unitIndex == selectedTargetIndex;
                if (selected) DrawRect(new Rect(x - 4f, y - 4f, 152f, 64f), new Color(1f, 0.78f, 0.12f, 1f));
                Rect panel = new Rect(x, y, 144f, 56f);
                DrawRect(panel, new Color(0.025f, 0.04f, 0.07f, 0.94f));
                GUI.Label(new Rect(x + 4f, y + 2f, 136f, 20f), $"{unit.Data.displayName}  Lv.{unit.Level}", CenterStyle(12, Color.white));
                float hpRatio = Mathf.Clamp01((float)unit.HP / Mathf.Max(1, unit.MaxHP));
                DrawRect(new Rect(x + 8f, y + 26f, 128f, 10f), new Color(0.18f, 0.08f, 0.09f, 1f));
                DrawRect(new Rect(x + 8f, y + 26f, 128f * hpRatio, 10f), new Color(0.18f, 0.9f, 0.35f, 1f));
                GUI.Label(new Rect(x + 8f, y + 36f, 128f, 18f), $"HP {unit.HP}/{unit.MaxHP}   MP {unit.MP}", CenterStyle(10, Color.white));
                if (selected) GUI.Label(new Rect(x, y - 25f, 144f, 22f), "▼ TARGET", CenterStyle(13, Color.yellow));
                if (unit.Data.team == BattleTeam.Enemies && GUI.Button(panel, GUIContent.none, GUIStyle.none))
                    ClickAttackTarget(unit);
            }
        }

        void DrawBattleMessage()
        {
            float width = Mathf.Min(720f, Screen.width - 40f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 82f, width, 54f);
            DrawRect(rect, new Color(0.02f, 0.035f, 0.065f, 0.94f));
            GUI.Label(rect, message + "\n[1] 공격   [2] 스킬   [3] 방어   [R] 재시작", CenterStyle(15, Color.white));
        }

        void DrawCommandMenu()
        {
            float y = Screen.height - 148f;
            float x = (Screen.width - 430f) * 0.5f;
            string skillName = active.Data.skills != null && active.Data.skills.Length > 0 ? active.Data.skills[0].displayName : "스킬";
            if (DrawCommandBox(new Rect(x, y, 130f, 54f), "1  공격", selectedCommand == 0)) ChooseCommand(0);
            if (DrawCommandBox(new Rect(x + 150f, y, 130f, 54f), "2  " + skillName, selectedCommand == 1)) ChooseCommand(1);
            if (DrawCommandBox(new Rect(x + 300f, y, 130f, 54f), "3  방어", selectedCommand == 2)) ChooseCommand(2);
            if (selectingTarget)
                GUI.Label(new Rect(x - 20f, y - 32f, 470f, 26f), "← → TARGET    ENTER 결정    ESC 취소", CenterStyle(14, Color.yellow));
        }

        static bool DrawCommandBox(Rect rect, string text, bool selected)
        {
            DrawRect(rect, selected ? new Color(1f, 0.75f, 0.18f, 0.96f) : new Color(0.12f, 0.2f, 0.3f, 0.96f));
            GUI.Label(rect, text, CenterStyle(16, selected ? Color.black : Color.white));
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        void ClickAttackTarget(TurnGameCharacter target)
        {
            if (!waitingForPlayer || animating || target == null || !target.IsAlive) return;
            int index = units.IndexOf(target);
            if (index < 0) return;
            if (!selectingTarget) ChooseCommand(0);
            if (!selectingTarget) return;
            selectedTargetIndex = index;
            ConfirmTarget();
        }

        static void DrawRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        static GUIStyle CenterStyle(int size, Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = size;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = color;
            return style;
        }

        sealed class DefaultTurnScheduler : ITurnScheduler
        {
            public int SelectNext(IReadOnlyList<BattleSnapshot> list) { int best = -1; for (int i = 0; i < list.Count; i++) if (list[i].IsAlive && (best < 0 || list[i].NextActionTime < list[best].NextActionTime)) best = i; return best; }
            public float CalculateDelay(int speed, int bonus) => 100f / Mathf.Max(1, speed + bonus);
        }
        sealed class DefaultTargetSelector : ITargetSelector
        {
            public int SelectTarget(IReadOnlyList<BattleSnapshot> list, BattleTeam team) { for (int i = 0; i < list.Count; i++) if (list[i].IsAlive && list[i].Team == team) return i; return -1; }
        }
        sealed class DefaultExperienceCalculator : IExperienceCalculator
        {
            public int RequiredExperience(int level) => 100 + (level - 1) * 50;
            public ExperienceResult AddExperience(int level, int current, int gained) { current += gained; int count = 0; while (current >= RequiredExperience(level)) { current -= RequiredExperience(level); level++; count++; } return new ExperienceResult { Level = level, RemainingExperience = current, LevelsGained = count }; }
        }
        sealed class DefaultDamageCalculator : IDamageCalculator
        {
            public DamageResult Calculate(DamageRequest r) { float e=r.SkillElement!=BattleElement.None&&r.SkillElement==r.Weakness?1.5f:r.SkillElement!=BattleElement.None&&r.SkillElement==r.Resistance?0.5f:1f; bool c=r.RandomValue<0.1f; float d=Mathf.Max(1,r.Attack*r.SkillPower/100f-r.Defense*0.5f)*e*(c?1.5f:1f)*(r.Guarding?0.5f:1f); return new DamageResult{Damage=Mathf.Max(1,Mathf.RoundToInt(d)),IsCritical=c,ElementMultiplier=e}; }
        }
        sealed class DefaultTurnEffectProcessor : ITurnEffectProcessor
        {
            public float Apply(float d, TurnEffectType e, float a) { if(e==TurnEffectType.Haste)d-=a; else if(e==TurnEffectType.Stun)d+=Mathf.Max(5,a); else if(e!=TurnEffectType.None)d+=a; return Mathf.Max(0,d); }
        }
        sealed class DefaultEnemyAI : IEnemyDecisionAI
        {
            public int SelectTarget(IReadOnlyList<BattleSnapshot> u, BattleElement e) { int b=-1;float s=float.MinValue;for(int i=0;i<u.Count;i++){if(!u[i].IsAlive||u[i].Team!=BattleTeam.Heroes)continue;float n=(1-u[i].HP/(float)Mathf.Max(1,u[i].MaxHP))*40+(u[i].HP<=25?100:0)+(e!=BattleElement.None&&u[i].Weakness==e?30:0);if(n>s){s=n;b=i;}}return b; }
        }
        sealed class DefaultRankCalculator : IBattleRankCalculator
        {
            public string CalculateRank(BattleRankInput i) { int s=100-i.TurnCount*2-i.DamageTaken/10+(i.SurvivingHeroes==i.TotalHeroes?20:0)+i.SkillUseCount*2; return s>=90?"S":s>=70?"A":s>=50?"B":"C"; }
        }
    }
}
