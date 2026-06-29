using BFPlus.Extensions.BattleStuff;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Extensions.EnemyAI;
using BFPlus.Extensions.Maps;
using HarmonyLib;
using InputIOManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static MainManager;

namespace BFPlus.Extensions
{
    public enum MusicSetting
    {
        Mix,
        OnlyFight,
        OnlyTIGS,
        Off,
    }
    public class MainManager_Ext : MonoBehaviour
    {
        public static AssetBundle assetBundle;
        public static AssetBundle mapPrefabs;
        public static int DiscoveriesRewardAmount = 12;
        public static int FlagNumber = 1000;
        public static int CBFlagNumber = 73;
        public static int DashFlag = 699;
        public static int FlagVarNumber = 83;
        public static Medal[] medalDupes = new Medal[] 
        { 
            Medal.Powerbank, 
            Medal.HPDown, 
            Medal.MPPlus, Medal.MPPlus, Medal.MPPlus, 
            Medal.Smearcharge, 
            Medal.DizzyResistance,
            Medal.Whirliwig    
        };
        public static string[] enemyData;
        public static string[,] extraEnemyData;
        public static bool fastText = false;
        public static bool showResistance = false;
        public static MusicSetting musicOption = MusicSetting.Mix;
        public bool guiSwapped = false;
        public static int skillListType = -1;
        public bool musicPlayer = false;
        public static int mpPlusBonus = 0;
        public const int newMaxLevel = 40;
        public static bool[] newUnlocks = new bool[Enum.GetNames(typeof(NewCode)).Length];
        public static bool newCodeUsed = false;
        public static bool inSeedlingMinigame = false;
        public static bool noJump = false;
        public int minibossAmount = -1;
        public int bossAmount = -1;
        public const int SUPERBOSS_AMOUNT = 12;
        public const int HDWGH_CONDITIONS = 10; //20 was max, nerfed cause not fun
        public int newBossMap = -1;
        List<Sprite> oldGui;
        public static AreaData backgroundData;
        static MainManager_Ext instance;
        public SavedRenderSettings savedRenderSettings;
        public static int oldOutline = -1;
        public const int MYSTERY_SHADE_PRICE = 3;
        public MedalPreset[] medalPresets = new MedalPreset[10];
        public Dictionary<int, bool> balanceChanges = new Dictionary<int, bool>()
        {
            { (int)NewMenuText.TornadoToss, true },
            { (int)NewMenuText.HurricaneToss, true },
            { (int)NewMenuText.NeedleToss, true },
            { (int)NewMenuText.NeedlePincer, true },
            { (int)NewMenuText.Understrike, true },
            { (int)NewMenuText.IceRain, true },
            { (int)NewMenuText.HeavyStrike, true },
            { (int)NewMenuText.FlyDrop, true}
        };

        Dictionary<NewMusic, NewMusic> fightMusics = new Dictionary<NewMusic, NewMusic>()
        {
            {(NewMusic)Musics.Battle0, (NewMusic)Musics.Battle6 },
            {NewMusic.BattleCaves, NewMusic.TigsCave },
            {NewMusic.BattleFactory, NewMusic.TigsFactory },
            {NewMusic.BattleFarGrasslands, NewMusic.TigsGrasslands },
            {NewMusic.BattleOutskirts, NewMusic.TigsOutskirts },
            {NewMusic.BattleForsakenLands, NewMusic.TigsForsaken },
            {NewMusic.BattleGoldenHills, NewMusic.TigsHills },
            {NewMusic.BattleMetalLake, NewMusic.TigsLake },
            {NewMusic.BattleLostSands, NewMusic.TigsDesert },
            {NewMusic.BattleRubberPrison, NewMusic.TigsPrison },
            {NewMusic.BattleSandCastle, NewMusic.TigsCastle },
            {NewMusic.BattleSwamplands, NewMusic.TigsSwamp },
            {NewMusic.BattleSnakemouthLab, NewMusic.TigsLab },
        };

