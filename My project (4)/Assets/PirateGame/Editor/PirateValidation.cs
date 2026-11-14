using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PirateGame.Editor
{
    // Development checks only. No test behaviour is attached to the delivered scene.
    [InitializeOnLoad]
    public static class PirateValidation
    {
        const string Key = "PirateGame.Validation";
        static int stage, assertions;
        static float stageTime;
        static double deadline;
        static GameFlowController flow;
        static ShipHealth target;
        static Vector3 startPosition;
        static float startRotation;
        static int shots, hits, sinks, misses;
        static string report;
        static PirateValidation()
        {
            if (SessionState.GetBool(Key, false)) EditorApplication.delayCall += Attach;
        }
        [MenuItem("Pirate Game/Run Gameplay Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            PirateSceneBuilder.Build();
            EditorSceneManager.OpenScene(PirateSceneBuilder.ScenePath);
            assertions = 0;
            report = "PirateGame checks\n";
            Economy();
            Check(UnityEngine.Object.FindFirstObjectByType<GameFlowController>() != null, "scene has flow");
            Check(UnityEngine.Object.FindObjectsByType<PirateGame.Juice.HitStop>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0, "Juice starts unimplemented/unattached");
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".assertions", assertions);
            SessionState.SetString(Key + ".report", report);
            EditorApplication.isPlaying = true;
            Attach();
        }
        static void Economy()
        {
            var p = new PlayerProgress(); var dock = new DockService(p);
            Check(p.Gold == 100 && p.Hull == 100, "fresh economy");
            Check(!dock.Repair(), "full hull cannot spend repair gold");
            p.SetHull(70.2f);
            Check(dock.RepairCost == 30 && dock.Repair() && p.Gold == 70 && p.Hull == 100, "repair rounds up and restores hull");
            Check(dock.UpgradeDamage() && p.DamageLevel == 1 && p.Gold == 20 && p.Damage == 25, "damage purchase");
            Check(!dock.UpgradeReload() && p.Gold == 20, "insufficient gold does not spend");
            p.AddGold(10000);
            for (int i=0;i<10;i++) { dock.UpgradeDamage(); dock.UpgradeReload(); }
            Check(p.DamageLevel == 5 && p.ReloadLevel == 5, "upgrade caps");
            Check(p.ReloadSeconds > 0.47f && p.ReloadSeconds < 0.48f, "reload multiplicative scaling");
            Check(!p.Spend(-1), "negative spending rejected");
        }
        static void Attach()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + 90;
            stage = 0;
            assertions = SessionState.GetInt(Key + ".assertions", assertions);
            report = SessionState.GetString(Key + ".report", report);
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Key, false)) { EditorApplication.update -= Tick; return; }
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Gameplay check timeout at stage " + stage);
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
                if (stage == 0)
                {
                    flow = UnityEngine.Object.FindFirstObjectByType<GameFlowController>();
                    if (!flow || flow.Dock == null || flow.ui.dock.stats.text.Length == 0) return;
                    Check(flow.Phase == GamePhase.Dock && flow.ui.dock.gameObject.activeSelf, "boot enters dock");
                    Capture("01_Dock");
                    flow.ui.dock.damageButton.onClick.Invoke();
                    Check(flow.Progress.Damage == 25 && flow.Progress.Gold == 50, "dock button purchase");
                    flow.ui.dock.sailButton.onClick.Invoke();
                    Check(flow.Phase == GamePhase.RoundSelect, "round screen");
                    Capture("02_RoundSelect");
                    flow.ui.roundSelect.roundButtons[0].onClick.Invoke();
                    Check(flow.Phase == GamePhase.Battle && flow.battle.EnemiesRemaining == 2, "round 1 spawns two enemies");
                    Check(flow.battle.Player.GetComponent<CannonWeapon>().damage == 25, "upgrade applied to player");
                    DisableControls();
                    flow.battle.feedback.ShotFired += _ => shots++;
                    flow.battle.feedback.Hit += _ => hits++;
                    flow.battle.feedback.Sunk += _ => sinks++;
                    flow.battle.feedback.WaterMiss += _ => misses++;
                    startPosition = flow.battle.Player.transform.position;
                    startRotation = flow.battle.Player.GetComponent<Rigidbody2D>().rotation;
                    flow.battle.Player.GetComponent<ShipMotor>().SetInput(1, 1);
                    stageTime = Time.time; stage = 1;
                }
                else if(stage == 1 && Time.time > stageTime + 0.35f)
                {
                    var player = flow.battle.Player;
                    Check(Vector3.Distance(startPosition, player.transform.position) > 0.2f, "physics ship moves");
                    Check(Mathf.Abs(Mathf.DeltaAngle(startRotation, player.GetComponent<Rigidbody2D>().rotation)) > 10, "physics ship turns");
                    player.GetComponent<ShipMotor>().SetInput(0,0);
                    player.GetComponent<Rigidbody2D>().position = new Vector2(-5,0);
                    var enemies = flow.battle.actorsRoot.GetComponentsInChildren<EnemyShipAI>();
                    target = enemies[0].GetComponent<ShipHealth>(); target.GetComponent<Rigidbody2D>().position = new Vector2(-1,0);
                    enemies[1].GetComponent<Rigidbody2D>().position = new Vector2(7,4);
                    Physics2D.SyncTransforms();
                    stageTime=Time.time;stage=10;
                }
                else if(stage==10 && Time.time>stageTime+0.08f)
                {
                    var player=flow.battle.Player;
                    Capture("03_Battle");
                    var weapon=player.GetComponent<CannonWeapon>();
                    Check(!weapon.TryFire((Vector2)player.transform.position + Vector2.right*11), "out of range shot rejected");
                    Check(!weapon.TryFire((Vector2)player.transform.position + Vector2.right*0.1f), "too-close shot rejected");
                    Check(weapon.TryFire(target.transform.position), "valid shot fires");
                    Check(!weapon.TryFire(target.transform.position) && shots==1, "reload blocks duplicate shots");
                    stageTime=Time.time;stage=2;
                }
                else if(stage==2 && Time.time>stageTime+0.9f)
                {
                    Check(target && Mathf.Abs(target.Hull-15)<0.01f && hits==1, "real projectile hits once for upgraded damage");
                    Check(flow.battle.Player.Hull==100, "own projectile does not hit own hull");
                    Check(flow.battle.Player.GetComponent<CannonWeapon>().TryFire(target.transform.position), "reload recovers");
                    stageTime=Time.time;stage=3;
                }
                else if(stage==3 && Time.time>stageTime+0.5f)
                {
                    Check(target && !target.IsAlive && sinks==1 && flow.battle.EnemiesRemaining==1, "real second shot sinks enemy once");
                    Check(flow.Phase==GamePhase.Battle, "one surviving enemy keeps round active");
                    foreach(var h in flow.battle.actorsRoot.GetComponentsInChildren<ShipHealth>())
                        if(!h.isPlayer && h.IsAlive)h.TakeDamage(10000,h.transform.position);
                    stage=4;
                }
                else if(stage==4 && flow.Phase==GamePhase.Result)
                {
                    Check(flow.LastResult.Victory && flow.LastResult.Sunk==2 && flow.Progress.Gold==130, "victory rewards once");
                    Check(flow.ui.result.heading.text=="VICTORY", "victory result view");
                    Capture("04_Victory");
                    stageTime=Time.time;stage=5;
                }
                else if(stage==5 && Time.time>stageTime+0.05f)
                {
                    Check(flow.Progress.Gold==130, "result does not repeat rewards");
                    flow.ui.result.dockButton.onClick.Invoke();
                    Check(flow.Phase==GamePhase.Dock && flow.battle.Player==null, "return clears battle");
                    flow.SelectRound();flow.StartRound(1);DisableControls();
                    Check(flow.battle.EnemiesRemaining==3, "round 2 spawns three enemies");
                    flow.battle.Player.TakeDamage(30,flow.battle.Player.transform.position);
                    foreach(var h in flow.battle.actorsRoot.GetComponentsInChildren<ShipHealth>())
                        if(!h.isPlayer && h.IsAlive)h.TakeDamage(10000,h.transform.position);
                    stage=6;
                }
                else if(stage==6 && flow.Phase==GamePhase.Result)
                {
                    Check(flow.Progress.Hull==70 && flow.Progress.Gold==250, "surviving hull and round 2 reward persist");
                    flow.ReturnToDock();
                    Check(flow.Dock.RepairCost==30, "persistent hull repair quote");
                    flow.Repair();
                    Check(flow.Progress.Hull==100 && flow.Progress.Gold==220, "repair after battle");
                    flow.SelectRound();flow.StartRound(2);DisableControls();
                    Check(flow.battle.EnemiesRemaining==4, "round 3 spawns four enemies");
                    flow.battle.Player.TakeDamage(10000,flow.battle.Player.transform.position);
                    stage=7;
                }
                else if(stage==7 && flow.Phase==GamePhase.Result)
                {
                    Check(!flow.LastResult.Victory && flow.Progress.Gold==240 && flow.Progress.Hull==50, "defeat consolation and free hull");
                    Check(flow.ui.result.heading.text=="SHIP LOST", "defeat result view");
                    Capture("05_Defeat");
                    flow.ReturnToDock();flow.SelectRound();flow.StartRound(0);DisableControls();
                    flow.battle.Player.GetComponent<Rigidbody2D>().position=new Vector2(-7,0);
                    var enemies=flow.battle.actorsRoot.GetComponentsInChildren<EnemyShipAI>();
                    for(int i=0;i<enemies.Length;i++)enemies[i].GetComponent<Rigidbody2D>().position=new Vector2(6+i,4);
                    Physics2D.SyncTransforms();
                    Check(flow.battle.Player.GetComponent<CannonWeapon>().TryFire(new Vector2(-7,-5)), "miss test fires into empty water");
                    stageTime=Time.time;stage=8;
                }
                else if(stage==8 && Time.time>stageTime+0.95f)
                {
                    Check(misses>0, "water miss event");
                    Check(Time.timeScale==1, "normal time after repeated loops");
                    flow.ReturnToDock();
                    stageTime=Time.time;stage=9;
                }
                else if(stage==9 && Time.time>stageTime+0.05f)
                {
                    Check(flow.battle.actorsRoot.childCount==0 && flow.battle.projectileRoot.childCount==0, "no stale actors/projectiles after clear");
                    flow.SelectRound();flow.StartRound(0);
                    flow.battle.Player.GetComponent<PlayerShipInput>().enabled=false;
                    flow.battle.Player.GetComponent<CannonAim>().enabled=false;
                    target=flow.battle.actorsRoot.GetComponentsInChildren<EnemyShipAI>()[0].GetComponent<ShipHealth>();
                    startPosition=target.transform.position; startRotation=shots;
                    stageTime=Time.time;stage=11;
                }
                else if(stage==11 && Time.time>stageTime+4.5f)
                {
                    Check(target && Vector3.Distance(startPosition,target.transform.position)>1, "autonomous enemy approaches player");
                    Check(shots>startRotation && flow.battle.Player.Hull<50, "autonomous enemy fires and damages player");
                    flow.battle.Player.GetComponent<Rigidbody2D>().position=new Vector2(50,50);
                    stageTime=Time.time;stage=12;
                }
                else if(stage==12 && Time.time>stageTime+0.1f)
                {
                    Vector2 position=flow.battle.Player.GetComponent<Rigidbody2D>().position;
                    Check(Mathf.Abs(position.x)<11.5f && Mathf.Abs(position.y)<6f, "arena bounds constrain physics ship");
                    flow.ReturnToDock();
                    Complete(true, "");
                }
            }
            catch(Exception e) { Complete(false,e.ToString()); }
        }
        static void Capture(string name)
        {
            if(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            Directory.CreateDirectory("Logs/PirateScreenshots");
            Camera camera=flow.battle.worldCamera;
            Canvas canvas=flow.ui.GetComponent<Canvas>();
            RenderMode previousMode=canvas.renderMode;
            Camera previousCamera=canvas.worldCamera;
            float previousPlane=canvas.planeDistance;
            int previousOrder=canvas.sortingOrder;
            var previousTarget=camera.targetTexture;
            var previousActive=RenderTexture.active;
            float previousAspect=camera.aspect;
            var texture=new RenderTexture(1280,720,24);
            var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder=1000;
                camera.targetTexture=texture;camera.aspect=1280f/720;
                Canvas.ForceUpdateCanvases();camera.Render();
                RenderTexture.active=texture;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
                File.WriteAllBytes("Logs/PirateScreenshots/"+name+".png",pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=previousTarget;RenderTexture.active=previousActive;camera.aspect=previousAspect;
                canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;canvas.planeDistance=previousPlane;canvas.sortingOrder=previousOrder;
                texture.Release();UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
        static void DisableControls()
        {
            var p=flow.battle.Player;p.GetComponent<PlayerShipInput>().enabled=false;p.GetComponent<CannonAim>().enabled=false;
            foreach(var ai in flow.battle.actorsRoot.GetComponentsInChildren<EnemyShipAI>())ai.enabled=false;
            foreach(var motor in flow.battle.actorsRoot.GetComponentsInChildren<ShipMotor>())motor.SetInput(0,0);
        }
        static void Check(bool condition,string label)
        {
            if(!condition)throw new Exception("FAILED: "+label);
            assertions++;report += "PASS "+label+"\n";
        }
        static void Complete(bool success,string error)
        {
            SessionState.SetBool(Key,false);EditorApplication.update-=Tick;
            string output=(success?"PASS":"FAIL")+" "+assertions+" checks\n"+report+error;
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/PirateGameValidation.txt",output);
            if(success)Debug.Log("PIRATE_GAME_VALIDATION_OK: "+assertions);else Debug.LogError(output);
            EditorApplication.isPlaying=false;
            if(Application.isBatchMode)EditorApplication.delayCall+=()=>EditorApplication.Exit(success?0:1);
        }
    }
}
