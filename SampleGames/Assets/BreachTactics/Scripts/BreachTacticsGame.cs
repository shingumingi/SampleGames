using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BreachTactics
{
    public sealed class BreachTacticsGame : MonoBehaviour
    {
        private const int BoardWidth = 8;
        private const int BoardHeight = 8;
        private const int FinalTurn = 5;

        private readonly List<BreachUnitState> units = new();
        private readonly List<BreachUnitState> initialUnits = new();
        private readonly Dictionary<int, BreachUnitView> views = new();
        private readonly List<GameObject> intentObjects = new();
        private readonly List<GameObject> healthObjects = new();
        private readonly List<GameObject> rangeObjects = new();

        private IEnemyIntentPlanner intentPlanner;
        private IPushResolver pushResolver;
        private IDamageCalculator damageCalculator;
        private IBoardEvaluator boardEvaluator;
        private BreachTacticsUI ui;
        private BreachIntentHUD intentHud;
        private List<EnemyIntent> intents = new();
        private BreachUnitState selectedUnit;
        private GameObject selectionRing;
        private int turn = 1;
        private string message = "아군을 선택하고 이동하거나 적을 공격하세요";
        private bool battleEnded;

        private void Awake()
        {
            EnsureCameraController();
            EnsureAlgorithms();
            LoadSceneUnits();

            ui = gameObject.AddComponent<BreachTacticsUI>();
            ui.Build();
            ui.EndTurnRequested += EndPlayerTurn;
            ui.RestartRequested += ResetBattle;

            intentHud = gameObject.AddComponent<BreachIntentHUD>();
            intentHud.Build();

            CreateSelectionRing();
            PlanEnemyIntents();
            RefreshAllViews();
        }

        private static void EnsureCameraController()
        {
            if (Camera.main != null
                && Camera.main.GetComponent<BreachCameraController>() == null)
            {
                Camera.main.gameObject.AddComponent<BreachCameraController>();
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetBattle();
            }

            if (!battleEnded)
            {
                HandleBoardClick();
            }

            AnimateSelection();
            UpdateUI();
        }

        private void EnsureAlgorithms()
        {
            intentPlanner = BreachAlgorithmFactory.Create<IEnemyIntentPlanner>(
                new FallbackEnemyIntentPlanner());
            pushResolver = BreachAlgorithmFactory.Create<IPushResolver>(
                new FallbackPushResolver());
            damageCalculator = BreachAlgorithmFactory.Create<IDamageCalculator>(
                new FallbackDamageCalculator());
            boardEvaluator = BreachAlgorithmFactory.Create<IBoardEvaluator>(
                new FallbackBoardEvaluator());
        }

        private void LoadSceneUnits()
        {
            BreachUnitView[] found = FindObjectsByType<BreachUnitView>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (BreachUnitView view in found)
            {
                BreachUnitState state = view.CreateState();
                units.Add(state);
                initialUnits.Add(CloneState(state));
                views[state.Id] = view;
            }
        }

        private static BreachUnitState CloneState(BreachUnitState source)
        {
            return new BreachUnitState
            {
                Id = source.Id,
                Name = source.Name,
                Team = source.Team,
                Position = source.Position,
                Health = source.Health,
                Attack = source.Attack,
                MoveRange = source.MoveRange,
                CanPush = source.CanPush,
                Acted = source.Acted
            };
        }

        private void HandleBoardClick()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame
                || Camera.main == null)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f)
                .OrderBy(hit => hit.distance)
                .ToArray();

            BreachUnitView clickedView = hits
                .Select(hit => hit.collider.GetComponentInParent<BreachUnitView>())
                .FirstOrDefault(view => view != null);

            if (clickedView != null)
            {
                HandleUnitClick(clickedView.Id);
                return;
            }

            BreachTileView tile = hits
                .Select(hit => hit.collider.GetComponent<BreachTileView>())
                .FirstOrDefault(value => value != null);

            if (tile != null)
            {
                HandleTileClick(tile.Position);
            }
        }

        private void HandleUnitClick(int unitId)
        {
            BreachUnitState clicked = FindUnit(unitId);
            if (clicked == null || !clicked.Alive)
            {
                return;
            }

            if (clicked.Team == BreachTeam.Ally)
            {
                if (clicked.Acted)
                {
                    message = $"{clicked.Name}은 이미 행동했습니다";
                    return;
                }

                SelectUnit(clicked);
                message = $"{clicked.Name} 선택: 빈 타일로 이동하거나 인접한 적을 클릭하세요";
                return;
            }

            if (clicked.Team == BreachTeam.Enemy && selectedUnit != null)
            {
                TryAttack(selectedUnit, clicked);
                return;
            }

            message = clicked.Team == BreachTeam.Enemy
                ? $"{clicked.Name}의 공격 예고를 확인하세요"
                : $"{clicked.Name} 건물을 지켜야 합니다";
        }

        private void HandleTileClick(GridPos destination)
        {
            if (selectedUnit == null)
            {
                message = "먼저 행동할 아군을 선택하세요";
                return;
            }

            if (selectedUnit.Position.Manhattan(destination) > selectedUnit.MoveRange)
            {
                message = $"이동 범위를 벗어났습니다. 최대 {selectedUnit.MoveRange}칸";
                return;
            }

            if (Occupied().ContainsKey(destination))
            {
                message = "다른 유닛이나 건물이 있는 타일로 이동할 수 없습니다";
                return;
            }

            selectedUnit.Position = destination;
            selectedUnit.Acted = true;
            message = $"{selectedUnit.Name} 이동 완료";
            ClearSelection();
            PlanEnemyIntents();
            RefreshAllViews();
        }

        private void TryAttack(BreachUnitState attacker, BreachUnitState target)
        {
            int distance = attacker.Position.Manhattan(target.Position);
            if (distance != 1)
            {
                message = "인접한 적만 공격할 수 있습니다";
                return;
            }

            GridPos direction = new GridPos(
                target.Position.X - attacker.Position.X,
                target.Position.Y - attacker.Position.Y);

            Dictionary<GridPos, int> occupied = Occupied();
            occupied.Remove(target.Position);

            PushResult push = attacker.CanPush
                ? pushResolver.Resolve(target.Position, direction, occupied, BoardWidth, BoardHeight)
                : new PushResult(target.Position, false, false, -1);

            int damage = damageCalculator.Calculate(
                attacker.Attack,
                push.CollisionUnitId >= 0,
                push.HitBoundary,
                false);

            target.Health -= damage;

            StartCoroutine(PlayAttackMotion(attacker, target));

            if (push.CollisionUnitId >= 0)
            {
                BreachUnitState collided = FindUnit(push.CollisionUnitId);
                if (collided != null)
                {
                    int collisionDamage = damageCalculator.Calculate(0, true, false,
                        collided.Team == BreachTeam.Building);
                    collided.Health -= collisionDamage;
                }
            }

            if (target.Alive && push.Moved)
            {
                target.Position = push.Destination;
            }

            attacker.Acted = true;
            message = push.HitBoundary
                ? $"{attacker.Name} 공격: {target.Name}이 경계에 충돌했습니다"
                : push.CollisionUnitId >= 0
                    ? $"{attacker.Name} 공격: 연쇄 충돌 발생"
                    : $"{attacker.Name} 공격: 피해 {damage}, 밀치기 {(push.Moved ? "성공" : "없음")}";

            ClearSelection();
            RemoveDestroyedViews();
            PlanEnemyIntents();
            RefreshAllViews();
            CheckBattleEnd();
        }

        private void EndPlayerTurn()
        {
            if (battleEnded)
            {
                return;
            }

            ExecuteEnemyIntents();
            RemoveDestroyedViews();

            if (CheckBattleEnd())
            {
                RefreshAllViews();
                return;
            }

            turn++;
            if (turn > FinalTurn)
            {
                battleEnded = true;
                message = "임무 성공: 5턴 동안 도시를 방어했습니다";
                ClearIntentObjects();
                RefreshAllViews();
                return;
            }

            foreach (BreachUnitState ally in units.Where(unit =>
                unit.Team == BreachTeam.Ally && unit.Alive))
            {
                ally.Acted = false;
            }

            selectedUnit = null;
            message = $"TURN {turn}: 적 공격 예고를 보고 행동하세요";
            PlanEnemyIntents();
            RefreshAllViews();
        }

        private void ExecuteEnemyIntents()
        {
            foreach (EnemyIntent intent in intents)
            {
                BreachUnitState enemy = FindUnit(intent.EnemyId);
                if (enemy == null || !enemy.Alive)
                {
                    continue;
                }

                BreachUnitState target = FindFirstTargetInLine(enemy, intent.Direction);
                if (target == null)
                {
                    continue;
                }

                int damage = damageCalculator.Calculate(
                    enemy.Attack,
                    false,
                    false,
                    target.Team == BreachTeam.Building);

                target.Health -= damage;
                StartCoroutine(PlayAttackMotion(enemy, target));
                message = $"{enemy.Name} 공격: {target.Name} 피해 {damage}";
            }
        }

        private BreachUnitState FindFirstTargetInLine(BreachUnitState enemy, BreachDirection direction)
        {
            GridPos step = GridPos.FromDirection(direction);
            GridPos cursor = enemy.Position;

            for (int i = 0; i < 7; i++)
            {
                cursor = cursor.Add(step);
                if (!InsideBoard(cursor))
                {
                    return null;
                }

                BreachUnitState target = units.FirstOrDefault(unit =>
                    unit.Alive
                    && unit.Team != BreachTeam.Enemy
                    && unit.Position.Equals(cursor));

                if (target != null)
                {
                    return target;
                }
            }

            return null;
        }

        private void PlanEnemyIntents()
        {
            intents = intentPlanner.Plan(units) ?? new List<EnemyIntent>();
            DrawIntentObjects();
        }

        private void DrawIntentObjects()
        {
            ClearIntentObjects();
            intentHud?.SetIntents(intents, units);
        }

        private void RefreshAllViews()
        {
            foreach (BreachUnitState state in units)
            {
                if (!views.TryGetValue(state.Id, out BreachUnitView view))
                {
                    continue;
                }

                view.gameObject.SetActive(state.Alive);
                if (!state.Alive)
                {
                    continue;
                }

                view.transform.position = GridToWorld(state.Position)
                    + Vector3.up * HeightFor(state.Team);
            }

            RefreshHealthObjects();

            if (selectedUnit != null && selectedUnit.Alive)
            {
                selectionRing.transform.position = GridToWorld(selectedUnit.Position)
                    + Vector3.up * 0.08f;
            }
        }

        private void RefreshHealthObjects()
        {
            foreach (GameObject value in healthObjects)
            {
                Destroy(value);
            }

            healthObjects.Clear();

            foreach (BreachUnitState state in units.Where(unit => unit.Alive))
            {
                for (int i = 0; i < state.Health; i++)
                {
                    GameObject pip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pip.name = $"Health_{state.Id}_{i}";
                    Vector3 basePosition = GridToWorld(state.Position)
                        + Vector3.up * (HeightFor(state.Team) + 0.95f);
                    pip.transform.position = basePosition
                        + Vector3.right * ((i - (state.Health - 1) * 0.5f) * 0.22f);
                    pip.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
                    pip.GetComponent<Renderer>().material.color = state.Team == BreachTeam.Enemy
                        ? new Color(1f, 0.25f, 0.18f)
                        : new Color(0.25f, 1f, 0.48f);
                    Destroy(pip.GetComponent<Collider>());
                    healthObjects.Add(pip);
                }
            }
        }

        private void RemoveDestroyedViews()
        {
            foreach (BreachUnitState state in units.Where(unit => !unit.Alive))
            {
                if (views.TryGetValue(state.Id, out BreachUnitView view))
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private bool CheckBattleEnd()
        {
            bool buildingsLost = !units.Any(unit =>
                unit.Team == BreachTeam.Building && unit.Alive);
            bool alliesLost = !units.Any(unit =>
                unit.Team == BreachTeam.Ally && unit.Alive);

            if (!buildingsLost && !alliesLost)
            {
                return false;
            }

            battleEnded = true;
            message = buildingsLost
                ? "임무 실패: 모든 건물이 파괴되었습니다"
                : "임무 실패: 모든 아군 기체가 파괴되었습니다";
            ClearSelection();
            ClearIntentObjects();
            return true;
        }

        private void UpdateUI()
        {
            int allies = units.Count(unit => unit.Team == BreachTeam.Ally && unit.Alive);
            int enemies = units.Count(unit => unit.Team == BreachTeam.Enemy && unit.Alive);
            int buildings = units.Count(unit => unit.Team == BreachTeam.Building && unit.Alive);
            int available = units.Count(unit =>
                unit.Team == BreachTeam.Ally && unit.Alive && !unit.Acted);
            int score = boardEvaluator.Evaluate(units, turn);

            ui.SetObjective("적의 공격 예고를 바꾸고 도시를 5턴 동안 방어하세요");
            ui.SetTurn(battleEnded
                ? "BATTLE END"
                : $"TURN {turn} / {FinalTurn}\n행동 가능 {available}기");
            ui.SetStatus(
                $"BREACH TACTICS\n{message}\n"
                + $"아군 {allies}  적 {enemies}  건물 {buildings}\n"
                + $"전장 평가 {score}\n"
                + "파랑: 이동 가능  빨강: 공격 가능  |  R: 재시작\n"
                + "카메라: WASD/방향키 이동  Q/E 회전  우클릭 드래그 회전  휠 줌");
        }

        private void SelectUnit(BreachUnitState unit)
        {
            selectedUnit = unit;
            selectionRing.SetActive(true);
            selectionRing.transform.position = GridToWorld(unit.Position) + Vector3.up * 0.08f;
            DrawActionRange(unit);
        }

        private void ClearSelection()
        {
            selectedUnit = null;
            ClearRangeObjects();
            if (selectionRing != null)
            {
                selectionRing.SetActive(false);
            }
        }

        private void CreateSelectionRing()
        {
            selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionRing.name = "Runtime_SelectedUnit_Ring";
            selectionRing.transform.localScale = new Vector3(0.9f, 0.025f, 0.9f);
            selectionRing.GetComponent<Renderer>().material.color = new Color(1f, 0.9f, 0.1f);
            Destroy(selectionRing.GetComponent<Collider>());
            selectionRing.SetActive(false);
        }

        private void AnimateSelection()
        {
            if (selectionRing == null || !selectionRing.activeSelf)
            {
                return;
            }

            float scale = 0.9f + Mathf.Sin(Time.unscaledTime * 5f) * 0.08f;
            selectionRing.transform.localScale = new Vector3(scale, 0.025f, scale);
        }

        private Dictionary<GridPos, int> Occupied()
        {
            return units
                .Where(unit => unit.Alive)
                .ToDictionary(unit => unit.Position, unit => unit.Id);
        }

        private BreachUnitState FindUnit(int id)
        {
            return units.FirstOrDefault(unit => unit.Id == id);
        }

        private static bool InsideBoard(GridPos position)
        {
            return position.X >= 0 && position.X < BoardWidth
                && position.Y >= 0 && position.Y < BoardHeight;
        }

        private static float HeightFor(BreachTeam team)
        {
            return team == BreachTeam.Building ? 0.8f : 0.72f;
        }

        private static Vector3 GridToWorld(GridPos position)
        {
            return new Vector3(position.X - 3.5f, 0f, position.Y - 3.5f);
        }

        private void ClearIntentObjects()
        {
            foreach (GameObject value in intentObjects)
            {
                Destroy(value);
            }

            intentObjects.Clear();
            intentHud?.Clear();
        }

        private void DrawActionRange(BreachUnitState unit)
        {
            ClearRangeObjects();
            Dictionary<GridPos, int> occupied = Occupied();

            var reachable = new HashSet<GridPos> { unit.Position };
            var frontier = new Queue<(GridPos Position, int Distance)>();
            frontier.Enqueue((unit.Position, 0));
            GridPos[] steps =
            {
                new(0, 1), new(1, 0), new(0, -1), new(-1, 0)
            };

            while (frontier.Count > 0)
            {
                (GridPos current, int distance) = frontier.Dequeue();
                if (distance >= unit.MoveRange) continue;

                foreach (GridPos step in steps)
                {
                    GridPos next = current.Add(step);
                    if (!InsideBoard(next) || occupied.ContainsKey(next)
                        || !reachable.Add(next)) continue;
                    frontier.Enqueue((next, distance + 1));
                }
            }

            for (int x = 0; x < BoardWidth; x++)
            for (int y = 0; y < BoardHeight; y++)
            {
                GridPos position = new(x, y);
                int distance = unit.Position.Manhattan(position);
                bool emptyMove = distance > 0 && reachable.Contains(position);
                BreachUnitState enemy = units.FirstOrDefault(value => value.Alive
                    && value.Team == BreachTeam.Enemy && value.Position.Equals(position));
                bool attackable = distance == 1 && enemy != null;
                if (!emptyMove && !attackable) continue;

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = attackable ? "AttackableTile" : "MoveRangeTile";
                marker.transform.position = GridToWorld(position) + Vector3.up * 0.12f;
                marker.transform.localScale = new Vector3(0.84f, 0.055f, 0.84f);
                marker.GetComponent<Renderer>().material.color = attackable
                    ? new Color(1f, 0.12f, 0.08f, 0.8f)
                    : new Color(0.05f, 0.65f, 1f, 0.62f);
                Destroy(marker.GetComponent<Collider>());
                rangeObjects.Add(marker);
            }
        }

        private void ClearRangeObjects()
        {
            foreach (GameObject value in rangeObjects) Destroy(value);
            rangeObjects.Clear();
        }

        private IEnumerator PlayAttackMotion(BreachUnitState attacker,
            BreachUnitState target)
        {
            if (!views.TryGetValue(attacker.Id, out BreachUnitView attackerView)
                || !views.TryGetValue(target.Id, out BreachUnitView targetView)) yield break;

            Transform attackerTransform = attackerView.transform;
            Transform targetTransform = targetView.transform;
            Vector3 origin = attackerTransform.position;
            Vector3 targetOrigin = targetTransform.position;
            Vector3 impact = Vector3.Lerp(origin, targetOrigin, 0.55f) + Vector3.up * 0.12f;

            for (float elapsed = 0f; elapsed < 0.14f; elapsed += Time.deltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.14f);
                attackerTransform.position = Vector3.Lerp(origin, impact, t);
                yield return null;
            }

            Vector3 knock = (targetOrigin - origin).normalized * 0.28f;
            for (float elapsed = 0f; elapsed < 0.1f; elapsed += Time.deltaTime)
            {
                float pulse = Mathf.Sin(elapsed / 0.1f * Mathf.PI);
                targetTransform.position = targetOrigin + knock * pulse;
                targetTransform.localScale = Vector3.one * (1f + pulse * 0.18f);
                yield return null;
            }

            for (float elapsed = 0f; elapsed < 0.16f; elapsed += Time.deltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.16f);
                attackerTransform.position = Vector3.Lerp(impact, origin, t);
                yield return null;
            }

            attackerTransform.position = origin;
            targetTransform.position = targetOrigin;
            targetTransform.localScale = Vector3.one;
            RefreshAllViews();
        }

        private void ResetBattle()
        {
            units.Clear();
            foreach (BreachUnitState initial in initialUnits)
            {
                units.Add(CloneState(initial));
            }

            turn = 1;
            battleEnded = false;
            message = "아군을 선택하고 이동하거나 적을 공격하세요";
            ClearSelection();
            PlanEnemyIntents();
            RefreshAllViews();
        }
    }
}