        public static MainManager_Ext Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = MainManager.instance.gameObject.AddComponent<MainManager_Ext>();
                }
                return instance;
            }
        }

        public static void CheckSpuderCardEffect(Transform entity)
        {
            Transform goldStars = entity.transform.Find("GoldStars");
            if (MainManager.BadgeIsEquipped((int)Medal.SpuderCard) && !MainManager.instance.flags[916])
            {
                if (goldStars == null)
                {
                    Transform transform = (Instantiate(Resources.Load("Prefabs/Particles/GoldStars"), entity.transform.position, Quaternion.identity) as GameObject).transform;
                    transform.name = "GoldStars";
                    transform.parent = entity.transform;
                    transform.localPosition = new Vector3(0f, 0.5f, -0.2f);
                }
            }
            else
            {
                Destroy(goldStars?.gameObject);
            }
        }

        static void ResetInputCooldown() => MainManager.instance.inputcooldown = 1f;

        static void SetOptionsText(int settingType, SpriteRenderer spriteRenderer)
        {
            Dictionary<int, bool> settingsType = new Dictionary<int, bool>()
            {
                { (int)NewMenuText.TextSkip, fastText },
                { (int)NewMenuText.ShowResistance, showResistance }
            };

            if (settingsType.ContainsKey(settingType))
            {
                MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[settingsType[settingType] ? 38 : 39],
                    new Vector3(6.25f, -0.15f), spriteRenderer.transform));
            }

            if(settingType == (int)NewMenuText.NewBattleThemes)
            {
                int menuText = musicOption == MusicSetting.Off ? 39 : (int)NewMenuText.MusicMix + (int)musicOption;

                MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[menuText],
                    new Vector3(6.25f, -0.15f), spriteRenderer.transform));
            }
        }

        static string TestSkillIndex(int skillIndex, SpriteRenderer bar)
        {
            int skillID = MainManager.listvar[skillIndex];
            string text = " |size,0.55,0.6|";
            if (skillID == (int)MainManager.Skills.Cleanse)
            {
                if (MainManager.BadgeIsEquipped((int)Medal.DeepCleaning))
                {
                    text += $"|icon,{(int)NewGui.DeepCleaning}|";
                }

                if (MainManager.BadgeIsEquipped((int)Medal.RinseRegen))
                {
                    text += $"|icon,{(int)NewGui.RinseRegen}|";
                }

                if (MainManager.BadgeIsEquipped((int)Medal.Liquidate))
                {
                    text += $"|icon,{(int)NewGui.Liquidate}|";
                }
            }

            if (skillID == (int)NewSkill.RainDance)
            {
                if (MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.HealPlus, MainManager.instance.playerdata[MainManager.battle.currentturn].trueid))
                {
                    text += $"|icon,218|";
                }
            }

            if (skillID == (int)MainManager.Skills.PeebleToss)
            {
                text = " |size,0.48,0.53|";
                if (MainManager.BadgeIsEquipped((int)Medal.SkippingStone))
                {
                    text += $"|icon,{(int)NewGui.SkippingStone}|";
                }

                if (MainManager.BadgeIsEquipped((int)Medal.GrumbleGravel))
                {
                    text += $"|icon,{(int)NewGui.GrumbleGravel}|";
                }

                if (MainManager.BadgeIsEquipped((int)Medal.RockyRampUp))
                {
                    text += $"|icon,{(int)NewGui.RockyRampUp}|";
                }

                if (MainManager.BadgeIsEquipped((int)Medal.Avalanche))
                {
                    text += $"|icon,{(int)NewGui.Avalanche}|";
                }

                if (MainManager.BadgeIsEquipped((int)Medal.TanjyToss))
                {
                    text += $"|icon,{(int)NewGui.TanjyToss}|";
                }
            }
            if (skillID == (int)MainManager.Skills.PebbleTossPlus)
            {
                text = " |size,0.48,0.53|";
                if (MainManager.BadgeIsEquipped((int)Medal.Avalanche))
                    text += $"|icon,{(int)NewGui.Avalanche}|";

                if (MainManager.BadgeIsEquipped((int)Medal.HornRattle))
                    text += $"|icon,{(int)NewGui.HornRattle}|";
            }

            if (BadgeIsEquipped((int)Medal.HornRattle))
            {
                if (skillID == (int)Skills.HeavyStrike ||
                    skillID == (int)Skills.HornDash ||
                    skillID == (int)Skills.BeeFly)
                {
                    text += $" |size,0.48,0.53||icon,{(int)NewGui.HornRattle}|";
                }
            }

            if (BattleControl_Ext.Instance.holoSkillID != -1)
            {
                bar.color = Color.black;
            }

            if (skillID >= (int)NewSkill.HoloVi && skillID <= (int)NewSkill.HoloLeif)
            {
                var guiID = 0;

                if (skillID == (int)NewSkill.HoloVi)
                    guiID = (int)NewGui.HoloVi;
                else if (skillID == (int)NewSkill.HoloKabbu)
                    guiID = (int)NewGui.HoloKabbu;
                else if (skillID == (int)NewSkill.HoloLeif)
                    guiID = (int)NewGui.HoloLeif;

                bar.color = Color.black;

                foreach (Transform child in bar.transform)
                {
                    Destroy(child.gameObject);
                }

                int destinyDreamBug = Sleep.GetDestinyDreamBug();
                if (destinyDreamBug != -1)
                {
                    var destinyDreamIcon = bar.transform.Find("destinyDream");
                    Destroy(destinyDreamIcon.gameObject);
                }

                MainManager.NewUIObject("tp", bar.transform, new Vector3(2.25f, 0.27f), new Vector3(0.3f, 0.32f, 0.21f), MainManager.guisprites[guiID], 10).GetComponent<SpriteRenderer>();
            }

            return text;
        }

        public static string SetHoloSkillTextEffect(string text)
        {
            if (BattleControl_Ext.Instance.holoSkillID != -1)
            {
                if (string.IsNullOrEmpty(text))
                {
                    text += "|color,5||glitchy|";
                }
                else
                {
                    text += "|glitchy|";
                }
            }
            return text;
        }

        public static string[] GetNewItems(string[] items, string assetName, string delimiter, bool completeReplace, bool addEmpty)
        {
            if (MainManager_Ext.assetBundle == null)
            {
                MainManager_Ext.assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vengeance);
            }
            if (!completeReplace)
            {
                List<string> oldItems = items.ToList();
                if (assetName != "Items" && assetName != "ItemData")
                    oldItems.RemoveAll(string.IsNullOrEmpty);

                var newItems = assetBundle.LoadAsset<TextAsset>(assetName).ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i != newItems.Length; i++)
                {
                    if (newItems[i].Contains(delimiter))
                    {
                        string[] replacedData = newItems[i].Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                        int replaceLine = int.Parse(replacedData[0]);
                        oldItems.RemoveAt(replaceLine);
                        oldItems.Insert(replaceLine, replacedData[1]);
                        continue;
                    }
                    oldItems.Add(newItems[i]);
                }
                if (addEmpty)
                    oldItems.Add("");
                return oldItems.ToArray();
            }

            return assetBundle.LoadAsset<TextAsset>(assetName).ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] GetNewItems(string[] items, string assetName, bool addEmpty) => GetNewItems(items, assetName, "{", false, addEmpty);

        public static void SetEnemyData()
        {
            enemyData = assetBundle.LoadAsset<TextAsset>("EnemyData").ToString().Split(new char[] { '\n' });
            MainManager.enemydata = new string[enemyData.Length, enemyData[0].Split(',').Length];
            for (int i = 0; i != enemyData.Length; i++)
            {
                var data = enemyData[i].Split(',');
                for (int j = 0; j != data.Length; j++)
                {
                    MainManager.enemydata[i, j] = data[j];
                }
            }

            SetEnemyExtraData();
        }

        public static void SetQuestChecks()
        {
            var questChecksRef = typeof(MainManager).GetField("questchecks", BindingFlags.NonPublic | BindingFlags.Static);

            List<int[]> questChecks = new List<int[]>();
            var temp = assetBundle.LoadAsset<TextAsset>("QuestChecks").ToString().Split(new char[] { '\n' });

            for (int i = 0; i < temp.Length; i++)
            {
                var quest = temp[i].Split('@');
                List<int> questData = new List<int>();
                for (int j = 0; j < quest.Length; j++)
                {
                    questData.Add(Convert.ToInt32(quest[j]));
                }
                questChecks.Add(questData.ToArray());
            }

            questChecksRef.SetValue(MainManager.instance, questChecks.ToArray());
        }

        public static void SetEnemyPortrait()
        {
            List<Sprite> newsprites = new List<Sprite>(MainManager.librarysprites);
            newsprites.AddRange(assetBundle.LoadAllAssets<Sprite>().Where(sprite => sprite.name.Split('_')[0] == "EnemyPortrait").OrderBy(sprite => sprite.name.Split('_')[1]).ToArray());
            MainManager.librarysprites = newsprites.ToArray();
        }

        public static int CheckMaxCharge(int index)
        {
            return 3 + MainManager.BadgeHowManyEquipped((int)Medal.Powerbank, MainManager.instance.playerdata[index].trueid);
        }

        public static void SetMenuText()
        {
            List<string> tempList = new List<string>(MainManager.menutext);
            var temp = assetBundle.LoadAsset<TextAsset>("MenuText").ToString().Split(new char[] { '\n' });
            tempList.AddRange(temp);
            MainManager.menutext = tempList.ToArray();

            List<int> tempSetting = new List<int>(MainManager.settingsindex);
            tempSetting.Add((int)NewMenuText.TextSkip);
            tempSetting.Add((int)NewMenuText.ShowResistance);
            tempSetting.Add((int)NewMenuText.NewBattleThemes);
            tempSetting.Add((int)NewMenuText.BalanceChanges);
            MainManager.settingsindex = tempSetting.ToArray();
        }

        public static int FixHoloSkillID(int type)
        {
            skillListType = Mathf.Abs(type) - 1;
            if (MainManager.instance.inbattle)
            {
                return MainManager.battle.currentturn;
            }
            return skillListType;
        }


        public static void CreatePrefabParticle(GameObject gameObject, string objectPath, int maxAmount, float speed, float liveFrames, float cooldown, float shrinkSpeed, Vector3 limit, Vector3 maxSize, Vector3 childspin)
        {
            PrefabParticle particles = gameObject.AddComponent<PrefabParticle>();
            particles.prefabpart = Resources.Load(objectPath) as GameObject;
            particles.maxammount = maxAmount;
            particles.speed = speed;
            particles.liveframes = liveFrames;
            particles.cooldown = cooldown;
            particles.shrinkspeed = shrinkSpeed;
            particles.limits = limit;
            particles.maxsize = maxSize;
            particles.childspin = childspin;
        }

        static void CheckCustomMap(int id)
        {
            if (id <= Enum.GetValues(typeof(MainManager.Maps)).Cast<int>().Max())
            {
                GameObject gameObject = Instantiate(Resources.Load("Prefabs/Maps/" + (MainManager.Maps)id)) as GameObject;
                MainManager.map = gameObject.GetComponent<MapControl>();
                gameObject.name = id.ToString();
            }
            else
            {
                NewMaps newMap = (NewMaps)id;
                MapFactory.CreateMap(newMap).LoadMap(newMap);
            }
        }

        public static bool IsCustomMap()
        {
            return Enum.IsDefined(typeof(NewMaps), (int)MainManager.map.mapid);
        }

        public static bool HasCustomBattleMap()
        {
            Instance.savedRenderSettings = new SavedRenderSettings(MainManager.map.skyboxmat, MainManager.map.fogcolor, MainManager.map.fogend, MainManager.map.skycolor, MainManager.map.globallight);

            int battleMap = MainManager.battle.sdata.stage;
            if (MainManager.map.mapid == MainManager.Maps.HBsLab && battleMap != (int)MainManager.BattleMaps.HBsLab && battleMap != -1)
            {
                GameObject gameObject = Instantiate(Resources.Load("Prefabs/Maps/" + (MainManager.Maps)Instance.GetMapFromBattleMap((MainManager.BattleMaps)battleMap))) as GameObject;
                MapControl mapControl = gameObject.GetComponent<MapControl>();
                Instance.SetRenderSettingsFromMap(mapControl);
                Destroy(gameObject);
            }

            return IsCustomMap() || (MainManager.map.mapid == MainManager.Maps.HBsLab && Instance.newBossMap != -1 && IsInBoss()) || MainManager.map.mapid == MainManager.Maps.SnakemouthEmpty || MainManager.map.mapid == MainManager.Maps.GoldenSettlement3 || MainManager.map.mapid == MainManager.Maps.BarrenLandsEntrance;
        }

        public void SetRenderSettingsFromMap(MapControl mapControl)
        {
            RenderSettings.skybox = mapControl.skyboxmat;
            RenderSettings.fogColor = mapControl.fogcolor;
            RenderSettings.fogEndDistance = mapControl.fogend;
            RenderSettings.ambientSkyColor = mapControl.skycolor;
            RenderSettings.ambientLight = mapControl.globallight;
        }

        public static bool IsValidMap()
        {
            int mapID = (int)((MainManager.map.readdatafromothermap != MainManager.Maps.TestRoom) ? MainManager.map.readdatafromothermap : MainManager.map.mapid);
            return Resources.Load<TextAsset>("Data/EntityData/" + mapID) != null || IsCustomMap();
        }

        static GameObject LoadCustomBattleMap()
        {
            int mapId = (int)MainManager.map.mapid;

            if (mapId == (int)MainManager.Maps.HBsLab && Instance.newBossMap != -1 && MainManager.lastevent == 85)
            {
                mapId = Instance.newBossMap;
            }

            if (mapId == (int)MainManager.Maps.SnakemouthEmpty)
            {
                GameObject stage = Instantiate(Resources.Load("Prefabs/maps/SnakemouthEmpty")) as GameObject;
                MapControl mapControl = stage.GetComponent<MapControl>();
                Instance.SetRenderSettingsFromMap(mapControl);
                Destroy(stage.GetComponent<MapControl>());
                stage.transform.position = new Vector3(0, -2.5f, 0);
                return stage;
            }

            if (mapId == (int)MainManager.Maps.GoldenSettlement3)
            {
                GameObject stage = Instantiate(Resources.Load("Prefabs/maps/GoldenSettlement3")) as GameObject;
                MapControl mapControl = stage.GetComponent<MapControl>();
                Instance.SetRenderSettingsFromMap(mapControl);
                Destroy(stage.GetComponent<MapControl>());
                stage.transform.position = new Vector3(-1.6f, 0f, 1.1f);
                if (MainManager.instance.flags[682] && MainManager.instance.flags[555])
                {
                    var cerise = EntityControl.CreateNewEntity("cerise", 403, new Vector3(-1.8955f, 0, 3.9845f));
                    cerise.transform.parent = stage.transform;
                    cerise.animstate = 6;
                }
                return stage;
            }

            if (mapId == (int)MainManager.Maps.BarrenLandsEntrance)
            {
                GameObject stage = Instantiate(Resources.Load("Prefabs/maps/BarrenLandsEntrance")) as GameObject;
                MapControl mapControl = stage.GetComponent<MapControl>();
                Instance.SetRenderSettingsFromMap(mapControl);
                Destroy(stage.GetComponent<MapControl>());
                stage.transform.position = new Vector3(-7.25f, -0.1f, -0.061f);
                stage.transform.localEulerAngles = new Vector3(0f, 19.7419f, 0f);

                Transform pattonHouse = stage.transform.GetChild(1);
                pattonHouse.GetChild(0).gameObject.SetActive(false);
                pattonHouse.localPosition = new Vector3(4.52f, -0.04f, 3.16f);
                pattonHouse.localEulerAngles = new Vector3(270f, 338.47f, 0);
                pattonHouse.localScale = new Vector3(1.5f, 1.30f, 1.34f);

                GameObject tubeMesh = Resources.Load<GameObject>("prefabs/maps/UpperSnekMiddleRoom");
                pattonHouse.GetChild(5).GetChild(0).gameObject.GetComponent<MeshFilter>().mesh = tubeMesh.transform.GetChild(0).Find("Tube (4)").GetChild(0).GetComponent<MeshFilter>().mesh;

                return stage;
            }
            NewMaps newMap = (NewMaps)mapId;

            MainManager_Ext.LoadMapsBundle();
            GameObject battleMap = MapFactory.CreateMap(newMap).LoadBattleMap();
            UnloadMapsBundle();
            return battleMap;
        }

        public static void CreateItemEntity(int itemID, EntityControl entity, Vector3 startPos, int itemType)
        {
            Vector3 itemBounce = MainManager.RandomItemBounce(4f, 10f);
            NPCControl itemEntity = EntityControl.CreateItem(startPos + Vector3.up * 0.5f, itemType, itemID, itemBounce, 600);
            itemEntity.entity.TempIgnoreColision(entity.ccol, 5f);
            itemEntity.entity.LateVelocity(itemBounce);
        }

        public static void CreateMusicParticle()
        {
            if (MainManager.player != null && MainManager.player.transform.Find("MusicSimple(Clone)") == null)
            {
                var musicParticles = Instantiate(Resources.Load("Prefabs/Particles/MusicSimple")) as GameObject;
                musicParticles.transform.parent = MainManager.player.entity.transform;
                musicParticles.transform.localPosition = new Vector3(0f, 2f, -0.1f);
            }
        }

        static string[] GetEntityValues()
        {
            if (assetBundle == null)
            {
                assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vengeance);
            }
            string[] baseValues = Resources.Load<TextAsset>("Data/EntityValues").ToString().Split(new char[] { '\n' });
            string[] newValues = assetBundle.LoadAsset<TextAsset>("NewEntityValues").ToString().Replace("\r\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return baseValues.AddRangeToArray(newValues);
        }

        static int GetEntityValuesAmount()
        {
            return Enum.GetNames(typeof(MainManager.AnimIDs)).Length + Enum.GetNames(typeof(NewAnimID)).Length;
        }


        public void CheckSwitcheroo()
        {
            if (MainManager.player != null && MainManager.instance.badges != null && MainManager.guisprites != null && assetBundle != null && !MainManager.instance.flags[916])
            {
                if (MainManager.BadgeIsEquipped((int)Medal.Switcheroo) && !guiSwapped)
                {
                    if (oldGui == null)
                    {
                        oldGui = new List<Sprite>(MainManager.guisprites);
                    }

                    guiSwapped = true;
                    List<Sprite> altGui = assetBundle.LoadAssetWithSubAssets<Sprite>("alt_gui1").ToList();
                    altGui.AddRange(assetBundle.LoadAssetWithSubAssets<Sprite>("alt_gui2").ToList());

                    foreach (var sprite in altGui)
                    {
                        int index = int.Parse(sprite.name.Split(new char[] { '_' })[1]);
                        MainManager.guisprites[index] = sprite;
                    }
                    RefreshHudSprites();

                    foreach (var player in MainManager.instance.playerdata)
                    {
                        player.entity.SetAnimator();
                    }
                }

                if (oldGui != null && !MainManager.BadgeIsEquipped((int)Medal.Switcheroo) && guiSwapped)
                {
                    MainManager.guisprites = oldGui.ToArray();
                    RefreshHudSprites();
                    oldGui = null;
                    guiSwapped = false;
                    foreach (var player in MainManager.instance.playerdata)
                    {
                        player.entity.SetAnimator();
                    }
                }
            }
        }

        static void RefreshHudSprites()
        {
            var partyArray = MainManager.instance.playerdata.Select(p => p.animid).OrderBy(p => p).ToArray();
            for (int i = 0; i < partyArray.Length; i++)
            {
                var facesprite = MainManager.instance.hud[i].GetChild(0).Find("facesprite");
                facesprite.GetComponent<SpriteRenderer>().sprite = MainManager.guisprites[partyArray[i] + 5];
            }
        }

        static string[] GetNewFortuneTellerClues(string[] oldClues)
        {
            string[] newClues = new string[0];
            switch (MainManager.instance.option)
            {
                case 0:
                    //i apologize to the bf community for this irational sin of hardcoding this string
                    oldClues[39] = "|color,3|Ah yes!, In the Sacred Hills, in a bush!";

                    newClues = assetBundle.LoadAsset<TextAsset>("FortuneTeller0").ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    break;
                case 2:
                    newClues = assetBundle.LoadAsset<TextAsset>("FortuneTeller2").ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    break;
            }
            return oldClues.AddRangeToArray(newClues);
        }

        static int[][] GetNewFortuneTellerFlags(int[][] oldFlags)
        {
            if (MainManager.instance.option == 2)
            {
                int[] newFlags = new int[]
                {
                    750,
                    751,
                    752,
                    753,
                    856,
                    755,
                    757,
                    758,
                    760,
                    761,
                    765,
                    772,
                    773,
                    767,
                    783,
                    842,
                    762, //spuder card
                    784,  //liquidate
                    889, //ink bubble
                    897, //inkblot
                    898, //smearcharge
                    884, //permanent ink
                    885, //inkwell
                    879, //web sheet
                    882, //loom legs
                    880, //sturdy strands
                    881, //honey web
                    883, //slugskin
                    964, //hailstorm
                    989, //whirliwig
                    990, //corkscrew
                    992, //spinout
                };
                oldFlags[1] = oldFlags[1].AddRangeToArray(newFlags);
            }
            return oldFlags;
        }

        public static void CheckGamerFX(ref AudioClip soundclip)
        {
            if (soundclip != null && MainManager.basicload && MainManager.player != null && MainManager.BadgeIsEquipped((int)Medal.GamerFX) && !MainManager.instance.flags[916])
            {
                bool replaceSound = false;
                string[] validSounds = new string[] { "Damage0", "OverworldIce", "Cut", "WoodHit", "ShieldHit" };

                foreach (var sound in validSounds)
                {
                    if (soundclip.name == sound)
                    {
                        replaceSound = true;
                        break;
                    }
                }
                if (MainManager.instance.inbattle && MainManager.battle != null && MainManager.battle.enemy)
                    replaceSound = false;

                if (replaceSound)
                {
                    string[] gamerSounds = new string[] { "MkHit", "MkHit2", "FBPoint", "FBFlower", "FBStart", "FBDeath", "MkDeath", "MKKey", "MKPotion" };
                    soundclip = Resources.Load<AudioClip>("Audio/Sounds/" + gamerSounds[UnityEngine.Random.Range(0, gamerSounds.Length)]);
                }
            }
        }

        static Sprite GetDewlingCopter(EntityControl entity)
        {
            if (entity.animid == (int)MainManager.AnimIDs.Flowering - 1)
            {
                if (IsNewEnemy(entity, NewEnemies.Dewling))
                    return assetBundle.LoadAssetWithSubAssets<Sprite>("Dewling").Where(s => s.name == "seedlingking_46").FirstOrDefault();
                else
                    return MainManager.instance.projectilepsrites[11];
            }
            return MainManager.instance.projectilepsrites[1];
        }

        static Vector3 GetFloweringCopterScale(EntityControl entity)
        {
            if (IsNewEnemy(entity, NewEnemies.Dewling))
                return Vector3.one;
            else
                return Vector3.one * 0.75f;
        }

        static Sprite[] LoadNewTitle()
        {
            if (assetBundle == null)
                assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vengeance);
            return new Sprite[] { assetBundle.LoadAsset<Sprite>("BFPlusTitle") };
        }

        static Sprite GetNewBossPortrait()
        {
            int[] enemyIds = new int[] { MainManager.listvar[MainManager.instance.option] };
            int index = 0;

            if (enemyIds[0] == (int)NewEnemies.Mars
                || enemyIds[0] == (int)NewEnemies.LeafbugShaman || enemyIds[0] == (int)NewEnemies.DynamoSpore
                || enemyIds[0] == (int)NewEnemies.Patton || enemyIds[0] == (int)NewEnemies.JumpAnt)
            {
                return MainManager.librarysprites[MainManager.GetEnemyPortrait(enemyIds[0])];
            }

            GetNewBossIds(ref enemyIds);
            if (enemyIds.Length == 3)
            {
                var time = Mathf.Sin(Time.time * 2.5f);
                if (time <= -0.4f)
                    index = 0;
                else if (time > -0.4 && time <= 0.4f)
                    index = 1;
                else
                    index = 2;
            }
            else if (enemyIds.Length == 2)
            {
                index = Mathf.Sin(Time.time * 2.5f) > 0 ? 0 : 1;

            }

            return MainManager.librarysprites[MainManager.GetEnemyPortrait(enemyIds[index])];
        }

        static void GetNewBossIds(ref int[] enemyIds)
        {
            switch (enemyIds[0])
            {
                case (int)NewEnemies.DarkVi:
                    enemyIds = new int[] { (int)NewEnemies.DarkVi, (int)NewEnemies.DarkKabbu, (int)NewEnemies.DarkLeif };
                    break;

                case (int)NewEnemies.Mars:
                    enemyIds = new int[] { (int)NewEnemies.MarsSprout, (int)NewEnemies.Mars, (int)NewEnemies.MarsSprout };
                    break;

                case (int)NewEnemies.Levi:
                    enemyIds = new int[] { (int)NewEnemies.Levi, (int)NewEnemies.Celia };
                    break;

                case (int)NewEnemies.LeafbugShaman:
                    enemyIds = new int[] { (int)MainManager.Enemies.LeafbugNinja, (int)NewEnemies.LeafbugShaman, (int)MainManager.Enemies.LeafbugArcher };
                    break;
                case (int)NewEnemies.DynamoSpore:
                    enemyIds = new int[] { (int)NewEnemies.BatteryShroom, (int)NewEnemies.DynamoSpore, (int)NewEnemies.BatteryShroom };
                    break;

                case (int)MainManager.Enemies.Stratos:
                    enemyIds = new int[] { (int)MainManager.Enemies.Stratos, (int)MainManager.Enemies.Delilah };
                    break;

                case (int)MainManager.Enemies.DeadLanderA:
                    enemyIds = new int[] { (int)MainManager.Enemies.DeadLanderA, (int)MainManager.Enemies.DeadLanderB, (int)MainManager.Enemies.DeadLanderG };
                    break;
                case (int)MainManager.Enemies.Maki:
                    enemyIds = new int[] { (int)MainManager.Enemies.Maki, (int)MainManager.Enemies.Kina, (int)MainManager.Enemies.Yin };
                    break;

                case (int)MainManager.Enemies.TermiteSoldier:
                    enemyIds = new int[] { (int)MainManager.Enemies.TermiteSoldier, (int)MainManager.Enemies.TermiteNasute };
                    break;

                case (int)NewEnemies.Patton:
                    enemyIds = new int[] { (int)NewEnemies.MechaJaw, (int)NewEnemies.LonglegsSpider, (int)NewEnemies.Patton };
                    break;

                case (int)NewEnemies.JumpAnt:
                    enemyIds = new int[] { (int)NewEnemies.JumpAnt, (int)NewEnemies.Caveling };
                    break;
            }
        }

        static void SetNewBossEntity(int[] enemyIDs, EntityControl enemy, ref string music, ref Vector3 epos, ref int mapId)
        {
            Instance.newBossMap = -1;
            if (MainManager.instance.flags[899])
            {
                int bossMap = Instance.GetBossMap(enemyIDs[0]);

                if (bossMap != -1)
                {
                    mapId = bossMap;
                }
                else
                {
                    Instance.newBossMap = Instance.GetNewBossMap(enemyIDs[0]);
                }

            }

            switch (enemyIDs[0])
            {
                case (int)MainManager.Enemies.DeadLanderA:
                    music = "Alert";
                    break;
                case (int)MainManager.Enemies.TANGYBUG:
                    music = NewMusic.PlusBosses.ToString();
                    break;

                case (int)MainManager.Enemies.Stratos:
                case (int)MainManager.Enemies.Maki:
                    music = "Bounty";
                    break;

                case (int)NewEnemies.TermiteKnight:
                    music = "MiteKnight";
                    break;

                case (int)NewEnemies.DullScorp:
                case (int)NewEnemies.IronSuit:
                case (int)NewEnemies.BatteryShroom:
                case (int)NewEnemies.Belosslow:
                case (int)NewEnemies.Jester:
                    music = NewMusic.PlusBosses.ToString();
                    break;

                case (int)MainManager.Enemies.LeafbugNinja:
                case (int)NewEnemies.Levi:
                case (int)NewEnemies.MechaJaw:
                    music = NewMusic.NewMiniboss.ToString();
                    break;

                case (int)NewEnemies.JumpAnt:
                    music = NewMusic.JumpAntTheme.ToString();
                    break;
            }

            if (enemyIDs[0] == (int)MainManager.Enemies.LeafbugNinja)
            {
                Vector3 offset = new Vector3(3f, 0, 1f);
                int animid = (int)MainManager.AnimIDs.LeafbugNinja;

                for (int i = 0; i < 2; i++)
                {
                    if (i == 1)
                    {
                        offset = new Vector3(-3f, 0, 1f);
                        animid = (int)MainManager.AnimIDs.LeafbugArcher;
                    }
                    EntityControl leafbug = EntityControl.CreateNewEntity("leafbug", animid - 1, epos + offset);
                    leafbug.transform.parent = enemy.transform;
                    leafbug.hologram = true;
                }
            }

            if (enemyIDs[0] == (int)MainManager.Enemies.DeadLanderA)
            {
                epos = new Vector3(2f, 0.75f, 11.85f);
                int animid = (int)MainManager.AnimIDs.DeadLanderB;

                for (int i = 0; i < 2; i++)
                {
                    if (i == 1)
                    {
                        animid = (int)MainManager.AnimIDs.DeadLanderC;
                    }
                    EntityControl deadlander = EntityControl.CreateNewEntity("deadlander", animid - 1, epos + new Vector3(3f + 1 + i, 0, 1f));
                    deadlander.transform.parent = enemy.transform;
                    deadlander.height = i == 0 ? 1 : 0;
                    deadlander.hologram = true;
                }
            }

            if (enemyIDs[0] == (int)MainManager.Enemies.Maki)
            {
                epos = new Vector3(2f, 0.75f, 11.85f);
                int animid = (int)MainManager.AnimIDs.Kina;

                for (int i = 0; i < 2; i++)
                {
                    if (i == 1)
                    {
                        animid = (int)MainManager.AnimIDs.YinMoth;
                    }
                    EntityControl bug = EntityControl.CreateNewEntity("bug", animid - 1, epos + new Vector3(3f + (i * 2f), 0, 0f));
                    bug.transform.parent = enemy.transform;
                    bug.hologram = true;
                }
            }

            if (enemyIDs[0] == (int)MainManager.Enemies.Stratos)
            {
                Vector3 offset = new Vector3(3f, 0, 1f);
                EntityControl delilah = EntityControl.CreateNewEntity("delilah", (int)MainManager.AnimIDs.Delilah - 1, epos + offset);
                delilah.transform.parent = enemy.transform;
                delilah.hologram = true;
                delilah.animstate = (int)MainManager.Animations.BattleIdle;
            }

            if (enemyIDs[0] == (int)MainManager.Enemies.TermiteSoldier)
            {
                Vector3 offset = new Vector3(3f, 0, 1f);
                EntityControl poi = EntityControl.CreateNewEntity("delilah", (int)MainManager.AnimIDs.TermiteNasute - 1, epos + offset);
                poi.transform.parent = enemy.transform;
                poi.hologram = true;
                poi.animstate = (int)MainManager.Animations.BattleIdle;
            }

            if (Enum.IsDefined(typeof(NewEnemies), enemyIDs[0]))
            {
                enemy.name = "enemy" + enemyIDs[0];

                if (enemyIDs[0] == (int)NewEnemies.DarkVi)
                {
                    music = NewMusic.DarkSnek.ToString();
                    epos = new Vector3(2f, 0.75f, 11.85f);
                    EntityControl[] entities = new EntityControl[] { enemy, null, null };

                    for (int i = 0; i < entities.Length; i++)
                    {
                        if (i > 0)
                        {
                            Vector3 offset = new Vector3(3f, 0, 1f);
                            if (i == 2)
                            {
                                offset = new Vector3(4f, 0, 0f);
                            }

                            entities[i] = EntityControl.CreateNewEntity("enemy" + enemyIDs[i], i, epos + offset);
                            entities[i].transform.parent = enemy.transform;
                            entities[i].hologram = true;
                        }
                        entities[i].animstate = (int)MainManager.Animations.BattleIdle;
                    }
                }

                if (enemyIDs[0] == (int)NewEnemies.Levi)
                {
                    Vector3 offset = new Vector3(3f, 0, 1f);
                    EntityControl celia = EntityControl.CreateNewEntity("celia", (int)MainManager.AnimIDs.ShielderAnt - 1, epos + offset);
                    celia.transform.parent = enemy.transform;
                    celia.hologram = true;
                    celia.animstate = (int)MainManager.Animations.BattleIdle;
                }


                if (enemyIDs[0] == (int)NewEnemies.BatteryShroom)
                {
                    enemy.name = "enemy" + (int)NewEnemies.DynamoSpore;
                    for (int i = 0; i < 2; i++)
                    {
                        Vector3 offset = new Vector3(i == 0 ? -2f : 2, 0, 1f);
                        EntityControl batteryShroom = EntityControl.CreateNewEntity("enemy" + (int)NewEnemies.BatteryShroom, (int)MainManager.AnimIDs.Mushroom - 1, epos + offset);
                        batteryShroom.transform.parent = enemy.transform;
                        batteryShroom.hologram = true;
                    }
                }

                if (enemyIDs[0] == (int)NewEnemies.MarsSprout)
                {
                    music = "MarsTheme";
                    for (int i = 0; i < 2; i++)
                    {
                        Vector3 offset = new Vector3(i == 0 ? -3f : 3f, 0, 1f);
                        EntityControl bud = EntityControl.CreateNewEntity("bud", (int)NewAnimID.MarsSummon, epos + offset);
                        bud.transform.parent = enemy.transform;
                        bud.hologram = true;
                    }
                }

                if (enemyIDs[0] == (int)NewEnemies.MechaJaw)
                {
                    epos = new Vector3(5f, 0.7865f, 11.85f);
                    int animid = (int)MainManager.AnimIDs.LongLegs -1;

                    for (int i = 0; i < 2; i++)
                    {
                        string name = "spider";
                        if (i == 1)
                        {
                            animid = (int)NewAnimID.MechaJaw;
                            name = NewEnemies.MechaJaw.ToString();
                        }
                        EntityControl bug = EntityControl.CreateNewEntity(name, animid, epos + new Vector3(-3.5f * (1 + (i * 0.5f)), 0, 1f));
                        bug.transform.parent = enemy.transform;
                        bug.hologram = true;
                    }
                }
            }
        }

        static string GetBossNames(int enemyId)
        {
            switch (enemyId)
            {
                case (int)NewEnemies.DarkVi:
                    return "Dark Team Snakemouth";

                case (int)NewEnemies.Levi:
                    return "Team Celia";

                case (int)MainManager.Enemies.DeadLanderA:
                    return "Dead Lander Trio";

                case (int)MainManager.Enemies.Stratos:
                    return "Team Slacker";

                case (int)MainManager.Enemies.TermiteSoldier:
                    return "Cross and Poi";
            }
            return MainManager.enemynames[enemyId];
        }

        static int[] CheckBossList(List<int> bosslist)
        {
            bosslist.Remove(-2);
            var tempList = new List<int>(bosslist);
            tempList.AddRange(new int[] {
                (int)NewEnemies.JumpAnt
            });
            Instance.bossAmount = bosslist.Count;
            return tempList.ToArray();
        }

        static int[] CheckMinibossList(int[] minibosslist)
        {
            var tempList = new List<int>(minibosslist);
            tempList.AddRange(new int[] {
                (int)MainManager.Enemies.TermiteSoldier,
                (int)MainManager.Enemies.PrimalWeevil,
                (int)NewEnemies.Levi,
                (int)NewEnemies.LeafbugShaman,
                (int)NewEnemies.Patton,
                (int)MainManager.Enemies.DeadLanderA
            });
            Instance.minibossAmount = tempList.Count;
            return tempList.ToArray();
        }

        public static int[] GetSuperBosses()
        {
            return new int[] {
                (int)NewEnemies.DynamoSpore,
                (int)NewEnemies.DullScorp,
                (int)NewEnemies.Belosslow,
                (int)NewEnemies.IronSuit,
                (int)NewEnemies.Jester,
                (int)MainManager.Enemies.Maki,
                (int)MainManager.Enemies.Stratos,
                -2, //holo team snek
                (int)MainManager.Enemies.TANGYBUG,
                (int)NewEnemies.TermiteKnight,
                (int)NewEnemies.Mars,
                (int)NewEnemies.DarkVi
            };
        }

        static void CheckSuperbossList()
        {
            if (MainManager.instance.flagvar[1] == 2)
            {
                MainManager.instance.multilist = GetSuperBosses();
            }
        }

        static void CreateHeaderIcons(int challengeIndex, DialogueAnim[] saves, int saveIndex, Sprite sprite, float sizeDif, int challengeAmount, int totalChallenge)
        {
            switch (challengeIndex)
            {
                //BIGFABLE
                case 6:
                    sprite = MainManager.itemsprites[0, 142];
                    sizeDif = 0.7f;
                    break;

                //EVEN
                case 7:
                    sprite = MainManager.itemsprites[0, 102];
                    sizeDif = 0.7f;
                    break;

                //COMMAND
                case 8:
                    sprite = MainManager.guisprites[13];
                    sizeDif = 0.5f;
                    break;

                //SCAVENGE
                case 9:
                    sprite = MainManager.guisprites[22];
                    sizeDif = 0.45f;
                    break;
            }

            MainManager.NewUIObject("mode" + challengeIndex, saves[saveIndex].transform, new Vector3(5.65f, Mathf.Lerp(1f, -1.5f, (float)challengeAmount / (float)totalChallenge)), Vector3.one * sizeDif, sprite, 30 - challengeIndex + saveIndex * 5);
        }

        /// <summary>
        /// Called in Transpiler EventControl.Event8, makes new prompt with new codes for savefile.
        /// </summary>
        /// <returns></returns>
        static IEnumerator DoNewChallengesPrompt()
        {
            int newChallengesUnlocked = newUnlocks.Where(a => a).ToArray().Length;

            if (newChallengesUnlocked > 0)
            {

                int id = 0;
                NewCode[] newCodes = (NewCode[])Enum.GetValues(typeof(NewCode));
                int[] selections = new int[newChallengesUnlocked];
                bool firsttime = false;

                do
                {
                    string prompt = "|prompt,map,$0.25," + (newChallengesUnlocked + 1);
                    string a = string.Empty;
                    for (int i = 0; i < newUnlocks.Length; i++)
                    {
                        if (newUnlocks[i])
                        {
                            prompt += ",-11";
                            a += "," + "@" + newCodes[i].ToString();

                            if (!firsttime)
                            {
                                selections[id] = i;
                                id++;
                            }
                            a = a + " - " + ((!MainManager.instance.flags[(int)newCodes[i]]) ? MainManager.menutext[39] : MainManager.menutext[38]);
                        }
                    }
                    firsttime = true;
                    prompt += ",-11";
                    string text = prompt;
                    prompt = string.Concat(new string[]
                    {
                        text,
                        a,
                        ",@",
                        MainManager.menutext[42],
                        ",none|"
                    });
                    MainManager.DialogueText("|boxstyle,5||center||rainbow||font,1||spd,0||halfline|SECRET" + prompt, null, null);
                    while (MainManager.instance.message)
                    {
                        yield return null;
                    }
                    if (MainManager.instance.option < newChallengesUnlocked)
                    {
                        MainManager.instance.flags[(int)newCodes[selections[MainManager.instance.option]]] = !MainManager.instance.flags[(int)newCodes[selections[MainManager.instance.option]]];
                        yield return null;
                    }
                }
                while (MainManager.instance.option < newChallengesUnlocked);
                MainManager.PlaySound("ATKSuccess");
            }
            yield return EventControl.halfsec;
        }

        /// <summary>
        /// Called in Transpiler Event8, Checks if the filename is equals to a new custom codes and activates the proper flags.
        /// </summary>
        /// <returns></returns>
        static IEnumerator CheckNewCodeString()
        {
            NewCode[] newCodes = (NewCode[])Enum.GetValues(typeof(NewCode));
            for (int i = 0; i < newCodes.Length; i++)
            {
                if (!MainManager.instance.flags[(int)newCodes[i]] && MainManager.instance.flagstring[10] == newCodes[i].ToString())
                {
                    MainManager.PlaySound("ATKSuccess");
                    MainManager.instance.flags[(int)newCodes[i]] = true;
                    MainManager.instance.flagstring[10] = string.Empty;
                    yield return null;
                    if (!newUnlocks[i])
                    {
                        newUnlocks[i] = true;
                        InputIO.LoadSettings(true);
                    }
                    newCodeUsed = true;
                    break;
                }
            }
        }

        static bool NewCodeUsed()
        {
            if (newCodeUsed)
            {
                newCodeUsed = false;
                return true;
            }
            return false;
        }

        public static bool IsNewEnemy(EntityControl entity, NewEnemies enemyType)
        {
            return entity.name.Contains(enemyType.ToString()) || entity.name == ("enemy" + (int)enemyType);
        }

        static void SetStrategyValues()
        {
            if (MainManager.BadgeIsEquipped((int)Medal.TrustFall))
            {
                MainManager.listvar = new int[] { 0, 271, 1, 2, (int)NewMenuText.TrustFall, 3 };
            }
        }

        static int[] SetStrategyDescValues(int[] descValues)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.TrustFall))
            {
                List<int> temp = descValues.ToList();
                temp.Insert(descValues.Length - 1, (int)NewMenuText.TrustFallDesc);
                return temp.ToArray();
            }
            return descValues;
        }

        static bool CheckTrustFallColor(int index)
        {
            return MainManager.BadgeIsEquipped((int)Medal.TrustFall) && index == 4 && BattleControl_Ext.Instance.trustFallTurn > -1;
        }

        static int[] SetStrategyTextValues(int[] values)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.TrustFall))
            {
                List<int> temp = values.ToList();
                temp.Insert(values.Length - 1, (int)NewMenuText.TrustFall);
                return temp.ToArray();
            }
            return values;
        }

        static bool CanUseTrustFall()
        {
            return BattleControl_Ext.Instance.trustFallTurn == -1 && MainManager.instance.option == 4;
        }

        static int GetMaxMedals()
        {
            return 120 + Enum.GetValues(typeof(Medal)).Length + medalDupes.Length - 3; //Tp coma & Everlasting Flame && Pebble Toss;
        }

        static int GetMaxQuests()
        {
            return 60 + Enum.GetValues(typeof(NewQuest)).Length;
        }

        static void CheckCustomAI()
        {
            //add entity ext to everyone, this is after both array are initiated with entities so its perfect right here
            
            for(int i=0; i < MainManager.instance.playerdata.Length; i++)
            {
                var entityExt = Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[i].battleentity);
                entityExt.id = i;
                entityExt.lastHp = MainManager.instance.playerdata[i].hp; //lifelust medal
                entityExt.lastTurnHp = MainManager.instance.playerdata[i].hp; //determination medal
            }

            foreach (var enemy in MainManager.battle.enemydata)
            {
                var entityExt = Entity_Ext.GetEntity_Ext(enemy.battleentity);
            }

            BattleControl_Ext.Instance.statusInfo = new StatusInfo();

            if (MainManager.HasFollower(MainManager.AnimIDs.AntCapitain) && ((NewMaps)MainManager.map.mapid == NewMaps.Pit100BaseRoom || (NewMaps)MainManager.map.mapid == NewMaps.Pit100Reward) && MainManager.instance.flags[834] && !MainManager.instance.flags[835])
            {
                MainManager.battle.AddAI((int)MainManager.AnimIDs.AntCapitain - 1, (int)MainManager.Animations.BattleIdle);
            }

            int[] jumpAntEvents = { (int)NewEvents.JumpAntIntermission1, (int)NewEvents.JumpAntIntermission2, (int)NewEvents.JumpAntIntermission3End, (int)NewEvents.JumpAntIntermission4End, (int)NewEvents.JumpAntIntermission5, (int)NewEvents.JumpAntIntermission6 };

            if (jumpAntEvents.Contains(lastevent) || HasFollower((AnimIDs)NewAnimID.JumpAnt + 1))
                battle.AddAI((int)NewAnimID.JumpAnt, (int)Animations.BattleIdle);

        }

        static UnityEngine.Object CheckModel(EntityControl entity, string path)
        {
            if (entity.animid == (int)NewAnimID.Mars || entity.animid == (int)NewAnimID.MarsSummon || entity.animid == (int)NewAnimID.Jester || entity.animid == (int)NewAnimID.FirePopper || entity.animid == (int)NewAnimID.MechaJaw)
            {
                return assetBundle.LoadAsset(((NewAnimID)entity.animid).ToString());
            }

            return Resources.Load(path);
        }

        public static void AddJesterComponent(Transform obj, int linkCount, Vector3 middle, Transform parent, bool jester = true)
        {
            JesterSprings midPos = obj.gameObject.AddComponent<JesterSprings>();
            var links = new List<Transform>();

            for (int i = 0; i < linkCount; i++)
            {
                links.Add(obj.transform.GetChild(i));
            }
            //head attach
            links.Add(obj.transform.GetChild(linkCount).GetChild(jester ? 1 : 0));

            midPos.links = links.ToArray();
            midPos.middle = middle;
            midPos.localpos = false;
        }

        //temp
        static void CheckNPCEnemySound(EntityControl entity, string sound)
        {
            float volume = 1;
            if (inSeedlingMinigame)
            {
                if (sound == "Lost")
                {
                    return;
                }
                volume = 0.1f;
            }
            entity.PlaySound(sound, volume);
        }

        public static void DoNewItemUse(NewItemUse itemUse, int value, int? characterid)
        {
            switch (itemUse)
            {
                case NewItemUse.MultiUse:
                    MainManager.instance.items[0].Add(value);
                    break;
                case NewItemUse.MultiUseRandom:
                    if (UnityEngine.Random.Range(0, 100) < 50)
                    {
                        MainManager.instance.items[0].Add(value);
                    }

                    break;
                case NewItemUse.AddInk:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("WaterSplash2");
                    MainManager.SetCondition(MainManager.BattleCondition.Inked, ref MainManager.instance.playerdata[characterid.Value], value);

                    break;
                case NewItemUse.AddInkParty:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("WaterSplash2");
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
                            MainManager.SetCondition(MainManager.BattleCondition.Inked, ref MainManager.instance.playerdata[i], value);
                    }
                    break;
                case NewItemUse.AddSticky:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("AhoneynationSpit");
                    MainManager.SetCondition(MainManager.BattleCondition.Sticky, ref MainManager.instance.playerdata[characterid.Value], value);
                    break;
                case NewItemUse.AddStickyParty:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("AhoneynationSpit");
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
                            MainManager.SetCondition(MainManager.BattleCondition.Sticky, ref MainManager.instance.playerdata[i], value);
                    }
                    break;


                case NewItemUse.RandomBuffParty:
                case NewItemUse.RandomBuff:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        bool once = itemUse == NewItemUse.RandomBuff;
                        if (once)
                            i = characterid.Value;
                        if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
                        {
                            MainManager_Ext.Instance.DoRandomMysteryBuff(i, value, true);
                        }
                        if (once)
                            break;
                    }
                    break;
                case NewItemUse.RandomDebuff:
                case NewItemUse.RandomDebuffParty:

                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        bool once = itemUse == NewItemUse.RandomDebuff;
                        if (once)
                            i = characterid.Value;

                        if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
                        {
                            MainManager_Ext.Instance.DoRandomMysteryDebuff(i, value, true);
                        }

                        if (once)
                            break;
                    }
                    break;

                case NewItemUse.AddDefDown:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.battle.StatusEffect(MainManager.instance.playerdata[characterid.Value], MainManager.BattleCondition.DefenseDown, value, true, false);
                    break;
                case NewItemUse.AddAtkDown:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.battle.StatusEffect(MainManager.instance.playerdata[characterid.Value], MainManager.BattleCondition.AttackDown, value, true, false);
                    break;

                case NewItemUse.AddTaunt:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.SetCondition(MainManager.BattleCondition.Taunted, ref MainManager.instance.playerdata[characterid.Value], value);
                    MainManager.PlaySound("Taunt");
                    break;
                case NewItemUse.AddSturdy:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.SetCondition(MainManager.BattleCondition.Sturdy, ref MainManager.instance.playerdata[characterid.Value], value);
                    MainManager.PlaySound("MagicUp");
                    break;

                case NewItemUse.AddFire:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.SetCondition(MainManager.BattleCondition.Fire, ref MainManager.instance.playerdata[characterid.Value], value);
                    MainManager.PlaySound("Flame");
                    MainManager.PlayParticle("Fire", MainManager.instance.playerdata[characterid.Value].battleentity.transform.position, 1f);
                    break;

                case NewItemUse.ChargeMax:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("StatUp");
                    MainManager.instance.playerdata[characterid.Value].charge = CheckMaxCharge(characterid.Value);
                    MainManager.battle.StartCoroutine(MainManager.battle.StatEffect(MainManager.instance.playerdata[characterid.Value].battleentity, 4));
                    break;

                case NewItemUse.AddSlugskin:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("Shield", 1.4f, 1);
                    MainManager.SetCondition((MainManager.BattleCondition)NewCondition.Slugskin, ref MainManager.instance.playerdata[characterid.Value], value);
                    break;

                case NewItemUse.AtkUpParty:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                            MainManager.battle.StatusEffect(MainManager.instance.playerdata[i], MainManager.BattleCondition.AttackUp, value, true, false);
                    }
                    break;

                case NewItemUse.DefUpParty:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                            MainManager.battle.StatusEffect(MainManager.instance.playerdata[i], MainManager.BattleCondition.DefenseUp, value, true, false);
                    }
                    break;

                case NewItemUse.GradualTPParty:

                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("Heal3");
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                        {
                            MainManager.SetCondition(MainManager.BattleCondition.GradualTP, ref MainManager.instance.playerdata[i], value);
                        }
                    }
                    break;

                case NewItemUse.AddVitiation:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    MainManager.PlaySound("Shield");
                    MainManager.SetCondition((MainManager.BattleCondition)NewCondition.Vitiation, ref MainManager.instance.playerdata[characterid.Value], value);
                    break;

                case NewItemUse.AddDizzy:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    BattleControl_Ext.Instance.TryDizzy(null, ref MainManager.instance.playerdata[characterid.Value], value);
                    break;

                case NewItemUse.AddDizzyParty:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                        {
                            BattleControl_Ext.Instance.TryDizzy(null, ref MainManager.instance.playerdata[i], value);
                        }
                    }
                    break;

                case NewItemUse.DizzyAfter:
                    if (!MainManager.instance.inbattle)
                    {
                        return;
                    }
                    Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[characterid.Value].battleentity).extraData.dizzyAfter.Add(value);
                    break;
            }
        }

        public void DoRandomMysteryDebuff(int characterid, int value, bool player)
        {
            var usages = new MainManager.ItemUsage[]{ (MainManager.ItemUsage)NewItemUse.AddAtkDown, (MainManager.ItemUsage)NewItemUse.AddDefDown,
                        MainManager.ItemUsage.AddPoison,MainManager.ItemUsage.AddSleep, MainManager.ItemUsage.AddNumb, MainManager.ItemUsage.AddFreeze
                        ,(MainManager.ItemUsage)NewItemUse.AddTaunt,(MainManager.ItemUsage)NewItemUse.AddSticky,(MainManager.ItemUsage)NewItemUse.AddInk,
                        (MainManager.ItemUsage)NewItemUse.AddFire
                    };
            Instance.DoRandomItemEffect(usages, characterid, value, player);
        }

        public void DoRandomMysteryBuff(int characterid, int value, bool player)
        {
            MainManager.ItemUsage[] usages = { MainManager.ItemUsage.AtkUpStat, MainManager.ItemUsage.DefUpStat,
                        MainManager.ItemUsage.GradualHP,MainManager.ItemUsage.GradualTP, MainManager.ItemUsage.ChargeUp, MainManager.ItemUsage.TurnNextTurn
                        ,(MainManager.ItemUsage)NewItemUse.AddSturdy
                    };
            Instance.DoRandomItemEffect(usages, characterid, value, player);
        }

        void DoRandomItemEffect(MainManager.ItemUsage[] effects, int characterid, int value, bool player)
        {
            MainManager.ItemUsage randomBuff = effects[UnityEngine.Random.Range(0, effects.Length)];

            if (randomBuff == MainManager.ItemUsage.TurnNextTurn || randomBuff == (MainManager.ItemUsage)NewItemUse.AddSturdy || randomBuff == (MainManager.ItemUsage)NewItemUse.AddTaunt)
                value = 1;

            if (player)
            {
                if (randomBuff == MainManager.ItemUsage.AtkUpStat)
                {
                    MainManager.battle.StatusEffect(MainManager.instance.playerdata[characterid], MainManager.BattleCondition.AttackUp, value, true, false);

                }
                else if (randomBuff == MainManager.ItemUsage.DefUpStat)
                {
                    MainManager.battle.StatusEffect(MainManager.instance.playerdata[characterid], MainManager.BattleCondition.DefenseUp, value, true, false);
                }
                else if (randomBuff == MainManager.ItemUsage.AddFreeze)
                {
                    MainManager.PlayParticle("mothicenormal", null, MainManager.instance.playerdata[characterid].battleentity.transform.position + Vector3.up).transform.localScale = Vector3.one * 1.5f;
                    if (MainManager.HasCondition(MainManager.BattleCondition.Freeze, MainManager.instance.playerdata[characterid]) > -1 && (MainManager.instance.playerdata[characterid].battleentity.icecube == null || !MainManager.instance.playerdata[characterid].battleentity.icecube.activeInHierarchy))
                    {
                        MainManager.instance.playerdata[characterid].battleentity.Freeze();
                    }
                }
                else
                {
                    MainManager.DoItemEffect(randomBuff, value, characterid);
                }
            }
            else
            {
                StartCoroutine(BattleControl_Ext.Instance.DoItemEffect(randomBuff, value, characterid, -1, MainManager.Items.None));
            }
        }

        public static void CheckEnemyVariantAnimator(EntityControl __instance)
        {
            NewEnemies[] variants = new NewEnemies[]
            {
                NewEnemies.PirahnaChomp,
                NewEnemies.SplotchSpider,
                NewEnemies.Spineling,
                NewEnemies.Dewling,
                NewEnemies.FireAnt,
                NewEnemies.DynamoSpore,
                NewEnemies.Belosslow,
                NewEnemies.BatteryShroom,
                NewEnemies.MarsBud
            };

            foreach (var v in variants)
            {
                if (IsNewEnemy(__instance, v))
                {
                    NewEnemies enemyId = v;

                    MainManager.AnimIDs animID = (MainManager.AnimIDs)__instance.animid + 1;

                    if (__instance.anim.runtimeAnimatorController.name == animID.ToString())
                    {
                        __instance.anim.runtimeAnimatorController = assetBundle.LoadAsset<RuntimeAnimatorController>(enemyId.ToString());
                        break;
                    }

                }
            }
        }

        public static void LoadMapsBundle()
        {
            if (mapPrefabs == null)
                mapPrefabs = AssetBundle.LoadFromMemory(Properties.Resources.mapprefabs);
        }

        public static void UnloadMapsBundle()
        {
            mapPrefabs?.Unload(false);
        }

        //probably a dogshit approah, i just dont want to add any areas to existing arrays stuff
        public static string GetNewAreaNames(string __result, int mapid)
        {
            //Pit maps -> Pit of Trials
            //PowerPlant maps -> PowerPlant
            //Abandoned Tower maps -> Iron Tower
            //Deep Cave -> Deep Cave
            //Metal Lake -> Peacock Island
            string[] areaNames = new string[] { "Pit of Trials", "Power Plant", "Iron Tower", "Lush Abyss", "Peacock Island", "Sand Castle Depths", "Leafbug Village", "Giant's Playroom" };
            int areaId = GetNewAreaId(mapid);

            if (areaId != -1)
            {
                __result = areaNames[areaId];
            }
            return __result;
        }


        public static int GetNewAreaId(int mapId)
        {
            int[][] newAreasId = new int[][]
            {
                new int[]{ (int)NewMaps.Pit100BaseRoom, (int)NewMaps.Pit100Reward, (int)NewMaps.PitBossRoom, (int)MainManager.Maps.CaveOfTrials},
                new int[]{ (int)NewMaps.PowerPlantExtra, (int)NewMaps.PowerPlantBoss, (int)NewMaps.PowerPlantBigRoom, (int)NewMaps.PowerPlantElecPuzzle, (int)MainManager.Maps.PowerPlant},
                new int[]{ (int)NewMaps.AbandonedTower, (int)NewMaps.AbandonedTower1, (int)NewMaps.AbandonedTower2, (int)NewMaps.AbandonedTower3, (int)NewMaps.AbandonedTowerBoss, (int)NewMaps.AbandonedTowerCards},
                new int[]{ (int)NewMaps.DeepCave1, (int)NewMaps.DeepCave2, (int)NewMaps.DeepCaveBoss, (int)NewMaps.DeepCaveEntrance},
                new int[]{ (int)MainManager.Maps.MysteryIsland, (int)MainManager.Maps.MysteryIslandInside},
                new int[]{ (int)NewMaps.SandCastleDepthsBoss, (int)NewMaps.SandCastleDepthsIcePuzzle, (int)NewMaps.SandCastleDepthsMain, (int)NewMaps.SandCastleDepthsWall},
                new int[]{ (int)NewMaps.LeafbugVillage},
                new int[]{ (int)NewMaps.GiantLairPlayroom1,(int)NewMaps.GiantLairPlayroom2,(int)NewMaps.GiantLairPlayroom3,(int)NewMaps.GiantLairPlayroomBoss},
            };

            for (int i = 0; i < newAreasId.GetLength(0); i++)
            {
                if (newAreasId[i].Contains(mapId))
                    return i;
            }
            return -1;
        }

        public static bool CheckNewMusic(string musicClip, float fadeSpeed, int id, bool seamless)
        {
            if(BattleControl_Ext.inStartBattle)
            {
                if(BattleControl_Ext.Instance != null)
                    BattleControl_Ext.Instance.battleMusic = musicClip;
                BattleControl_Ext.inStartBattle = false;
            }

            if (musicClip != null)
            {
                if (Enum.TryParse(musicClip, out NewMusic result))
                {
                    MainManager.ChangeMusic(MainManager_Ext.assetBundle.LoadAsset<AudioClip>(musicClip), 0.1f, 0);
                    return false;
                }
            }
            return true;
        }

        public static int CheckMusicId(string musicClip)
        {
            if (Enum.TryParse(musicClip, out NewMusic result))
            {
                return (int)result;
            }
            return (int)Enum.Parse(typeof(MainManager.Musics), musicClip);
        }

        public static IEnumerator LoadNewMusicAsync(int musicId)
        {
            if (Enum.IsDefined(typeof(NewMusic), musicId))
            {
                var request = MainManager_Ext.assetBundle.LoadAssetAsync<AudioClip>(((NewMusic)musicId).ToString());
                while (!request.isDone)
                {
                    yield return null;
                }
                MainManager.ChangeMusic((AudioClip)request.asset);
            }
            else
            {
                ResourceRequest r = Resources.LoadAsync<AudioClip>("Audio/Music/" + ((MainManager.Musics)musicId).ToString());
                while (!r.isDone)
                {
                    yield return null;
                }
                MainManager.ChangeMusic((AudioClip)r.asset);
            }

        }

        static bool CheckListType(int type, ref bool showDescription)
        {
            if (type == (int)NewListType.GourmetItem)
            {
                MainManager.listvar = MainManager.GradualFill(2);
                return true;
            }

            if (type == (int)NewListType.MedalCategories)
            {
                MainManager.listvar = PauseMenu_Ext.Instance.GetObtainedCategories();
                return true;
            }

            if (type == (int)NewListType.BadgeShops)
            {
                int badgeId = 0;

                if (MainManager.map.mapid == MainManager.Maps.UndergroundBar)
                    badgeId = 1;

                MainManager.listvar = MainManager.instance.badgeshops[badgeId].ToArray();
                showDescription = !MainManager.instance.flags[681];
                return true;
            }

            if (type == (int)NewListType.MedalPreset)
            {
                if (PauseMenu_Ext.Instance.presetId == -1)
                    MainManager.listvar = MainManager.GradualFill(10);
                else
                    MainManager.listvar = MainManager.GradualFill(8);

                return true;
            }

            if (type == (int)NewListType.MusicPlayer)
            {
                List<int> list = new List<int>();
                for (int i = 0; i < MainManager.instance.samiramusics.Count; i++)
                {
                    if (MainManager.instance.samiramusics[i][1] >= 0)
                        list.Add(MainManager.instance.samiramusics[i][0]);
                }
                MainManager.listvar = list.ToArray();
                return true;
            }

            if (type == (int)NewListType.BalanceChanges)
            {
                MainManager.listvar = Instance.balanceChanges.Keys.ToArray();
                return true;
            }

            return false;
        }

        static string GetNewListText(int type, int index, SpriteRenderer listBar, ref float barYOffset, ref float textSizeX, ref float textSizeY)
        {
            if (type == (int)NewListType.GourmetItem)
            {
                string[] listText = new string[2] { "Items", "Double Dip" };
                int tpCost = GetDoubleDipCost();

                string colorText = "";
                if (MainManager.instance.tp < tpCost && index == 1)
                {
                    colorText = "|color,1|";
                }
                Sprite barSprite = index == 0 ? MainManager.guisprites[(int)NewGui.ItemLeaf] : MainManager.itemsprites[1, (int)Medal.GourmetStomach];
                Vector3 size = index == 0 ? new Vector3(0.55f, 0.6f, 1f) : new Vector3(0.45f, 0.5f, 1f);
                MainManager.NewUIObject("barSprite", listBar.transform, new Vector3(-2.5f, 0f), size, barSprite, 10);

                if (index == 1)
                {
                    MainManager.instance.StartCoroutine(MainManager.SetText("|sort,10||size,0.75||font,0|" + colorText + tpCost.ToString().PadLeft(2, ' '), new Vector3(1.6f, -0.15f), listBar.transform));
                    MainManager.NewUIObject("tp", listBar.transform, new Vector3(2.55f, 0f), new Vector3(0.45f, 0.5f, 1f), MainManager.guisprites[28], 10);
                }

                return colorText + listText[index];
            }

            if (type == (int)NewListType.MedalCategories)
            {
                PauseMenu_Ext.MedalCategory category = PauseMenu_Ext.Instance.medalCategories[MainManager.listvar[index]];
                string text = "|size,1,1||font,0||single|" + category.name + " Medals";
                textSizeX = 0.35f;
                textSizeY = -0.3f;
                barYOffset -= 0.2f;

                SpriteRenderer icon = new GameObject("itemsprite").AddComponent<SpriteRenderer>();
                icon.sprite = category.iconId >= 0 ? PauseMenu_Ext.Instance.categoryIcons[category.iconId] : MainManager.guisprites[Mathf.Abs(category.iconId)];
                icon.transform.parent = listBar.transform;
                icon.gameObject.layer = 5;
                icon.transform.localScale = Vector3.one * 0.45f;
                icon.transform.localPosition = new Vector2(8.8f, category.iconId >= 0 ? 0f : -0.45f);

                return text;
            }

            if (type == (int)NewListType.MedalPreset)
            {
                return Instance.GetMedalPresetListText(type, index, listBar, ref barYOffset, ref textSizeX, ref textSizeY);
            }

            if (type == (int)NewListType.BalanceChanges)
            {
                float offset = 2.25f;
                for (int i = 0; i < 2; i++)
                {
                    SpriteRenderer slider = MainManager.NewUIObject("slider" + i, listBar.transform, new Vector3(offset, 0f), Vector3.one, MainManager.guisprites[1]).GetComponent<SpriteRenderer>();
                    slider.sortingOrder = 10;
                    slider.transform.localEulerAngles = new Vector3(0f, 0f, i == 0 ? -90f : 90f);
                    offset += 4;
                }
                int menuTextId = Instance.balanceChanges[MainManager.listvar[index]] ? (int)NewMenuText.BFPlus : (int)NewMenuText.Vanilla;
                MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.75|" + MainManager.menutext[menuTextId],
                    new Vector3(4.25f, -0.15f), listBar.transform));
                return MainManager.menutext[MainManager.listvar[index]];
            }

            return "";
        }

        string GetMedalPresetListText(int type, int index, SpriteRenderer listBar, ref float barYOffset, ref float textSizeX, ref float textSizeY)
        {
            textSizeX = 0.35f;
            textSizeY = -0.3f;
            barYOffset -= 0.2f;
            if (PauseMenu_Ext.Instance.presetId == -1)
            {
                string presetName;
                if (Instance.medalPresets[MainManager.listvar[index]] == null)
                {
                    presetName = "Empty Preset";
                }
                else
                {
                    var preset = Instance.medalPresets[MainManager.listvar[index]];
                    presetName = preset.name;

                    for (int i = 0; i < 3; i++)
                    {
                        MainManager.NewUIObject("icon" + i, listBar.transform, new Vector2(4.8f + i * 0.8f, 0f), Vector3.one * 0.45f, PauseMenu_Ext.Instance.categoryIcons[preset.icons[i]]);
                    }
                    MainManager.instance.StartCoroutine(MainManager.SetText("|font,0|" + preset.mpNeeded.ToString().PadLeft(3), 0, null, false, false, new Vector3(7.2f, -0.2f), Vector3.zero, Vector3.one, listBar.transform, null));
                    MainManager.NewUIObject("mpIcon", listBar.transform, new Vector2(8.8f, -0.45f), Vector3.one * 0.45f, MainManager.guisprites[109]);
                }

                return "|size,1,1||font,0||single|" + presetName;
            }
            else
            {
                List<string> listText = new List<string>();

                int count = 1;
                for (int i = 0; i < 8; i++)
                {
                    int baseIndex = i < 5 ? 294 : 304;
                    listText.Add(MainManager.menutext[i < 5 ? baseIndex + i : baseIndex]);

                    if (i >= 5)
                    {
                        listText[i] = listText[i] + " " + count;
                        count++;
                    }
                }

                string colorText = "";

                if (Instance.medalPresets[PauseMenu_Ext.Instance.presetId] == null && (MainManager.listvar[index] == 0 || MainManager.listvar[index] == 2 || MainManager.listvar[index] == 3 || MainManager.listvar[index] >= 5))
                {
                    colorText = "|color,1|";
                }

                if (MainManager.listvar[index] > 4)
                {
                    int spriteId = 0;

                    if (Instance.medalPresets[PauseMenu_Ext.Instance.presetId] != null)
                    {
                        spriteId = Instance.medalPresets[PauseMenu_Ext.Instance.presetId].icons[index - 5];
                    }

                    SpriteRenderer icon = new GameObject("itemsprite").AddComponent<SpriteRenderer>();
                    icon.sprite = PauseMenu_Ext.Instance.categoryIcons[spriteId];
                    icon.transform.parent = listBar.transform;
                    icon.gameObject.layer = 5;
                    icon.transform.localScale = Vector3.one * 0.45f;
                    icon.transform.localPosition = new Vector2(7f, 0f);
                    PauseMenu_Ext.Instance.presetIcons[index - 5] = icon;

                    for (int i = 0; i < 2; i++)
                    {
                        Transform sideIcon = MainManager.NewUIObject("side", listBar.transform, new Vector3(i == 0 ? 5.5f : 8.5f, 0f), Vector3.one, MainManager.guisprites[1]).transform;
                        sideIcon.localEulerAngles = new Vector3(0f, 0f, i == 0 ? -90f : 90f);
                    }

                }

                return "|size,1,1||font,0||single|" + colorText + listText[index];
            }
        }

        static string GetNewListDesc(int type)
        {
            if (type == (int)NewListType.GourmetItem)
            {
                string[] descText = new string[2] { "Use an Item.", "Lets you use up to 2 items in a row in one action." };
                return descText[MainManager.instance.option];
            }

            if (type == (int)NewListType.MedalCategories)
            {
                int categoryIndex = MainManager.listvar[MainManager.instance.option];

                if (categoryIndex == 0)
                    return "Browse all of your medals!";

                PauseMenu_Ext.MedalCategory category = PauseMenu_Ext.Instance.medalCategories[MainManager.listvar[MainManager.instance.option]];
                return $"Browse your {category.name} medals!";
            }
            return "";
        }

        public static int GetDoubleDipCost()
        {
            if (MainManager.instance.inevent && MainManager.lastevent == 42)
                return 0;

            int tpCost = 4;

            if (MainManager.battle != null && MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.TPSaver, MainManager.instance.playerdata[MainManager.battle.currentturn].trueid))
            {
                tpCost--;
            }
            return tpCost;
        }

        static void CheckCancelListDoubleDip()
        {
            if (MainManager.listtype == 0 && BattleControl_Ext.Instance.gourmetItemUse >= 0)
            {
                if (BattleControl_Ext.Instance.gourmetItemUse == 1)
                {
                    MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp + MainManager_Ext.GetDoubleDipCost(), 0, MainManager.instance.maxtp);
                }
                else
                {
                    MainManager.battle.EndPlayerTurn();
                }
                BattleControl_Ext.Instance.gourmetItemUse = -1;
            }
        }

        static int CheckResistanceValue(int res, MainManager.BattleData data)
        {
            if (MainManager.HasCondition(MainManager.BattleCondition.Inked, data) > -1)
                res -= 25;
            return res;
        }

        static int GetAllQuestsAmount()
        {
            return Enum.GetNames(typeof(MainManager.BoardQuests)).Length - 1 + Enum.GetNames(typeof(NewQuest)).Length;
        }

        static bool CantUseSkillInked()
        {
            bool inked = MainManager.HasCondition(MainManager.BattleCondition.Inked, MainManager.instance.playerdata[MainManager.battle.currentturn]) == -1;
            return inked || (!inked && MainManager.BadgeIsEquipped((int)Medal.InvisibleInk));
        }
        static bool CantUseItemSticky()
        {
            bool stickied = MainManager.HasCondition(MainManager.BattleCondition.Sticky, MainManager.instance.playerdata[MainManager.battle.currentturn]) == -1;
            return stickied || (!stickied && MainManager.BadgeIsEquipped((int)Medal.FlavorlessAdhesive));
        }

        public static void SetupNewShops()
        {
            List<List<int>> newShops = new List<List<int>>()
            {
                new List<int>(){(int)Medal.Wildfire,(int)Medal.HeatingUp,(int)Medal.Phoenix,(int)Medal.FierySpirit,(int)Medal.FireNeedles },
                new List<int>(){(int)Medal.FlavorlessAdhesive,(int)Medal.ThickSilk,(int)Medal.SpiderBait},
                new List<int>(){(int)Medal.InkBubble,(int)Medal.Smearcharge,(int)Medal.InvisibleInk}
            };

            if (MainManager.instance.avaliablebadgepool.Length < 2 + newShops.Count)
            {
                Array.Resize(ref MainManager.instance.avaliablebadgepool, 2 + newShops.Count);
                Array.Resize(ref MainManager.instance.badgeshops, 2 + newShops.Count);

                for (int i = 0; i < newShops.Count; i++)
                {
                    if (MainManager.instance.badgeshops[2 + i] == null)
                    {
                        MainManager.instance.badgeshops[2 + i] = new List<int>(newShops[i]);
                        MainManager.instance.avaliablebadgepool[2 + i] = new List<int>();
                    }
                }
            }
        }

        static BattleControl.AttackProperty? GetStickyProperty()
        {
            return BattleControl.AttackProperty.Sticky;
        }

        public int GetBossMap(int boss)
        {
            Dictionary<int, MainManager.BattleMaps> bossToMap = new Dictionary<int, MainManager.BattleMaps>()
            {
                {(int)MainManager.Enemies.Acolyte, MainManager.BattleMaps.GoldenSettlementArena },
                {(int)MainManager.Enemies.Ahoneynation, MainManager.BattleMaps.FactoryS2 },
                {(int)MainManager.Enemies.BanditLeader, MainManager.BattleMaps.HideoutAstotheles },
                {(int)MainManager.Enemies.Scorpion, MainManager.BattleMaps.Desert1},
                {(int)MainManager.Enemies.Scarlet, MainManager.BattleMaps.Cave0},
                {(int)MainManager.Enemies.Beetle, MainManager.BattleMaps.KaliShop},
                {(int)MainManager.Enemies.Carmina, MainManager.BattleMaps.CarminaRoom},
                {(int)MainManager.Enemies.Fisherman, MainManager.BattleMaps.FarGrasslands},
                {(int)MainManager.Enemies.Cenn, MainManager.BattleMaps.Grasslands1},
                {(int)MainManager.Enemies.WaspDriller, MainManager.BattleMaps.WaspThrone},
                {(int)MainManager.Enemies.Zasp, MainManager.BattleMaps.GoldenBattle2},
                {(int)MainManager.Enemies.PrimalWeevil, MainManager.BattleMaps.BarrenLands},
                {(int)MainManager.Enemies.DeadLanderA, MainManager.BattleMaps.GiantLair2},
                {(int)MainManager.Enemies.SpuderReal, MainManager.BattleMaps.Snakemouth3},
                {(int)MainManager.Enemies.VenusBoss, MainManager.BattleMaps.GoldenHillsBoss},
                {(int)MainManager.Enemies.BeeBoss, MainManager.BattleMaps.FactoryC},
                {(int)MainManager.Enemies.ZombieRoach, MainManager.BattleMaps.SandCastleBoss},
                {(int)MainManager.Enemies.Centipede, MainManager.BattleMaps.Swamplands},
                {(int)MainManager.Enemies.UltimaxTank, MainManager.BattleMaps.RubberPrisonBoss},
                {(int)MainManager.Enemies.FlyTrap, MainManager.BattleMaps.ChomperCavesBoss},
                {(int)MainManager.Enemies.MidgeBroodmother, MainManager.BattleMaps.Broodmother},
                {(int)MainManager.Enemies.SeedlingKing, MainManager.BattleMaps.Grasslands2},
                {(int)MainManager.Enemies.SandWyrm, MainManager.BattleMaps.StreamMountainBoss},
                {(int)MainManager.Enemies.PeacockSpider, MainManager.BattleMaps.MysteryIslandInside},
                {(int)MainManager.Enemies.FalseMonarch, MainManager.BattleMaps.AbandonedTent},
                {(int)MainManager.Enemies.Pitcher, MainManager.BattleMaps.PitcherPlant},
                {(int)MainManager.Enemies.Zommoth, MainManager.BattleMaps.UpperSnekBoss},
                {(int)MainManager.Enemies.WaspKing, MainManager.BattleMaps.FinalBoss1},
                {(int)MainManager.Enemies.EverlastingKing, MainManager.BattleMaps.FinalBoss2},
                {(int)MainManager.Enemies.Maki, MainManager.BattleMaps.AntBridge},
                {(int)MainManager.Enemies.Stratos, MainManager.BattleMaps.UndergroundBar},
                {(int)MainManager.Enemies.HoloVi, MainManager.BattleMaps.AssociationHQ},
                {(int)NewEnemies.TermiteKnight, MainManager.BattleMaps.TermiteColiseum },
                {(int)MainManager.Enemies.TermiteSoldier, MainManager.BattleMaps.TermiteColiseum },
                {(int)NewEnemies.JumpAnt, MainManager.BattleMaps.AssociationHQ },
            };

            if (bossToMap.ContainsKey(boss))
            {
                if (boss == (int)MainManager.Enemies.Zasp && MainManager.instance.flags[409])
                    return (int)MainManager.BattleMaps.TermiteColiseum;

                return (int)bossToMap[boss];
            }

            return -1;
        }

        public int GetNewBossMap(int boss)
        {
            Dictionary<int, int> bossToMap = new Dictionary<int, int>()
            {
                {(int)NewEnemies.Levi, (int)NewMaps.AntPalaceTrainingRoom },
                {(int)MainManager.Enemies.LeafbugNinja, (int)NewMaps.LeafbugShamanHut },
                {(int)NewEnemies.DullScorp, (int)NewMaps.SandCastleDepthsBoss },
                {(int)NewEnemies.BatteryShroom, (int)NewMaps.PowerPlantBoss },
                {(int)NewEnemies.Belosslow, (int)NewMaps.DeepCaveBoss },
                {(int)NewEnemies.IronSuit, (int)NewMaps.AbandonedTowerBoss },
                 {(int)NewEnemies.Jester, (int)NewMaps.GiantLairPlayroomBoss },
                {(int)MainManager.Enemies.TANGYBUG, (int)MainManager.Maps.GoldenSettlement3 },
                {(int)NewEnemies.MarsSprout, (int)NewMaps.PitBossRoom },
                {(int)NewEnemies.DarkVi, (int)MainManager.Maps.SnakemouthEmpty },
                //pattons fight
                {(int)NewEnemies.MechaJaw, (int)MainManager.Maps.BarrenLandsEntrance },
            };

            if (bossToMap.ContainsKey(boss))
                return bossToMap[boss];

            return -1;
        }

        public static bool IsHolo()
        {
            return (MainManager.instance.flags[162] && MainManager_Ext.Instance.newBossMap == -1 && MainManager.battle.sdata.stage == (int)MainManager.BattleMaps.HBsLab);
        }

        public int GetMapFromBattleMap(MainManager.BattleMaps battleMap)
        {
            Dictionary<MainManager.BattleMaps, MainManager.Maps> battleMapToMap = new Dictionary<MainManager.BattleMaps, MainManager.Maps>()
            {
                {MainManager.BattleMaps.GoldenSettlementArena, MainManager.Maps.GoldenSettlement1Night },
                {MainManager.BattleMaps.FactoryS2, MainManager.Maps.FactoryStorageMiniboss },
                {MainManager.BattleMaps.HideoutAstotheles, MainManager.Maps.HideoutEntrance },
                {MainManager.BattleMaps.Desert1, MainManager.Maps.DesertScorpion },
                {MainManager.BattleMaps.Cave0, MainManager.Maps.GoldenPathTunnel },
                {MainManager.BattleMaps.KaliShop, MainManager.Maps.DefiantRoot3 },
                {MainManager.BattleMaps.CarminaRoom, MainManager.Maps.MetalIsland2 },
                {MainManager.BattleMaps.FarGrasslands, MainManager.Maps.FarGrasslandsOutsideVillage },
                {MainManager.BattleMaps.Grasslands1, MainManager.Maps.BugariaOutskirtsOutsideCity },
                {MainManager.BattleMaps.WaspThrone, MainManager.Maps.WaspKingdomThrone },
                {MainManager.BattleMaps.GoldenBattle2, MainManager.Maps.GoldenHillsDungeonEntrance },
                {MainManager.BattleMaps.BarrenLands, MainManager.Maps.BarrenLandsMiniboss },
                {MainManager.BattleMaps.GiantLair2, MainManager.Maps.GiantLairFridgeOutside },
                {MainManager.BattleMaps.Snakemouth3, MainManager.Maps.SnakemouthTreasureRoom },
                {MainManager.BattleMaps.GoldenHillsBoss, MainManager.Maps.GoldenHillsDungeonBoss },
                {MainManager.BattleMaps.FactoryC, MainManager.Maps.HoneyFactoryCore },
                {MainManager.BattleMaps.SandCastleBoss, MainManager.Maps.SandCastleBossRoom },
                {MainManager.BattleMaps.Swamplands, MainManager.Maps.SwamplandsBoss },
                {MainManager.BattleMaps.RubberPrisonBoss, MainManager.Maps.RubberPrisonGiantLairBridge },
                {MainManager.BattleMaps.ChomperCavesBoss, MainManager.Maps.ChomperCaves3 },
                {MainManager.BattleMaps.Broodmother, MainManager.Maps.BroodmotherLair },
                {MainManager.BattleMaps.Grasslands2, MainManager.Maps.SeedlingHaven },
                {MainManager.BattleMaps.StreamMountainBoss, MainManager.Maps.StreamMountain5 },
                {MainManager.BattleMaps.MysteryIslandInside, MainManager.Maps.MysteryIslandInside },
                {MainManager.BattleMaps.AbandonedTent, MainManager.Maps.AbandonedCityTent },
                {MainManager.BattleMaps.PitcherPlant, MainManager.Maps.PitcherPlantArena },
                {MainManager.BattleMaps.UpperSnekBoss, MainManager.Maps.UpperSnekBossRoom },
                {MainManager.BattleMaps.FinalBoss1, MainManager.Maps.GiantLairSaplingPlains},
                {MainManager.BattleMaps.FinalBoss2, MainManager.Maps.GiantLairSaplingPlains },
                {MainManager.BattleMaps.AntBridge, MainManager.Maps.AntBridge },
                {MainManager.BattleMaps.UndergroundBar, MainManager.Maps.UndergroundBar },
                {MainManager.BattleMaps.AssociationHQ, MainManager.Maps.BugariaOutskirtsOutsideCity },
                {MainManager.BattleMaps.TermiteColiseum, MainManager.Maps.TermiteColiseum2 },
            };

            if (battleMapToMap.ContainsKey(battleMap))
                return (int)battleMapToMap[battleMap];

            return -1;
        }

        public static void ResetRenderSettings()
        {
            RenderSettings.skybox = Instance.savedRenderSettings.skyboxMat;
            RenderSettings.fogColor = Instance.savedRenderSettings.fogColor;
            RenderSettings.fogEndDistance = Instance.savedRenderSettings.fogEndDistance;
            RenderSettings.ambientSkyColor = Instance.savedRenderSettings.ambientSkyColor;
            RenderSettings.ambientLight = Instance.savedRenderSettings.globalLight;
            Instance.savedRenderSettings = null;
        }

        static bool IsInBoss() => MainManager.instance.flags[162];
        public class SavedRenderSettings
        {
            public Material skyboxMat;
            public Color fogColor;
            public float fogEndDistance;
            public Color ambientSkyColor;
            public Color globalLight;

            public SavedRenderSettings(Material skyBox, Color fog, float endDistance, Color skyColor, Color ambientLight)
            {
                skyboxMat = skyBox;
                fogColor = fog;
                fogEndDistance = endDistance;
                ambientSkyColor = skyColor;
                globalLight = ambientLight;
            }
        }

        public static void CheckWellRestedAchievement(bool bedbug)
        {
            Dictionary<MainManager.Maps, int> mapToFlag = new Dictionary<MainManager.Maps, int>()
            {
                {MainManager.Maps.BugariaMainPlaza,  903},
                {MainManager.Maps.ChucksAbode,  904},
                {MainManager.Maps.DefiantRoot2,  905},
                {MainManager.Maps.GiantLairRoachVillage,  906},
                {MainManager.Maps.GoldenSettlement1,  907},
                {MainManager.Maps.JaunesGallery,  909},
                {MainManager.Maps.TermiteIndustrial,  910},
                {MainManager.Maps.MetalIsland2,  911},
            };


            if (!bedbug)
            {
                if (mapToFlag.ContainsKey(MainManager.map.mapid))
                    MainManager.instance.flags[mapToFlag[MainManager.map.mapid]] = true;
            }
            else
            {
                MainManager.instance.flags[912] = true;
            }

            if (!MainManager.instance.librarystuff[3, (int)NewAchievement.WellRested])
            {
                if (mapToFlag.All(m => MainManager.instance.flags[m.Value]) && MainManager.instance.flags[912])
                {
                    MainManager.instance.flags[913] = true;
                }
            }
        }

        public static T GetWeightedResult<T>(Dictionary<T, int> dictionnary)
        {
            float totalWeight = dictionnary.Sum(r => r.Value);
            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            foreach (var r in dictionnary)
            {
                if (randomValue < r.Value)
                {
                    return r.Key;
                }
                randomValue -= r.Value;
            }
            return dictionnary.Keys.First();
        }

        public RuntimeAnimatorController GetSwitcherooAnim(int id)
        {
            Dictionary<int, string> idToAnimator = new Dictionary<int, string>()
            {
                { 0, "altVi" },
                { 1, "altKabbu" },
                { 2, "altLeif" },
            };
            return MainManager_Ext.assetBundle.LoadAsset<RuntimeAnimatorController>(idToAnimator[id]);
        }

        public static int[] GetNewBounties()
        {
            return new int[]{ (int)NewQuest.BountyDullScorp, (int)NewQuest.BountyDynamoSpore, (int)NewQuest.BountyBelosslow,
                (int)NewQuest.BountyIronSuit, (int)NewQuest.BountyJester };

        }

        public static IEnumerator LerpSpriteColor(SpriteRenderer sprite, float endFrames, Color targetColor)
        {
            float a = 0f;
            Color startColor = sprite.color;
            do
            {
                sprite.color = Color.Lerp(startColor, targetColor, a / endFrames);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < endFrames);
            sprite.color = targetColor;
        }

        public void CheckSuperBugAchievement()
        {
            int[] superbosses = GetSuperBosses();
            int[] holoTeamSnek = { (int)MainManager.Enemies.HoloVi, (int)MainManager.Enemies.HoloKabbu, (int)MainManager.Enemies.HoloLeif };

            foreach (var boss in superbosses)
            {
                if (boss == -2)
                {
                    for (int i = 0; i < holoTeamSnek.Length; i++)
                    {
                        if (MainManager.instance.enemyencounter[holoTeamSnek[i], 1] <= 0)
                        {
                            return;
                        }
                    }
                    continue;
                }

                if (MainManager.instance.enemyencounter[boss, 1] <= 0)
                {
                    return;
                }

            }
            MainManager.UpdateJounal(MainManager.Library.Logbook, (int)NewAchievement.SuperBug);
        }

        public void ChangeSpritePivot(EntityControl entity, Vector2? pivot = null)
        {
            if (pivot == null)
                pivot = new Vector2(0.5f, 0.5f);

            Sprite originalSprite = entity.sprite.sprite;
            Rect spriteRect = originalSprite.rect;
            Sprite newSprite = Sprite.Create(
                originalSprite.texture,
                spriteRect,
                pivot.Value,
                originalSprite.pixelsPerUnit,
                0,
                SpriteMeshType.Tight,
                originalSprite.border
            );
            entity.sprite.sprite = newSprite;
        }
        public class MedalPreset
        {
            public List<int[]> medals = new List<int[]>();
            public int[] icons = new int[3];
            public string name = "Empty";
            public int mpNeeded;

            public override string ToString()
            {
                string medalsString = string.Join("|", medals.Select(arr => string.Join(",", arr)));
                return $"{name}[{medalsString}[{string.Join(",", icons)}[{mpNeeded}";
            }

            public static MedalPreset GetPresetFromString(string presetString)
            {
                try
                {
                    string[] parts = presetString.Split('[');
                    if (parts.Length < 4)
                    {
                        Console.WriteLine("Invalid preset format");
                        return null;
                    }

                    MedalPreset preset = new MedalPreset();
                    preset.name = parts[0];

                    string[] medals = parts[1].Split('|');
                    foreach (var medal in medals)
                    {
                        if (string.IsNullOrWhiteSpace(medal))
                            continue;
                        int[] nums = medal.Split(',').Select(s => int.Parse(s)).ToArray();
                        preset.medals.Add(nums);
                    }

                    string[] icons = parts[2].Split(',');
                    for (int i = 0; i < preset.icons.Length; i++)
                    {
                        preset.icons[i] = int.Parse(icons[i]);
                    }

                    preset.mpNeeded = int.Parse(parts[3]);

                    return preset;
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse preset string: " + presetString + "\n" + e);
                    return null;
                }
            }
        }

        public class Compressor
        {
            public static string CompressAndEncode(string input)
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                using (var output = new MemoryStream())
                {
                    using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
                    {
                        gzip.Write(bytes, 0, bytes.Length);
                    }
                    return Convert.ToBase64String(output.ToArray());
                }
            }

            public static string DecodeAndDecompress(string base64)
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(base64);
                    using (var input = new MemoryStream(bytes))
                    using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                    using (var output = new MemoryStream())
                    {
                        gzip.CopyTo(output);
                        return Encoding.UTF8.GetString(output.ToArray());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Decompress] Invalid Base64 or GZip input: {e.Message}");
                    return null;
                }
            }
        }

        static bool CantUseRelayDizzy()
        {
            bool dizzy = MainManager.HasCondition((MainManager.BattleCondition)NewCondition.Dizzy, MainManager.instance.playerdata[MainManager.battle.currentturn]) > -1;
            return dizzy;
        }

        public static Bounds GetCombinedSpriteBounds(GameObject root)
        {
            SpriteRenderer[] renderers =
                root.GetComponentsInChildren<SpriteRenderer>(true);

            if (renderers.Length == 0)
                return new Bounds(root.transform.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        public void CheckJumpAntHintsFlags()
        {
            if (MainManager.instance.flags[41] && !MainManager.instance.flags[966] && !MainManager.instance.flags[969])
                MainManager.instance.flags[969] = true;

            if (MainManager.instance.flags[966] && MainManager.instance.flags[88] && !MainManager.instance.flags[967])
            {
                MainManager.instance.flags[968] = true;
            }

            if (MainManager.instance.flags[967] && MainManager.instance.flags[299])
            {
                MainManager.instance.flags[972] = true;
            }

            if (MainManager.instance.flags[971] && MainManager.instance.flags[345] && !MainManager.instance.flags[974])
            {
                MainManager.instance.flags[975] = true;
            }

            if (MainManager.instance.flags[974] && MainManager.instance.flags[359] && !MainManager.instance.flags[976])
            {
                MainManager.instance.flags[977] = true;
            }

            if (MainManager.instance.flags[976] && MainManager.instance.flags[454] && !MainManager.instance.flags[978])
            {
                MainManager.instance.flags[979] = true;
            }
        }

        public static bool IsOutdoorMap(MapControl map)
        {
            MainManager.Areas[] indoorAreas = new MainManager.Areas[]
            {
                MainManager.Areas.BanditHideout,
                MainManager.Areas.Beehive,
                MainManager.Areas.UpperSnakemouth,
                MainManager.Areas.StreamMountain,
                MainManager.Areas.ChomperCaves,
                MainManager.Areas.GiantLair,
                MainManager.Areas.HoneyFactory,
                MainManager.Areas.SandCastle,
                MainManager.Areas.TermiteCity,
                MainManager.Areas.WaspKingdom,
                MainManager.Areas.Snakemouth,
            };

            MainManager.Maps[] indoorMapsOfOutdoorAreas = new MainManager.Maps[]
            {
                MainManager.Maps.AbandonedCityTent,
                MainManager.Maps.GoldenPathTunnel,
                MainManager.Maps.GoldenPathTunnel2,
                MainManager.Maps.CaveOfTrials,
                (MainManager.Maps)NewMaps.Pit100BaseRoom,
                MainManager.Maps.MysteryIslandInside,
                MainManager.Maps.PowerPlant,
                MainManager.Maps.BroodmotherLair,
                MainManager.Maps.FGCave,
                MainManager.Maps.WizardTowerAttic,
                MainManager.Maps.WizardTowerBasement,
                MainManager.Maps.WizardTowerStairs,
                MainManager.Maps.BarrenLandsAntTunnel,
                MainManager.Maps.MetalIsland2,
                MainManager.Maps.MetalIslandAuditorium,
                MainManager.Maps.HermitCave
            };

            int newAreaId = GetNewAreaId((int)map.mapid);

            int[] indoorNewAreas =
            {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7
            };

            int[] outdoorMaps =
            {
                (int)NewMaps.LeafbugVillage,
                (int)MainManager.Maps.RubberPrisonPier,
                (int)MainManager.Maps.SandCastleRoof,
                (int)MainManager.Maps.WaspKingdomOutside

            };

            return (!indoorAreas.Contains(map.areaid) && !indoorMapsOfOutdoorAreas.Contains(map.mapid) && (newAreaId == -1 || !indoorNewAreas.Contains(newAreaId))) || outdoorMaps.Contains((int)map.mapid);
        }

        public bool GetWhirlySeedChance()
        {
            int roll = UnityEngine.Random.Range(0, 100);
            return roll <= 2;
        }

        public void ChangeBalanceChanges(int index)
        {
            MainManager.pausemenu.SettingsToggleSound();
            Instance.balanceChanges[index] = !Instance.balanceChanges[index];
            MainManager.pausemenu.UpdateText();
        }

        public bool GetBalanceChangeState(int index)
        {
            return Instance.balanceChanges[index];
        }

        public void ReloadSkillData()
        {
            string[] skillNames = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/Skills").ToString().Split(new char[] { '\n' });
            string[] skillData = Resources.Load<TextAsset>("Data/SkillData").ToString().Split(new char[] { '\n' });

            Dictionary<int, string> newSkillNames = assetBundle.LoadAsset<TextAsset>("SkillsName").text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s.Contains("{"))
            .Select(s => s.Split(new[] { '{' }, 2))
            .ToDictionary(
                p => int.Parse(p[0]),
                p => p[1]
            );

            Dictionary<int, string> newSkillData = assetBundle.LoadAsset<TextAsset>("SkillData").text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s.Contains("{"))
            .Select(s => s.Split(new[] { '{' }, 2))
            .ToDictionary(
                p => int.Parse(p[0]),
                p => p[1]
            );


            foreach (int key in Instance.balanceChanges.Keys)
            {
                int skillIndex = -1;
                switch (key)
                {
                    case (int)NewMenuText.BubbleShield:
                        skillIndex = (int)MainManager.Skills.BubbleShieldLite;
                        break;

                    case (int)NewMenuText.HeavyStrike:
                        skillIndex = (int)MainManager.Skills.HeavyStrike;
                        break;

                    case (int)NewMenuText.IceRain:
                        skillIndex = (int)MainManager.Skills.IceRain;
                        break;

                    case (int)NewMenuText.NeedleToss:
                        skillIndex = (int)MainManager.Skills.NeedleToss;
                        break;

                    case (int)NewMenuText.NeedlePincer:
                        skillIndex = (int)MainManager.Skills.NeedlePincer;
                        break;
                }


                if (skillIndex != -1)
                {
                    int times = skillIndex == (int)MainManager.Skills.BubbleShieldLite ? 2 : 1;

                    for (int i = 0; i < times; i++)
                    {
                        string[] data = null;
                        string[] names = null;
                        if (Instance.balanceChanges[key])
                        {
                            if (newSkillData.ContainsKey(skillIndex))
                                data = newSkillData[skillIndex].Split(new char[] { '@' });

                            if (newSkillNames.ContainsKey(skillIndex))
                                names = newSkillNames[skillIndex].Split(new char[] { '@' });
                        }
                        else
                        {
                            data = skillData[skillIndex].Split(new char[] { '@' });
                            names = skillNames[skillIndex].Split(new char[] { '@' });
                        }

                        if (names != null)
                        {
                            MainManager.skilldata[skillIndex, 0] = names[0];
                            MainManager.skilldata[skillIndex, 1] = names[1];
                        }

                        if (data != null)
                        {
                            for (int j = 2; j < MainManager.skilldata.GetLength(1); j++)
                            {
                                MainManager.skilldata[skillIndex, j] = data[j - 2];
                            }
                        }

                        if (skillIndex == (int)MainManager.Skills.BubbleShieldLite)
                        {
                            skillIndex = (int)MainManager.Skills.BubbleShield;
                        }
                    }
                }
            }
        }

        static void SetEnemyExtraData()
        {
            string[] data = assetBundle.LoadAsset<TextAsset>("BattleDataExtra")
                .ToString().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            extraEnemyData = new string[data.Length, data[0].Split(new char[] { ',' }).Length];
            for (int i = 0; i != data.Length; i++)
            {
                string[] enemyData = data[i].Split(new char[] { ',' });
                for (int j = 0; j != enemyData.Length; j++)
                {
                    extraEnemyData[i, j] = enemyData[j];
                }
            }
        }

        public BattleDataExtra GetEnemyExtraData(int enemyId)
        {
            BattleDataExtra data = default(BattleDataExtra);
            data.DizzyRes = int.Parse(extraEnemyData[enemyId, 0]);
            data.MaxCharge = int.Parse(extraEnemyData[enemyId, 1]);

            data.dizzyStarOffset = ReadExtraDataVector3(enemyId, 2);
            data.slugskinOffset = ReadExtraDataVector3(enemyId, 3);
            data.slugskinScale = ReadExtraDataVector3(enemyId, 4);
            data.vitiationOffset = ReadExtraDataVector3(enemyId, 5);
            data.vitationScale = ReadExtraDataVector3(enemyId, 6);
            data.inkBubbleOffset = ReadExtraDataVector3(enemyId, 7);
            data.inkBubbleScale = ReadExtraDataVector3(enemyId, 8);
            data.noDizzySpinAnim = bool.Parse(extraEnemyData[enemyId, 9]);
            data.dizzyAfter = new List<int>();
            return data;
        }

        //temporary for offset and scales stuff
        public BattleDataExtra GetPlayerExtraData(int playerId)
        {
            BattleDataExtra data = default(BattleDataExtra);
            switch (playerId)
            {
                case 0:
                    data = GetEnemyExtraData((int)NewEnemies.DarkVi);
                    break;
                case 1:
                    data = GetEnemyExtraData((int)NewEnemies.DarkKabbu);
                    break;
                case 2:
                    data = GetEnemyExtraData((int)NewEnemies.DarkLeif);
                    break;
            }
            data.DizzyRes = BadgeHowManyEquipped((int)Medal.DizzyResistance, playerId) * 50;
            if (BadgeIsEquipped((int)BadgeTypes.ResistAll, playerId))
                data.DizzyRes += 50;
            return data;
        }

        Vector3 ReadExtraDataVector3(int enemyId, int field)
        {
            string[] vector3String = extraEnemyData[enemyId, field].Split(';');
            return new Vector3(float.Parse(vector3String[0]), float.Parse(vector3String[1]), float.Parse(vector3String[2]));
        }

        public static void ResetRecords(int[] records)
        {
            foreach (var record in records)
            {
                MainManager.instance.librarystuff[(int)MainManager.Library.Logbook, record] = false;
            }
        }

        /// <summary>
        /// Gives both TIGS and FIGHT version of songs in samiras if you heard one or the other.
        /// </summary>
        public void FixSamiraFightMusic()
        {
            Dictionary<NewMusic, NewMusic> reverseFightMusics = fightMusics.ToDictionary(x => x.Value, x => x.Key);
            List<int[]> toAdd = new List<int[]>();

            foreach (int[] music in MainManager.instance.samiramusics)
            {
                NewMusic current = (NewMusic)music[0];

                if (fightMusics.TryGetValue(current, out NewMusic linkedMusic) && 
                    !MainManager.instance.samiramusics.Any(x => x[0] == (int)linkedMusic) && 
                    !toAdd.Any(x => x[0] == (int)linkedMusic))
                {
                    toAdd.Add(new int[] { (int)linkedMusic, -1 });
                }

                if (reverseFightMusics.TryGetValue(current, out NewMusic reverseLinkedMusic) && 
                    !MainManager.instance.samiramusics.Any(x => x[0] == (int)reverseLinkedMusic)&& 
                    !toAdd.Any(x => x[0] == (int)reverseLinkedMusic))
                {
                    toAdd.Add(new int[] { (int)reverseLinkedMusic, -1 });
                }
            }
            MainManager.instance.samiramusics.AddRange(toAdd);
        }

        public bool IsNewFightMusic(NewMusic music)
        {
            return fightMusics.ContainsKey(music) || fightMusics.ContainsValue(music);
        }

        public static string GetNewMedalsString()
        {
            var randomMedals = new List<int>(Enum.GetValues(typeof(Medal)).Cast<int>());
            randomMedals.Remove((int)Medal.TPComa);
            randomMedals.Remove((int)Medal.EverlastingFlame);
            randomMedals.AddRange(MainManager_Ext.medalDupes.Cast<int>());
            return String.Join(",", Array.ConvertAll(randomMedals.ToArray(), x => x.ToString()));
        }
    }


}
