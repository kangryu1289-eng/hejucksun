using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

namespace PirateGame.Editor
{
    [InitializeOnLoad]
    public static class PirateSceneBuilder
    {
        public const string Root = "Assets/PirateGame";
        public const string ScenePath = Root + "/Scenes/PirateBattle.unity";
        static TMP_FontAsset font;
        static Sprite square;
        static Material spriteMaterial;
        static readonly Color Navy = new Color(0.035f, 0.10f, 0.16f, 0.96f);
        static readonly Color Gold = new Color(1f, 0.78f, 0.37f);
        static PirateSceneBuilder() { EditorApplication.delayCall += BuildIfMissing; }
        static void BuildIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || File.Exists(ScenePath)) return;
            if (SessionState.GetBool("PirateGame.BuildAttempted", false)) return;
            SessionState.SetBool("PirateGame.BuildAttempted", true);
            Build();
        }
        [MenuItem("Pirate Game/Open Game Scene")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath)) Build();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Pirate Game/Create Missing Game Assets")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(ScenePath)) { Debug.Log("Pirate scene already exists; existing work is preserved."); return; }
            foreach (string folder in new[] { "Scenes", "Prefabs", "Data", "Art", "Materials", "Audio" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (!font) throw new InvalidOperationException("Import TMP Essential Resources before creating the game scene.");
            spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/SpriteUnlit.mat");
            if (!spriteMaterial)
            {
                spriteMaterial = new Material(Shader.Find("Sprites/Default"));
                AssetDatabase.CreateAsset(spriteMaterial, Root + "/Materials/SpriteUnlit.mat");
            }
            square = MakeTexture("White", 8, (x,y) => Color.white);
            Sprite water = MakeTexture("Ocean", 128, (x,y) => {
                float wave = Mathf.Sin((x + Mathf.Sin(y * 0.049f) * 15) * 0.09817477f);
                return Color.Lerp(new Color(0.18f,0.60f,0.74f), new Color(0.29f,0.71f,0.81f), wave * 0.5f + 0.5f);
            });
            Sprite sand = CopySprite("Tiles/tile_01.png", "ShoreCorner");
            Sprite playerSprite = CopySprite("Ships/ship (5).png", "PlayerShip");
            Sprite enemySprite = CopySprite("Ships/ship (13).png", "EnemyShip");
            Sprite ballSprite = CopySprite("Ship parts/cannonBall.png", "Cannonball");
            CopySprite("Effects/explosion1.png", "Explosion1");
            CopySprite("Effects/explosion2.png", "Explosion2");
            CopySprite("Effects/explosion3.png", "Explosion3");
            MakeAudio("Shot", 0.18f, 100f, 0.65f);
            MakeAudio("Hit", 0.12f, 230f, 0.45f);
            MakeAudio("Splash", 0.28f, 600f, 0.8f);
            MakeAudio("Sink", 0.65f, 65f, 0.6f);

            var original = SceneManager.GetActiveScene();
            bool additive = !string.IsNullOrEmpty(original.path);
            if (!additive && original.isDirty) throw new InvalidOperationException("Save the untitled scene before creating game assets.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, additive ? NewSceneMode.Additive : NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0,0,-10); camera.orthographic = true; camera.orthographicSize = 7.8f;
            camera.backgroundColor = new Color(0.13f,0.49f,0.63f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            var world = new GameObject("World");
            var ocean = SpriteObject("Ocean", world.transform, water, new Vector3(0,0,2), Vector3.one);
            ocean.drawMode = SpriteDrawMode.Tiled; ocean.size = new Vector2(32,20); ocean.sortingOrder = -20;
            for (int i=0; i<18; i++)
            {
                var foam = SpriteObject("StaticWaterLine_" + i, world.transform, square,
                    new Vector3(-13 + (i*7 % 26), -6 + (i*3 % 13), 1), new Vector3(1.1f,0.025f,1));
                foam.color = new Color(0.8f,1,1,0.11f); foam.sortingOrder = -19;
                foam.transform.rotation = Quaternion.Euler(0,0,-18);
            }
            var arena = new GameObject("Arena"); arena.transform.SetParent(world.transform);
            Island(arena.transform, sand, new Vector2(-10.6f,4.8f));
            Island(arena.transform, sand, new Vector2(10.6f,-4.8f));
            // Mark visible arena edges without introducing damage or physical walls.
            Edge(arena.transform, new Vector2(0,6.5f), new Vector2(24,0.03f));
            Edge(arena.transform, new Vector2(0,-6.5f), new Vector2(24,0.03f));
            Edge(arena.transform, new Vector2(-12,0), new Vector2(0.03f,13));
            Edge(arena.transform, new Vector2(12,0), new Vector2(0.03f,13));
            arena.SetActive(false);

            var systems = new GameObject("Game Systems");
            var feedback = systems.AddComponent<CombatFeedbackEvents>();
            var battle = systems.AddComponent<BattleController>();
            var flow = systems.AddComponent<GameFlowController>();
            var actors = new GameObject("Actors"); actors.transform.SetParent(world.transform);
            var projectiles = new GameObject("Projectiles"); projectiles.transform.SetParent(world.transform);
            var juice = new GameObject("Juice Lab - add tutorial components here");
            var effects = new GameObject("Effects"); effects.transform.SetParent(world.transform);
            var ballObject = new GameObject("Cannonball");
            var ballRenderer = ballObject.AddComponent<SpriteRenderer>(); ballRenderer.sprite = ballSprite; ballRenderer.sharedMaterial = spriteMaterial; ballRenderer.sortingOrder = 10;
            ballObject.transform.localScale = Vector3.one * 1.4f;
            ballObject.AddComponent<Cannonball>();
            var ballPrefab = PrefabUtility.SaveAsPrefabAsset(ballObject, Root + "/Prefabs/Cannonball.prefab").GetComponent<Cannonball>();
            UnityEngine.Object.DestroyImmediate(ballObject);
            var player = MakeShip("PlayerShip", playerSprite, true, ballPrefab);
            var enemy = MakeShip("EnemyShip", enemySprite, false, ballPrefab);
            battle.playerPrefab = player; battle.enemyPrefab = enemy; battle.actorsRoot = actors.transform;
            battle.projectileRoot = projectiles.transform; battle.arenaRoot = arena; battle.worldCamera = camera; battle.feedback = feedback;
            flow.battle = battle;
            flow.rounds = new RoundDefinition[3];
            string[] titles = { "Coastal Patrol", "Smuggler Waters", "Pirate Armada" };
            for (int i=0;i<3;i++)
            {
                string path = Root + "/Data/Round" + (i+1) + ".asset";
                var round = AssetDatabase.LoadAssetAtPath<RoundDefinition>(path);
                if (!round)
                {
                    round = ScriptableObject.CreateInstance<RoundDefinition>();
                    round.title = titles[i]; round.enemyCount = i+2; round.enemyHull = 40+i*20;
                    round.enemyDamage = 8+i*2; round.victoryGold = new[]{80,120,180}[i];
                    AssetDatabase.CreateAsset(round, path);
                }
                flow.rounds[i] = round;
            }
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var canvasObject = new GameObject("Game Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = 0.5f;
            var ui = canvasObject.AddComponent<GameUI>(); flow.ui = ui;
            MakeDock(canvasObject.transform, ui);
            MakeRound(canvasObject.transform, ui);
            MakeHUD(canvasObject.transform, ui);
            MakeResult(canvasObject.transform, ui);
            ui.roundSelect.gameObject.SetActive(false); ui.battleHUD.gameObject.SetActive(false); ui.result.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var old = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            old.Insert(0, new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = old.ToArray();
            AssetDatabase.SaveAssets();
            if (additive)
            {
                EditorSceneManager.CloseScene(scene, true);
                if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            }
            Debug.Log("PIRATE_GAME_BUILD_OK: " + ScenePath);
        }
        static Sprite MakeTexture(string name, int size, Func<int,int,Color> color)
        {
            string path = Root + "/Art/" + name + ".png";
            if (!File.Exists(path))
            {
                var tex = new Texture2D(size,size,TextureFormat.RGBA32,false);
                var colors = new Color[size*size];
                for(int y=0;y<size;y++) for(int x=0;x<size;x++) colors[y*size+x] = color(x,y);
                tex.SetPixels(colors); tex.Apply(); File.WriteAllBytes(path,tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
            }
            return ImportSprite(path, size == 128 ? 64 : size);
        }
        static Sprite CopySprite(string relative, string name)
        {
            string path = Root + "/Art/" + name + ".png";
            if (!File.Exists(path)) { File.Copy("Assets/kenney_pirate-pack/PNG/Default size/" + relative, path); AssetDatabase.ImportAsset(path); }
            return ImportSprite(path,100);
        }
        static Sprite ImportSprite(string path, float ppu)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Repeat;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static SpriteRenderer SpriteObject(string name, Transform parent, Sprite sprite, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sharedMaterial = spriteMaterial; return renderer;
        }
        static void Edge(Transform parent, Vector2 point, Vector2 size)
        { var sr = SpriteObject("ArenaEdge", parent, square, point, new Vector3(size.x,size.y,1)); sr.color = new Color(0.65f,0.95f,1,0.25f); sr.sortingOrder = -5; }
        static void Island(Transform parent, Sprite corner, Vector2 position)
        {
            var go = new GameObject("Island"); go.transform.SetParent(parent); go.transform.position = position;
            // The four rotated shore tiles form a small sand island.
            Vector2[] offsets = {new Vector2(-0.64f,0.64f),new Vector2(0.64f,0.64f),new Vector2(0.64f,-0.64f),new Vector2(-0.64f,-0.64f)};
            for(int i=0;i<4;i++)
            {
                var sr=SpriteObject("Shore_" + i,go.transform,corner,(Vector3)(position+offsets[i]),Vector3.one*2);
                sr.transform.rotation=Quaternion.Euler(0,0,-90*i); sr.sortingOrder=-2;
            }
            var collider=go.AddComponent<CircleCollider2D>(); collider.radius=1.1f;
        }
        static ShipHealth MakeShip(string name, Sprite sprite, bool player, Cannonball ball)
        {
            var go = new GameObject(name);
            var body=go.AddComponent<Rigidbody2D>(); body.gravityScale=0; body.mass=2; body.linearDamping=2; body.angularDamping=2;
            body.collisionDetectionMode=CollisionDetectionMode2D.Continuous; body.interpolation=RigidbodyInterpolation2D.Interpolate;
            var shape=go.AddComponent<CapsuleCollider2D>(); shape.size=new Vector2(0.8f,1.55f); shape.direction=CapsuleDirection2D.Vertical;
            var health=go.AddComponent<ShipHealth>(); health.isPlayer=player;
            var motor=go.AddComponent<ShipMotor>(); if(!player) {motor.forwardSpeed=1.7f;motor.turnSpeed=65;}
            var weapon=go.AddComponent<CannonWeapon>(); weapon.projectilePrefab=ball;
            var visual=SpriteObject("Visual",go.transform,sprite,Vector3.zero,Vector3.one*1.5f);
            visual.transform.localRotation=Quaternion.Euler(0,0,180); visual.sortingOrder=2;
            if(player)
            {
                var aim=go.AddComponent<CannonAim>();
                var guideObject=new GameObject("BasicAimGuide");guideObject.transform.SetParent(go.transform);
                var line=guideObject.AddComponent<LineRenderer>();line.sharedMaterial=spriteMaterial;line.positionCount=2;line.startWidth=line.endWidth=0.025f;
                line.useWorldSpace=true;line.sortingOrder=1; aim.guide=line;
                go.AddComponent<PlayerShipInput>();
            }
            else go.AddComponent<EnemyShipAI>();
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab");
            UnityEngine.Object.DestroyImmediate(go); return prefab.GetComponent<ShipHealth>();
        }
        static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.sizeDelta=size;rt.anchoredPosition=position;return rt;
        }
        static GameObject Panel(string name,Transform parent,Vector2 size)
        { var rt=Rect(name,parent,size,Vector2.zero); var image=rt.gameObject.AddComponent<Image>();image.color=Navy;return rt.gameObject; }
        static TMP_Text Label(string name,Transform parent,string value,int fontSize,Vector2 size,Vector2 position,Color? color=null)
        {
            var rt=Rect(name,parent,size,position);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=fontSize;
            text.text=value;text.color=color??Color.white;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            text.textWrappingMode=TextWrappingModes.Normal;text.overflowMode=TextOverflowModes.Truncate;return text;
        }
        static Button Button(string name,Transform parent,string value,Vector2 size,Vector2 position,out TMP_Text label)
        {
            var rt=Rect(name,parent,size,position);var image=rt.gameObject.AddComponent<Image>();image.color=new Color(0.14f,0.32f,0.39f);
            var button=rt.gameObject.AddComponent<Button>();var colors=button.colors;colors.highlightedColor=new Color(1,0.86f,0.52f);colors.disabledColor=new Color(0.45f,0.48f,0.5f);button.colors=colors;
            label=Label("Label",rt,value,20,size-Vector2.one*8,Vector2.zero);
            return button;
        }
        static Slider Bar(string name,Transform parent,Vector2 pos,Color color)
        {
            var rt=Rect(name,parent,new Vector2(230,12),pos);
            var bg=rt.gameObject.AddComponent<Image>();bg.color=new Color(0,0,0,0.5f);bg.raycastTarget=false;
            var fill=Rect("Fill",rt,Vector2.zero,Vector2.zero);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;
            var image=fill.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;
            var slider=rt.gameObject.AddComponent<Slider>();slider.fillRect=fill;slider.interactable=false;slider.navigation=new Navigation{mode=Navigation.Mode.None};slider.value=1;return slider;
        }
        static void MakeDock(Transform root,GameUI ui)
        {
            var panel=Panel("Dock Panel",root,new Vector2(740,570));var dock=panel.AddComponent<DockPanel>();ui.dock=dock;
            Label("Title",panel.transform,"PIRATE HARBOR",36,new Vector2(650,50),new Vector2(0,225),Gold);
            Label("Subtitle",panel.transform,"Repair. Upgrade. Set sail.",18,new Vector2(650,30),new Vector2(0,180));
            dock.stats=Label("Ship Stats",panel.transform,"",22,new Vector2(680,110),new Vector2(0,95));
            dock.repairButton=Button("Repair",panel.transform,"",new Vector2(560,48),new Vector2(0,0),out dock.repairLabel);
            dock.damageButton=Button("Upgrade Damage",panel.transform,"",new Vector2(560,48),new Vector2(0,-60),out dock.damageLabel);
            dock.reloadButton=Button("Upgrade Reload",panel.transform,"",new Vector2(560,48),new Vector2(0,-120),out dock.reloadLabel);
            dock.sailButton=Button("Set Sail",panel.transform,"CHOOSE ROUND",new Vector2(560,52),new Vector2(0,-202),out _);
            Label("Footer",panel.transform,"BASE GAME / JUICE LAB",14,new Vector2(650,22),new Vector2(0,-255),Gold);
        }
        static void MakeRound(Transform root,GameUI ui)
        {
            var panel=Panel("Round Select Panel",root,new Vector2(740,520));var select=panel.AddComponent<RoundSelectPanel>();ui.roundSelect=select;
            Label("Title",panel.transform,"CHOOSE YOUR BATTLE",32,new Vector2(680,55),new Vector2(0,190),Gold);
            select.roundButtons=new Button[3];select.roundLabels=new TMP_Text[3];
            for(int i=0;i<3;i++)select.roundButtons[i]=Button("Round "+(i+1),panel.transform,"",new Vector2(620,80),new Vector2(0,85-i*100),out select.roundLabels[i]);
            select.backButton=Button("Back",panel.transform,"BACK TO HARBOR",new Vector2(620,45),new Vector2(0,-210),out _);
        }
        static void MakeHUD(Transform root,GameUI ui)
        {
            var rt=Rect("Battle HUD",root,Vector2.zero,Vector2.zero);rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
            var hud=rt.gameObject.AddComponent<BattleHUD>();ui.battleHUD=hud;
            var top=Panel("Top Readout",rt,new Vector2(600,78));top.GetComponent<RectTransform>().anchorMin=top.GetComponent<RectTransform>().anchorMax=new Vector2(0.5f,1);
            top.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-45);
            hud.status=Label("Status",top.transform,"",22,new Vector2(570,32),new Vector2(0,15));
            hud.hullBar=Bar("Hull Bar",top.transform,new Vector2(-130,-17),new Color(0.3f,0.9f,0.65f));
            hud.reloadBar=Bar("Reload Bar",top.transform,new Vector2(130,-17),Gold);
            var bottom=Panel("Aim Readout",rt,new Vector2(760,70));bottom.GetComponent<RectTransform>().anchorMin=bottom.GetComponent<RectTransform>().anchorMax=new Vector2(0.5f,0);
            bottom.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,43);
            hud.aimText=Label("Aim Text",bottom.transform,"",20,new Vector2(735,65),Vector2.zero);
        }
        static void MakeResult(Transform root,GameUI ui)
        {
            var panel=Panel("Result Panel",root,new Vector2(680,500));var result=panel.AddComponent<ResultPanel>();ui.result=result;
            result.heading=Label("Heading",panel.transform,"",40,new Vector2(630,60),new Vector2(0,175),Gold);
            result.summary=Label("Summary",panel.transform,"",24,new Vector2(630,230),new Vector2(0,0));
            result.dockButton=Button("Return to Dock",panel.transform,"RETURN TO HARBOR",new Vector2(540,55),new Vector2(0,-185),out _);
        }
        static void MakeAudio(string name,float duration,float frequency,float noiseMix)
        {
            string path=Root+"/Audio/"+name+".wav";if(File.Exists(path))return;
            const int rate=22050;int count=(int)(rate*duration);var random=new System.Random(name.Length*17+(int)frequency);
            using(var stream=File.Create(path))using(var w=new BinaryWriter(stream))
            {
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);
                for(int i=0;i<count;i++)
                {
                    float t=(float)i/rate;float envelope=Mathf.Exp(-6*t/duration);
                    float tone=Mathf.Sin(2*Mathf.PI*frequency*t);
                    float noise=(float)random.NextDouble()*2-1;
                    w.Write((short)(Mathf.Lerp(tone,noise,noiseMix)*envelope*0.6f*short.MaxValue));
                }
            }
            AssetDatabase.ImportAsset(path);
        }
    }
}
