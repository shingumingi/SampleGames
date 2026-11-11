using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TurnGame.Editor
{
    public static class TurnGameBuilder
    {
        const string Root = "Assets/TurnGame";
        const string DataRoot = Root + "/Data";
        const string ScenePath = Root + "/Scenes/TurnGame_Battle.unity";

        [InitializeOnLoadMethod]
        static void AutoCreateWhenMissing()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                    RebuildScene();
            };
        }

        [MenuItem("AlgoLab/Turn Game/Create Game Data")]
        public static void CreateData()
        {
            EnsureFolder("Assets", "TurnGame"); EnsureFolder(Root, "Data"); EnsureFolder(Root, "Scenes");
            var slash = Skill("slash", "파워 슬래시", "강한 단일 공격", 5, 155, 2, new Color(0.3f, 0.8f, 1f));
            var fire = Skill("fire", "파이어", "화염 마법", 7, 175, -1, new Color(1f, 0.25f, 0.1f));
            var fang = Skill("fang", "독니", "빠른 몬스터 공격", 0, 125, 3, new Color(0.5f, 1f, 0.3f));

            Character("hero_knight", "기사 루카", BattleTeam.Heroes, 150, 18, 25, 15, 12, 22, 4, 3, 1, 0, new Color(0.2f, 0.55f, 1f), slash);
            Character("hero_mage", "마도사 미아", BattleTeam.Heroes, 92, 35, 30, 7, 15, 13, 5, 1, 2, 0, new Color(0.65f, 0.3f, 1f), fire);
            Character("hero_rogue", "도적 카인", BattleTeam.Heroes, 110, 22, 22, 10, 20, 15, 4, 2, 2, 0, new Color(0.2f, 0.9f, 0.6f), slash);
            Character("hero_cleric", "성직자 세라", BattleTeam.Heroes, 120, 32, 18, 13, 11, 19, 3, 3, 1, 0, new Color(1f, 0.85f, 0.3f), fire);
            Character("enemy_slime", "블루 슬라임", BattleTeam.Enemies, 78, 0, 17, 6, 9, 12, 3, 2, 1, 25, new Color(0.15f, 0.7f, 1f), fang);
            Character("enemy_goblin", "고블린", BattleTeam.Enemies, 105, 0, 22, 9, 14, 16, 4, 2, 1, 35, new Color(0.3f, 0.75f, 0.2f), fang);
            Character("enemy_orc", "오크 전사", BattleTeam.Enemies, 175, 0, 29, 16, 7, 24, 5, 3, 1, 55, new Color(0.65f, 0.28f, 0.16f), fang);
            Character("enemy_wolf", "그림자 늑대", BattleTeam.Enemies, 120, 0, 26, 8, 19, 17, 4, 2, 2, 45, new Color(0.3f, 0.3f, 0.36f), fang);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("Turn Game data created.");
        }

        [MenuItem("AlgoLab/Turn Game/Rebuild Turn Game")]
        public static void RebuildScene()
        {
            CreateData();
            Scene previousScene = SceneManager.GetActiveScene();
            bool preservePrevious = previousScene.IsValid() && previousScene.isLoaded && !string.IsNullOrEmpty(previousScene.path);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                preservePrevious ? NewSceneMode.Additive : NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            scene.name = "TurnGame_Battle";
            RenderSettings.ambientLight = new Color(0.52f, 0.58f, 0.7f);

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0f, 11f, -14f); camera.transform.rotation = Quaternion.Euler(28f, 0f, 0f); camera.fieldOfView = 48f; camera.backgroundColor = new Color(0.04f, 0.07f, 0.12f); camera.clearFlags = CameraClearFlags.SolidColor;
            var light = new GameObject("Directional Light").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.3f; light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name = "Battle Floor"; floor.transform.position = new Vector3(0f, -0.35f, 0f); floor.transform.localScale = new Vector3(18f, 0.5f, 10f); floor.GetComponent<Renderer>().material.color = new Color(0.09f, 0.16f, 0.22f);
            CreatePortal("Hero Inbound", new Vector3(-8.2f, 1.5f, 0f), new Color(0.1f, 0.65f, 1f));
            CreatePortal("Enemy Inbound", new Vector3(8.2f, 1.5f, 0f), new Color(1f, 0.25f, 0.25f));

            var game = new GameObject("Turn Game Manager").AddComponent<TurnGameController>();
            game.heroData = new[] { LoadCharacter("hero_knight"), LoadCharacter("hero_mage"), LoadCharacter("hero_rogue") };
            game.enemyData = new[] { LoadCharacter("enemy_slime"), LoadCharacter("enemy_goblin"), LoadCharacter("enemy_orc") };
            game.reserveHero = LoadCharacter("hero_cleric"); game.reserveEnemy = LoadCharacter("enemy_wolf");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            if (preservePrevious)
                EditorSceneManager.CloseScene(scene, true);
            if (preservePrevious && previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
            Debug.Log("Turn Game scene created.");
        }

        [MenuItem("AlgoLab/Turn Game/Reset Game Progress")]
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey("TG_STAGE");
            foreach (string id in new[] { "hero_knight", "hero_mage", "hero_rogue", "hero_cleric" })
            {
                PlayerPrefs.DeleteKey("TG_LV_" + id);
                PlayerPrefs.DeleteKey("TG_XP_" + id);
            }
            PlayerPrefs.Save();
            Debug.Log("Turn Game progress reset.");
        }

        static void CreatePortal(string name, Vector3 pos, Color color)
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder); ring.name = name; ring.transform.position = pos; ring.transform.rotation = Quaternion.Euler(90f, 0f, 0f); ring.transform.localScale = new Vector3(1.8f, 0.15f, 1.8f); ring.GetComponent<Renderer>().material.color = color;
        }
        static SkillData Skill(string id, string display, string desc, int mp, int power, int speed, Color color)
        {
            string path = $"{DataRoot}/Skill_{id}.asset"; var data = AssetDatabase.LoadAssetAtPath<SkillData>(path);
            if (data == null && File.Exists(path)) AssetDatabase.DeleteAsset(path);
            data = AssetDatabase.LoadAssetAtPath<SkillData>(path) ?? ScriptableObject.CreateInstance<SkillData>();
            data.skillId=id; data.displayName=display; data.description=desc; data.mpCost=mp; data.power=power; data.speedBonus=speed; data.effectColor=color;
            if (!AssetDatabase.Contains(data)) AssetDatabase.CreateAsset(data,path); EditorUtility.SetDirty(data); return data;
        }
        static void Character(string id,string display,BattleTeam team,int hp,int mp,int atk,int def,int speed,int hpG,int atkG,int defG,int speedG,int xp,Color color,SkillData skill)
        {
            string path=$"{DataRoot}/Character_{id}.asset"; var d=AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (d == null && File.Exists(path)) AssetDatabase.DeleteAsset(path);
            d=AssetDatabase.LoadAssetAtPath<CharacterData>(path) ?? ScriptableObject.CreateInstance<CharacterData>();
            d.characterId=id;d.displayName=display;d.team=team;d.baseHP=hp;d.baseMP=mp;d.attack=atk;d.defense=def;d.speed=speed;d.hpGrowth=hpG;d.attackGrowth=atkG;d.defenseGrowth=defG;d.speedGrowth=speedG;d.experienceReward=xp;d.bodyColor=color;d.skills=new[]{skill};
            if(!AssetDatabase.Contains(d))AssetDatabase.CreateAsset(d,path);EditorUtility.SetDirty(d);
        }
        static CharacterData LoadCharacter(string id)=>AssetDatabase.LoadAssetAtPath<CharacterData>($"{DataRoot}/Character_{id}.asset");
        static void EnsureFolder(string parent,string name){if(!AssetDatabase.IsValidFolder(parent+"/"+name))AssetDatabase.CreateFolder(parent,name);}
    }
}
