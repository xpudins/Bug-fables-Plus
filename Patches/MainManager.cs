using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using HarmonyLib;
using InputIOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static MainManager;

namespace BFPlus.Patches
{

    [HarmonyPatch(typeof(MainManager), "FixSamira")]
    public class PatchMainManagerFixSamira
    {
        static void Prefix()
        {
            if (MainManager.instance.samiramusics != null && MainManager.instance.samiramusics.Count > 0)
            {
                MainManager.instance.samiramusics.RemoveAll(s =>
                    (MainManager.Musics)s[0] == (MainManager.Musics)NewMusic.AngryViTheme ||
                    (MainManager.Musics)s[0] == (MainManager.Musics)NewMusic.SadKabbuTheme ||
                    (MainManager.Musics)s[0] == (MainManager.Musics)NewMusic.Playroom ||
                    (MainManager.Musics)s[0] == (MainManager.Musics)NewMusic.DarkSnek
                );
                MainManager_Ext.Instance.FixSamiraFightMusic();

                if (!instance.samiramusics.Any(s => s[0] == (int)NewMusic.EventBattle))
                    instance.samiramusics.Add(new int[] { (int)NewMusic.EventBattle, -1 });
            }
        }
    }

    [HarmonyPatch(typeof(MainManager), "SetUpBadges")]
    public class PatchMainManagerSetUpBadges
    {
        static void Prefix(MainManager __instance)
        {
            //Tp plus from can't sleep quest.
            __instance.badgeshops[0].AddRange(new int[]
            {
                (int)MainManager.BadgeTypes.TPPlus, (int)Medal.HPDown, (int)Medal.MPPlus, (int)Medal.TimingTutor
            });

            __instance.badgeshops[1].AddRange(new int[]
            {
                (int)Medal.HPDown, (int)Medal.Powerbank, (int)Medal.PurifyingPulse, (int)Medal.RevitalizingRipple, (int)Medal.Carousel
            });
            MainManager.instance.flags[768] = true;
            MainManager_Ext.SetupNewShops();
        }
    }

    [HarmonyPatch(typeof(MainManager), "GetQuestsBoard")]
    public class PatchMainManagerGetQuestsBoard
    {
        static void Postfix(int type, ref int[] __result)
        {
            if (type == 0 && MainManager.map.mapid != MainManager.Maps.UndergroundBar)
            {
                List<int> tempList = __result.ToList();
                int[] questToRemove = MainManager_Ext.GetNewBounties();

                foreach (var quest in questToRemove)
                    tempList.Remove(quest);

                if (tempList.Count == 0)
                    tempList.Add(0);

                __result = tempList.ToArray();
            }
        }
    }


    [HarmonyPatch(typeof(MainManager), "GetSettings")]
    public class PatchMainManagerGetSettings
    {
        static void Postfix(ref int[] __result)
        {
            List<int> tempList = new List<int>(__result);
            tempList.InsertRange(tempList.FindIndex(setting => setting == 16), new int[] { 26, 27, 28 });
            tempList.Insert(tempList.Count - 1, 29);
            __result = tempList.ToArray();
        }
    }

    [HarmonyPatch(typeof(MainManager), "EndOfMessage")]
    public class PatchMainManagerEndOfMessage
    {
        static void Postfix()
        {
            if (MainManager.instance.flags[347] && MainManager.instance.badgeshops.Length > 2)
            {
                if (MainManager.instance.badgeshops[2].Count == 0)
                {
                    //when you bough all fire medals from the shop
                    MainManager.instance.flags[830] = true;
                }
            }

            MainManager_Ext.Instance.CheckJumpAntHintsFlags();
        }
    }

    [HarmonyPatch(typeof(MainManager), "ReadSettings")]
    public class PatchMainManagerReadSettings
    {
        static void Prefix(string[] c)
        {
            if (c.Length < 49)
            {
                MainManager_Ext.fastText = false;
                MainManager_Ext.showResistance = true;
                MainManager_Ext.musicOption = MusicSetting.Mix;
            }
            else
            {
                MainManager_Ext.fastText = Convert.ToBoolean(c[44]);

                var codes = c[45].Split(',');

                if (codes.Length == MainManager_Ext.newUnlocks.Length)
                {
                    for (int i = 0; i < MainManager_Ext.newUnlocks.Length; i++)
                    {
                        MainManager_Ext.newUnlocks[i] = Convert.ToBoolean(codes[i]);
                    }
                }
                MainManager_Ext.showResistance = Convert.ToBoolean(c[46]);

                //we changed this from a simple bool to multiple value possible
                if (Enum.TryParse(c[47], out MusicSetting result))
                    MainManager_Ext.musicOption = result;
                else
                    MainManager_Ext.musicOption = MusicSetting.Mix;

                var changes = c[48].Split(';');

                if (changes.Length <= MainManager_Ext.Instance.balanceChanges.Count)
                {
                    for (int i = 0; i < changes.Length; i++)
                    {
                        string[] temp = changes[i].Split(',');
                        MainManager_Ext.Instance.balanceChanges[int.Parse(temp[0])] = Convert.ToBoolean(temp[1]);
                    }
                }
            }
        }

        //Force english language
        static void Postfix()
        {
            MainManager.languageid = 0;
        }
    }

    [HarmonyPatch(typeof(MainManager), "HasSkillCost")]
    public class PatchMainManagerHasSkillCost
    {
        static void Postfix(int tpcost, int playerid, ref bool __result)
        {
            int destinyDreamHolder = Sleep.GetDestinyDreamBug();

            if (destinyDreamHolder != -1)
            {
                __result = MainManager.instance.playerdata[destinyDreamHolder].hp > Mathf.Abs(tpcost);
            }
        }
    }

    [HarmonyPatch(typeof(MainManager), "SaveSettings")]
    public class PatchMainManagerSaveSettings
    {
        static void Postfix(ref string __result)
        {
            string newResult = string.Concat(new object[] {
                __result,
                "\n",
                MainManager_Ext.fastText,
                "\n",
                string.Join(",", MainManager_Ext.newUnlocks.Select(a => a.ToString()).ToArray()),
                "\n",
                MainManager_Ext.showResistance,
                "\n",
                MainManager_Ext.musicOption,
                 "\n",
                 string.Join(";", MainManager_Ext.Instance.balanceChanges.Select(b => $"{b.Key},{b.Value}"))
            });
            __result = newResult;
        }
    }

    [HarmonyPatch(typeof(MainManager), "GetEnemyData", new Type[] { typeof(int), typeof(bool), typeof(bool) })]
    public class PatchMainManagerGetEnemyData
    {
        static void Postfix(ref MainManager.BattleData __result)
        {
            if (__result.deathtype == 13)
            {
                __result.entity.destroytype = NPCControl.DeathType.PlayerDeath;
            }

            if (__result.animid == (int)NewEnemies.LonglegsSpider)
            {
                __result.position = BattleControl.BattlePosition.Flying;
            }
        }
    }



    [HarmonyPatch(typeof(MainManager), "RefreshSkills", new Type[] { })]
    public class PatchMainManagerRefreshSkills
    {
        static void Postfix()
        {
            int[] playerIds = MainManager.instance.playerdata.Select(p => p.trueid).ToArray();
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                int trueid = MainManager.instance.playerdata[i].trueid;

                if (trueid == 0)
                {
                    if (MainManager.BadgeIsEquipped((int)Medal.HoloSkill, trueid))
                    {
                        if (playerIds.Contains(1))
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.HoloKabbu);
                        if (playerIds.Contains(2))
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.HoloLeif);
                    }

                    if (MainManager.instance.inbattle && BattleControl_Ext.Instance.holoSkillID != -1)
                    {
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.BeeFly);
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.IceBeemerang);
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.IceSphere);
                    }

                    if (MainManager.instance.partylevel >= 8)
                    {
                        int index = MainManager.instance.inbattle ? 2 : 5;
                        if (MainManager.instance.playerdata[i].skills.Count > index)
                            MainManager.instance.playerdata[i].skills.Insert(index, (int)NewSkill.Steal);
                        else
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.Steal);
                    }

                    if (MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.ElecNeedles))
                    {
                        int index = MainManager.instance.inbattle ? 5 : 8;
                        if (MainManager.instance.playerdata[i].skills.Count > index)
                            MainManager.instance.playerdata[i].skills.Insert(index, (int)NewSkill.NeedleSurge);
                        else
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.NeedleSurge);
                        MainManager.skilldata[(int)NewSkill.NeedleSurge, 12] = MainManager.BadgeIsEquipped((int)Medal.FireNeedles).ToString();
                    }
                }
                else if (trueid == 1)
                {
                    if (MainManager.BadgeIsEquipped((int)Medal.HoloSkill, trueid))
                    {
                        if (playerIds.Contains(0))
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.HoloVi);

                        if (playerIds.Contains(2))
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.HoloLeif);
                    }
                    if (MainManager.instance.inbattle && BattleControl_Ext.Instance.holoSkillID != -1)
                    {
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.BeeFly);
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.IceDrill);
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.IceSphere);
                    }

                    if (MainManager.instance.partylevel >= 13)
                    {
                        int index = MainManager.instance.inbattle ? 4 : 7;
                        if (MainManager.instance.playerdata[i].skills.Count > index)
                            MainManager.instance.playerdata[i].skills.Insert(index, (int)NewSkill.Lecture);
                        else
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.Lecture);
                    }
                }
                else if (trueid == 2)
                {
                    if (MainManager.BadgeIsEquipped((int)Medal.HoloSkill, trueid))
                    {
                        if (playerIds.Contains(0))
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.HoloVi);

                        if (playerIds.Contains(1))
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.HoloKabbu);
                    }

                    if (MainManager.instance.inbattle && BattleControl_Ext.Instance.holoSkillID != -1)
                    {
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.IceBeemerang);
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.IceDrill);
                        MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.IceSphere);
                    }

                    if (MainManager.BadgeIsEquipped((int)Medal.ViolentVitiation))
                    {
                        if (MainManager.instance.flags[160])
                        {
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.VitiationLite);
                            MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.BubbleShieldLite);
                        }

                        if (MainManager.instance.flags[20])
                        {
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.Vitiation);
                            MainManager.instance.playerdata[i].skills.Remove((int)MainManager.Skills.BubbleShield);
                        }
                    }

                    if (!MainManager.instance.playerdata[i].skills.Contains((int)MainManager.Skills.Cleanse))
                    {
                        MainManager.instance.playerdata[i].skills.Add((int)MainManager.Skills.Cleanse);
                    }

                    if (MainManager.instance.flags[878])
                    {
                        int index = MainManager.instance.inbattle ? 3 : 6;
                        if (MainManager.instance.playerdata[i].skills.Count > index)
                            MainManager.instance.playerdata[i].skills.Insert(index, (int)NewSkill.CordycepsLeech);
                        else
                            MainManager.instance.playerdata[i].skills.Add((int)NewSkill.CordycepsLeech);
                    }
                }

                if (MainManager.BadgeIsEquipped((int)Medal.SleepSchedule, trueid))
                {
                    MainManager.instance.playerdata[i].skills.Add((int)NewSkill.SleepSchedule);
                }

                if (MainManager.BadgeIsEquipped((int)Medal.TribalDance, trueid))
                {
                    MainManager.instance.playerdata[i].skills.Add((int)NewSkill.RainDance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MainManager), "ApplyBadges")]
    public class PatchMainManagerApplyBadges
    {
        static void Prefix()
        {
            if (MainManager.instance?.playerdata != null)
            {
                foreach (var player in MainManager.instance.playerdata)
                {
                    Transform entity = MainManager.instance.inbattle ? player.battleentity?.transform : player.entity?.transform;
                    if (entity != null)
                    {
                        MainManager_Ext.CheckSpuderCardEffect(entity);
                    }
                }
            }
        }

        static void Postfix()
        {
            //10/10 TP 5/5 BP | 7 TP 8 BP
            //MP PLUS CHECK
            //int maxBP = MainManager.instance.maxbp; //5 BP

            int mpPlusBuff = 3 * MainManager.BadgeHowManyEquipped((int)Medal.MPPlus);
            MainManager.instance.maxtp -= mpPlusBuff;
            MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp, 0, MainManager.instance.maxtp);

            //3 old new 6
            if (MainManager_Ext.mpPlusBonus != mpPlusBuff)
            {
                int mpPlusDebuff = MainManager_Ext.mpPlusBonus;
                MainManager.instance.maxbp += mpPlusBuff - mpPlusDebuff;
                MainManager.instance.bp = Mathf.Clamp(MainManager.instance.bp + mpPlusBuff - mpPlusDebuff, 0, MainManager.instance.maxbp);

                MainManager_Ext.mpPlusBonus = mpPlusBuff;
            }
        }
    }

    [HarmonyPatch(typeof(MainManager), "DestroyList")]
    public class PatchMainManagerDestroyList
    {
        static void Postfix()
        {
            if (MainManager.battle != null)
                BattleControl_Ext.Instance.destroyedList = true;
        }
    }

    [HarmonyPatch(typeof(MainManager), "ChangeMusic", new Type[] { typeof(AudioClip), typeof(float), typeof(int), typeof(bool) })]
    public class PatchMainManagerChangeMusic
    {
        static bool Prefix(AudioClip musicclip, float fadespeed, int id, bool seamless)
        {
            if (MainManager_Ext.Instance.musicPlayer)
            {
                if (MainManager.lastevent == 28)
                {
                    MainManager_Ext.Instance.musicPlayer = false;
                    if (MainManager.player.transform.Find("MusicSimple(Clone)") != null)
                    {
                        UnityEngine.Object.Destroy(MainManager.player.transform.Find("MusicSimple(Clone)").gameObject);
                    }
                    return true;
                }

                if (MainManager.music[0] != null && MainManager.music[0].volume != MainManager.musicvolume)
                {
                    MainManager.music[0].volume = MainManager.musicvolume;
                }

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MainManager), "Reset")]
    public class PatchMainManagerReset
    {
        static void Prefix()
        {
            BattleControl_Ext.stylishBarAmount = 0;
        }
    }

    [HarmonyPatch(typeof(MainManager), "GetTPCost", new Type[] { typeof(int), typeof(int), typeof(bool) })]
    public class PatchMainManagerGetTPCost
    {
        static void Postfix(ref int __result, int player, int id, bool matchid)
        {
            if (MainManager.instance.inevent && MainManager.lastevent == 42)
                __result = 0;
        }
    }

    [HarmonyPatch(typeof(MainManager), "LoadMap", new Type[] { typeof(int) })]
    public class PatchMainManagerLoadMap
    {
        static void Postfix()
        {
            if (MainManager.BadgeIsEquipped((int)Medal.SpuderCard))
            {
                if (MainManager.instance?.playerdata != null)
                {
                    foreach (var player in MainManager.instance.playerdata)
                    {
                        Transform entity = player.entity?.transform;
                        if (entity != null)
                        {
                            MainManager_Ext.CheckSpuderCardEffect(entity);
                        }
                    }
                }
            }

            if (MainManager_Ext.Instance.musicPlayer)
            {
                MainManager_Ext.CreateMusicParticle();
            }

            MainManager.instance.inmusicrange = -1; //incorrectly not set to -1 by loadmap, only by transfermap, lead to issues with using
                                                    //ant compass in a music range entity
        }
    }

    [HarmonyPatch(typeof(MainManager), "SetDefaultStats")]
    public class PatchMainManagerSetDefaultStats
    {
        static void Postfix(MainManager __instance, ref MainManager.BattleData __result)
        {
            if (__result.animid == 2)
            {
                __result.hp = 8;
                __result.maxhp = __result.hp;
                __result.basehp = __result.hp;
            }
        }
    }

    [HarmonyPatch(typeof(MainManager), "ResetStats")]
    public class PatchMainManagerResetStats
    {
        static void Postfix()
        {
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].trueid == 2)
                {
                    MainManager.instance.playerdata[i].basehp = 8;
                }
            }
        }
    }


    [HarmonyPatch(typeof(MainManager), "SetVariables")]
    public class PatchMainManagerSetVariables
    {
        static void Prefix(MainManager __instance)
        {
            if (MainManager_Ext.assetBundle == null)
            {
                MainManager_Ext.assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vengeance);
            }

            var specialEnemies = (int[][])typeof(EntityControl).GetField("specialenemy", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            if (specialEnemies[1].Length == 3)
            {
                var itemsOdds = specialEnemies[0].ToList();
                itemsOdds.Add(50); //frostfly 50%

                specialEnemies[0] = itemsOdds.ToArray();

                var enemyIds = specialEnemies[1].ToList();
                enemyIds.Add((int)NewEnemies.Frostfly);
                specialEnemies[1] = enemyIds.ToArray();


                var recipepool = (int[][])typeof(EntityControl).GetField("recipepool", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                var recipes = recipepool.ToList();
                recipes.Add(new int[] { (int)MainManager.Items.ShavedIce, (int)MainManager.Items.Ice });

                typeof(EntityControl).GetField("recipepool", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, recipes.ToArray());
            }
            MainManager.librarylimit[0] = 50 + Enum.GetNames(typeof(NewDiscoveries)).Length;
            MainManager.librarylimit[1] = 92 + Enum.GetNames(typeof(NewEnemies)).Length + 1;
            MainManager.librarylimit[2] = 70 + Enum.GetNames(typeof(NewRecipes)).Length;
            MainManager.librarylimit[3] = 30 + Enum.GetNames(typeof(NewAchievement)).Length;
        }
        static void Postfix()
        {
            Array.Resize(ref MainManager.instance.crystalbflags, MainManager_Ext.CBFlagNumber);

            var prizeenemyids = MainManager.instance.prizeenemyids.ToList();
            prizeenemyids.Add((int)MainManager.Enemies.Acolyte);
            prizeenemyids.Add((int)NewEnemies.TermiteKnight);
            prizeenemyids.Add((int)NewEnemies.LeafbugShaman);
            prizeenemyids.Add((int)NewEnemies.Patton);
            prizeenemyids.Add((int)NewEnemies.Levi);
            prizeenemyids.Add((int)NewEnemies.Mars);
            prizeenemyids.Add((int)NewEnemies.JumpAnt);
            MainManager.instance.prizeenemyids = prizeenemyids.ToArray();

            var prizeflags = MainManager.instance.prizeflags.ToList();
            prizeflags.Add((int)NewFlagVar.Aria_Reward);
            prizeflags.Add((int)NewFlagVar.TermiteKnight_Reward);
            prizeflags.Add((int)NewFlagVar.LeafbugShaman_Reward);
            prizeflags.Add((int)NewFlagVar.Patton_Reward);
            prizeflags.Add((int)NewFlagVar.TeamCelia_Reward);
            prizeflags.Add((int)NewFlagVar.Mars_Reward);
            prizeflags.Add((int)NewFlagVar.JumpAnt_Reward);
            MainManager.instance.prizeflags = prizeflags.ToArray();

            var prizeids = MainManager.instance.prizeids.ToList();
            prizeids.Add((int)Medal.Yawn);
            prizeids.Add((int)Medal.IgnitedMite);
            prizeids.Add((int)Medal.TribalDance);
            prizeids.Add((int)Medal.Adrenaline);
            prizeids.Add((int)Medal.MPPlus);
            prizeids.Add((int)Medal.Mothflower);
            prizeids.Add((int)Medal.MiniMegaMush);
            MainManager.instance.prizeids = prizeids.ToArray();

            MainManager_Ext.SetMenuText();
            MainManager_Ext.SetEnemyData();
            MainManager_Ext.SetEnemyPortrait();
            MainManager_Ext.SetQuestChecks();

            MainManager.instance.flags = new bool[MainManager_Ext.FlagNumber];

            MainManager.instance.flags[411] = true;
            MainManager.instance.flags[414] = true;

            Sprite[] sprites = MainManager_Ext.assetBundle.LoadAllAssets<Sprite>().Where(sprite => sprite.name.Split('_')[0] == "medal").OrderBy(sprite => sprite.name.Split('_')[1]).ToArray();
            for (int i = 0; i != sprites.Length; i++)
                MainManager.itemsprites[1, int.Parse(sprites[i].name.Split('_')[1])] = sprites[i];

            sprites = MainManager_Ext.assetBundle.LoadAllAssets<Sprite>().Where(sprite => sprite.name.Split('_')[0] == "item").OrderBy(sprite => sprite.name.Split('_')[1]).ToArray();
            for (int i = 0; i != sprites.Length; i++)
                MainManager.itemsprites[0, int.Parse(sprites[i].name.Split('_')[1])] = sprites[i];

            List<Sprite> newsprites = new List<Sprite>(MainManager.guisprites);
            sprites = MainManager_Ext.assetBundle.LoadAllAssets<Sprite>().Where(sprite => sprite.name.Split('_')[0] == "newGui").OrderBy(sprite => sprite.name.Split('_')[1]).ToArray();
            newsprites.AddRange(sprites);
            MainManager.guisprites = newsprites.ToArray();

            Sprite newStickyIcon = MainManager_Ext.assetBundle.LoadAsset<Sprite>("newGui_246");

            MainManager.guisprites[199] = newStickyIcon;

            MainManager.instance.conditionsprites[9] = MainManager.guisprites[(int)NewGui.Unsteady];
            MainManager.instance.conditionsprites[10] = MainManager.guisprites[(int)NewGui.Flipped];
            MainManager.instance.conditionsprites[20] = newStickyIcon;

            MainManager.instance.conditionsprites = MainManager.instance.conditionsprites.AddRangeToArray(new Sprite[] { MainManager.guisprites[(int)NewGui.Tiny], MainManager.guisprites[(int)NewGui.Huge], MainManager.guisprites[(int)NewGui.Dizzy], MainManager.guisprites[(int)NewGui.InkBubble], MainManager.guisprites[(int)NewGui.Slugskin], MainManager.guisprites[(int)NewGui.Vitiation] });

            MainManager.commandhelptext = MainManager.commandhelptext.AddRangeToArray(MainManager_Ext.assetBundle.LoadAsset<TextAsset>("ActionCommands").ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
            MainManager.musicnames = MainManager.musicnames.AddRangeToArray(MainManager_Ext.assetBundle.LoadAsset<TextAsset>("MusicList").ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
            MainManager.commondialogue = MainManager.commondialogue.AddRangeToArray(MainManager_Ext.assetBundle.LoadAsset<TextAsset>("CommonDialogues").ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

            MainManager.musicnames[1] = "FIGHT!";

            newsprites = new List<Sprite>(MainManager.grasssprite);
            sprites = MainManager_Ext.assetBundle.LoadAllAssets<Sprite>().Where(sprite => sprite.name.Split('_')[0] == "bush").OrderBy(sprite => int.Parse(sprite.name.Split('_')[1])).ToArray();
            newsprites.AddRange(sprites);
            MainManager.grasssprite = newsprites.ToArray();
        }
    }


    [HarmonyPatch(typeof(MainManager), "Load")]
    public class PatchMainManagerLoad
    {
        /*static Exception Finalizer()
        {
            return null;
        }*/
        static void Postfix(ref MainManager.LoadData? __result, int file, bool lite)
        {
            if (MainManager.instance != null && __result != null)
            {
                string[] data = new string[] { string.Empty };
                if (InputIO.IsConsole)
                    data = InputIO.ReadFile("save" + file + ".dat").Split('\n');
                else
                {
                    try
                    {
                        data = InputIO.Encrypt(InputIO.ReadFile("save" + file + ".dat")).Split('\n');
                    }
                    catch
                    {
                        Console.WriteLine("Cant load file for some reason :/");
                        return;
                    }
                }

                if (lite)
                {

                    string[] header = data[0].Split(',');
                    MainManager.LoadData value = __result.Value;
                    if (__result.Value.challenges.Length < 7)
                    {
                        var temp = __result.Value.challenges.ToList();
                        for (int i = 0; i < MainManager_Ext.newUnlocks.Length; i++)
                            temp.Add(false);
                        value.challenges = temp.ToArray();
                    }

                    for (int i = 0; i < MainManager_Ext.newUnlocks.Length; i++)
                    {
                        if (header.Length > 10 + i)
                        {
                            value.challenges[6 + i] = header[10 + i] == "True";
                        }
                    }
                    __result = value;

                }
                else
                {
                    MainManager.instance.flags = new bool[MainManager_Ext.FlagNumber];
                    MainManager.instance.crystalbflags = new bool[MainManager_Ext.CBFlagNumber];
                    MainManager.instance.flagvar = new int[MainManager_Ext.FlagVarNumber];
                    string[] flags = data[11].Split(',');

                    for (int i = 0; i < flags.Length; i++)
                    {
                        if (i < MainManager.instance.flags.Length)
                            MainManager.instance.flags[i] = Convert.ToBoolean(flags[i]);
                    }

                    string[] cbFlags = data[15].Split(',');
                    for (int i = 0; i < cbFlags.Length; i++)
                    {
                        if (i < MainManager.instance.crystalbflags.Length)
                            MainManager.instance.crystalbflags[i] = Convert.ToBoolean(cbFlags[i]);
                    }

                    string[] flagvar = data[13].Split(new char[] { ',' });
                    for (int i = 0; i < flagvar.Length; i++)
                    {
                        MainManager.instance.flagvar[i] = Convert.ToInt32(flagvar[i]);
                    }

                    MainManager.instance.flags[411] = true;
                    MainManager.instance.flags[414] = true;
                    BattleControl_Ext.stylishBarAmount = 0;
                    BattleControl_Ext.stylishReward = StylishReward.None;
                    MainManager_Ext.SetupNewShops();

                    if (!MainManager.instance.flags[901])
                    {
                        var superbosses = MainManager_Ext.GetSuperBosses();
                        for (int i = 0; i < superbosses.Length; i++)
                        {
                            if (superbosses[i] == -2)
                                superbosses[i] = (int)MainManager.Enemies.HoloVi;

                            if (MainManager.instance.enemyencounter[superbosses[i], 0] > 0)
                            {
                                MainManager.instance.flags[901] = true;
                                break;
                            }
                        }
                    }

                    //this shouldnt happen on new patch but i dont want to have to edit other people save so
                    //if we havent unlocked B.O.S.S and completed the new analysis quest, unlock b.o.s.s
                    if (!MainManager.instance.flags[161] && (MainManager.instance.flags[876] || MainManager.instance.flags[878]))
                        MainManager.instance.flags[161] = true;

                    //Fix vanilla file not having a way to have these music
                    int[] newMusics = new int[] { (int)NewMusic.KabbuTheme, (int)NewMusic.ViTheme };
                    foreach (var music in newMusics)
                    {
                        if (!MainManager.instance.samiramusics.Any(array => array.Length > 0 && array[0] == music))
                        {
                            MainManager.instance.samiramusics.Add(new int[] { music, -1 });
                        }
                    }

                    ///Reset the conflicting vision quest taken quest so aria can re-appear in the beehive
                    if (MainManager.instance.flags[891])
                        MainManager.instance.flags[890] = false;

                    ///Reset the in need of training taken quest so levi celia can appear in the training grounds
                    if (MainManager.instance.flags[859])
                        MainManager.instance.flags[858] = true;

                    //ink bubble under rock got
                    MainManager.instance.flags[889] = true;

                    MainManager_Ext.Instance.CheckJumpAntHintsFlags();
                    MainManager_Ext.Instance.FixSamiraFightMusic();

                    var inkBubbles = MainManager.instance.badges.Where(b => b[0] == (int)Medal.InkBubble).ToList();
                    if (inkBubbles.Count >= 2)
                    {
                        MainManager.instance.badges.Remove(inkBubbles[0]);
                    }

                    //Pre-jumpant update save
                    if (!MainManager.instance.flags[983])
                    {
                        MainManager.instance.flags[983] = true;

                        if (MainManager.instance.flags[681] && flags.Length >= 900)
                        {
                            string medalList = MainManager.instance.flagstring[13];
                            string newMedals = medalList == "" ? "" : ",";
                            newMedals += "167,168,169,170,170,171,171,172,173,174,175";
                            MainManager.instance.flagstring[13] += newMedals;
                            MainManager.GetRandomMedal(true, true);
                        }
                        
                        if (MainManager.instance.flags[88])
                        {
                            MainManager.instance.badgeshops[0].Add((int)Medal.DizzyResistance);
                        }
                        MainManager.instance.badgeshops[1].Add((int)Medal.Carousel);

                        //bought all medals at the merab shop 
                        MainManager.instance.flags[587] = false;

                        //bought all shades medal
                        MainManager.instance.flags[588] = false;

                        //our job is done flag
                        MainManager.instance.flags[63] = false;

                        //completed all quests
                        MainManager.instance.flags[671] = false;

                        //if completed pattons request, add whirliwig to medal shop
                        if (MainManager.instance.flags[934])
                        {
                            MainManager.instance.badgeshops[0].Add((int)Medal.Whirliwig);
                        }



                        int[] recordsId = { 1, 2, 3, 7, 8, 9, 27 };
                        MainManager_Ext.ResetRecords(recordsId);
                    }

                    //vanilla save
                    if (flags.Length < 900)
                    {
                        if (MainManager.instance.flags[681])
                        {
                            string medalList = MainManager.instance.flagstring[13];
                            string newMedals = medalList == "" ? "" : ",";
                            newMedals += MainManager_Ext.GetNewMedalsString();
                            MainManager.instance.flagstring[13] += newMedals;
                            MainManager.GetRandomMedal(true, true);
                        }

                        if (MainManager.instance.flags[627])
                        {
                            MainManager.instance.badgeshops[0].Add((int)Medal.SleepSchedule);
                        }
                        else
                        {
                            MainManager.instance.badgeshops[0].Add((int)MainManager.BadgeTypes.TPPlus);
                        }

                        if (MainManager.instance.flags[103])
                        {
                            MainManager.AddPrizeMedal((int)NewPrizeFlag.Aria);
                        }

                        if (MainManager.instance.flags[299])
                        {
                            MainManager.instance.badgeshops[1].Add((int)Medal.ChargeGuard);
                        }

                        if (MainManager.instance.flags[348])
                        {
                            MainManager.instance.badgeshops[1].Add((int)Medal.Recharge);
                        }

                        MainManager.instance.badgeshops[0].Add((int)Medal.HPDown);
                        MainManager.instance.badgeshops[0].Add((int)Medal.MPPlus);
                        MainManager.instance.badgeshops[0].Add((int)Medal.TimingTutor);
                        MainManager.instance.badgeshops[1].Add((int)Medal.HPDown);
                        MainManager.instance.badgeshops[1].Add((int)Medal.Powerbank);
                        MainManager.instance.badgeshops[1].Add((int)Medal.RevitalizingRipple);
                        MainManager.instance.badgeshops[1].Add((int)Medal.PurifyingPulse);

                        //completed all bounties flag
                        MainManager.instance.flags[612] = false;

                        //Received the Mysterious Piece from Neolith after starting Chapter 4
                        if (MainManager.instance.flags[411])
                        {
                            MainManager.instance.flags[238] = false;
                        }

                        //if chuck quest is completed, add grumble gravel to badgeshop
                        if (MainManager.instance.flags[45])
                        {
                            MainManager.instance.badgeshops[0].Add((int)Medal.GrumbleGravel);
                        }

                        int[] recordsId = { 1, 2, 3, 4, 7, 8, 9, 10, 27, 28 };
                        MainManager_Ext.ResetRecords(recordsId);

                        Console.WriteLine("save is new to bf plus, adding medals in badge shop, resetting records flags");
                    }
                    else
                    {
                        if (data.Length > 18)
                        {
                            string[] stylishData = data[18].Split(new char[] { '@' });
                            BattleControl_Ext.stylishBarAmount = float.Parse(stylishData[0]);

                            if (Enum.TryParse(stylishData[1], out StylishReward reward))
                            {
                                BattleControl_Ext.stylishReward = reward;
                            }
                        }

                        if (data.Length > 19)
                        {
                            string[] presetData = data[19].Split(new char[] { '@' });

                            for (int i = 0; i < presetData.Length; i++)
                            {
                                MainManager_Ext.Instance.medalPresets[i] = MainManager_Ext.MedalPreset.GetPresetFromString(presetData[i]);
                            }
                        }
                    }
                }
            }

        }
    }

    [HarmonyPatch(typeof(MainManager), "SaveFile")]
    public class PatchMainManagerSaveFile
    {
        static void Postfix(ref string __result)
        {
            string[] data = __result.Split('\n');
            foreach (int code in Enum.GetValues(typeof(NewCode)))
            {
                data[0] = data[0] + ',' + MainManager.instance.flags[code];
            }
            List<string> temp = data.ToList();
            temp.Add(BattleControl_Ext.stylishBarAmount + "@" + BattleControl_Ext.stylishReward);
            temp.Add(string.Join("@", MainManager_Ext.Instance.medalPresets.Where(p => p != null).Select(p => p.ToString())));
            __result = String.Join("\n", temp.ToArray());
        }
    }

    [HarmonyPatch(typeof(MainManager), "DoItemEffect")]
    public class PatchMainManagerDoItemEffect
    {
        static int[] conditionAmount = new int[3];
        static void Prefix(MainManager.ItemUsage type, ref int value, int? characterid)
        {
            if (MainManager.instance.inbattle && MainManager.battle != null)
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    conditionAmount[i] = MainManager.instance.playerdata[i].condition.Count;
                }

                if (characterid != null)
                {
                    if (!MainManager.battle.enemy && MainManager.HasCondition(MainManager.BattleCondition.Sticky, MainManager.instance.playerdata[MainManager.battle.currentturn]) > -1 && MainManager.BadgeIsEquipped((int)Medal.FlavorlessAdhesive))
                    {
                        MainManager.ItemUsage[] bannedUsage = {
                        (MainManager.ItemUsage)NewItemUse.MultiUse, (MainManager.ItemUsage)NewItemUse.MultiUseRandom,
                        MainManager.ItemUsage.Battle, MainManager.ItemUsage.None,
                    };

                        if (!bannedUsage.Contains(type))
                        {
                            value = Mathf.Clamp(Mathf.CeilToInt(value / 2), 1, 99);
                        }
                    }

                    if (MainManager.BadgeIsEquipped((int)Medal.FlavorCharger))
                    {
                        BattleControl_Ext.Instance.CheckFlavorCharger(type, characterid);
                    }
                }
            }

            if (Enum.TryParse<NewItemUse>(type.ToString(), out NewItemUse result))
            {
                MainManager_Ext.DoNewItemUse(result, value, characterid);
            }
        }

        static void Postfix(MainManager.ItemUsage type, ref int value, int? characterid)
        {
            if (MainManager.instance.inbattle && MainManager.battle != null)
            {
                List<MainManager.ItemUsage> cureType = new List<MainManager.ItemUsage>() { MainManager.ItemUsage.CureAll, MainManager.ItemUsage.CureFire, MainManager.ItemUsage.CureFreeze, MainManager.ItemUsage.CureNumb, MainManager.ItemUsage.CureParty, MainManager.ItemUsage.CurePoison, MainManager.ItemUsage.CurePoisonAll, MainManager.ItemUsage.CureSleep };

                if (cureType.Contains(type))
                {
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (conditionAmount[i] > MainManager.instance.playerdata[i].condition.Count)
                        {
                            int amountCleared = conditionAmount[i] - MainManager.instance.playerdata[i].condition.Count;

                            if (amountCleared > 0 && MainManager.instance.playerdata[i].hp > 0)
                            {
                                BattleControl_Ext.Instance.DoPurifyingPulseCheck(ref MainManager.instance.playerdata[i], amountCleared);
                                BattleControl_Ext.Instance.DoRevitalizingRippleCheck(ref MainManager.instance.playerdata[i], amountCleared);
                            }
                        }
                    }

                }
            }
        }
    }
    [HarmonyPatch(typeof(MainManager), "SetCondition", new Type[] { typeof(MainManager.BattleCondition), typeof(MainManager.BattleData), typeof(int), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal })]
    public class PatchMainManagerSetCondition
    {
        public static MainManager.BattleCondition[] StickyExceptions()
        {
            return new MainManager.BattleCondition[] {
                MainManager.BattleCondition.Eaten,
                MainManager.BattleCondition.Topple,
                MainManager.BattleCondition.EventStop,
                MainManager.BattleCondition.Flipped,
                MainManager.BattleCondition.Taunted,
                MainManager.BattleCondition.Reflection,
                MainManager.BattleCondition.Shield,
                (MainManager.BattleCondition)NewCondition.Vitiation,
                (MainManager.BattleCondition)NewCondition.Paintball,
                (MainManager.BattleCondition)NewCondition.Slugskin
            };
        }

        static bool Prefix(ref MainManager.BattleCondition condition, ref MainManager.BattleData entity, ref int turns, int fromplayer)
        {
            if (condition == MainManager.BattleCondition.Sturdy)
            {
                if (entity.delayedcondition != null)
                {
                    entity.delayedcondition.Clear();

                    if (entity.frostbitep != null)
                        UnityEngine.Object.Destroy(entity.frostbitep.gameObject);
                }
            }

            if (BattleControl_Ext.Instance.IsStatusImmune(entity, condition))
            {
                return false;
            }

            //if the bug has sticky any other condition gets buffed by 1 turn.
            if (MainManager.HasCondition(MainManager.BattleCondition.Sticky, entity) > -1 &&
                !StickyExceptions().Contains(condition))
            {
                int stickyBuff = 1 + MainManager.BadgeHowManyEquipped((int)Medal.SturdyStrands);
                if (turns > 0)
                    turns += stickyBuff;
                else
                    turns -= stickyBuff;
            }

            if (condition == MainManager.BattleCondition.Sticky && entity.animid == (int)NewEnemies.Mothmite)
            {
                MainManager.SetCondition((MainManager.BattleCondition)NewCondition.Slugskin, ref entity, Mathf.Max(1, turns / 2));
                return false;
            }

            if (condition != (MainManager.BattleCondition)NewCondition.Paintball &&
                MainManager.HasCondition(MainManager.BattleCondition.Inked, entity) > -1 &&
                MainManager.BadgeIsEquipped((int)Medal.InkBubble))
            {
                MainManager.SetCondition((MainManager.BattleCondition)NewCondition.Paintball, ref entity, 1);
            }

            return true;
        }

        static void Postfix(MainManager.BattleCondition condition, ref MainManager.BattleData entity, int turns, int fromplayer)
        {
            if (condition == MainManager.BattleCondition.Sleep)
            {
                Sleep.DoYawnCheck(entity, condition);
            }

            var entityExt = Entity_Ext.GetEntity_Ext(entity.battleentity);
            if (MainManager.HasCondition(MainManager.BattleCondition.Inked, entity) > -1 && !entityExt.inkDebuffed)
            {
                entityExt.CheckInkDebuff(ref entity);
                entityExt.permanentInkTriggered = false;
            }

            if ((int)condition == (int)NewCondition.Tiny || (int)condition == (int)NewCondition.Huge)
            {
                BattleControl_Ext.Instance.DoTinyHugeEffect(condition, ref entity, entityExt);
            }

            if ((int)condition == (int)NewCondition.Paintball && entity.battleentity != null)
            {
                entityExt.inkBubbleEnabled = true;
                entityExt.CreateInkBubble();
            }

            if ((int)condition == (int)NewCondition.Slugskin && entity.battleentity != null)
            {
                entityExt.slugskinActive = true;
                entityExt.CreateSlugskin();
            }

            if ((int)condition == (int)NewCondition.Vitiation && entity.battleentity != null)
            {
                entityExt.vitiation = true;
                entityExt.CreateVitiationShield();
            }

            if ((int)condition == (int)NewCondition.Dizzy 
                && MainManager.HasCondition((MainManager.BattleCondition)NewCondition.Dizzy, entity) > -1 
                && entity.battleentity != null)
            {
                entityExt.isDizzy = true;
            }

        }
    }

    [HarmonyPatch(typeof(MainManager), "CreateHUD")]
    public class PatchMainManagerCreateHUD
    {
        static void Postfix()
        {
            MainManager_Ext.Instance.CheckSwitcheroo();
        }
    }

    [HarmonyPatch(typeof(MainManager), "PlaySound", new Type[] { typeof(AudioClip), typeof(int), typeof(float), typeof(float), typeof(bool) })]
    public class PatchMainManagerPlaySound
    {
        static void Prefix(ref AudioClip soundclip)
        {
            MainManager_Ext.CheckGamerFX(ref soundclip);
        }
    }

    [HarmonyPatch(typeof(EntityControl), "PlaySound", new Type[] { typeof(AudioClip), typeof(float), typeof(float) })]
    public class PatchEntityControlPlaySound
    {
        static void Prefix(ref AudioClip clip)
        {
            MainManager_Ext.CheckGamerFX(ref clip);
        }
    }


    [HarmonyPatch(typeof(MainManager), "GetPlayerDataNullable")]
    public class PatchMainManagerGetPlayerDataNullable
    {
        static void Prefix(ref int id)
        {
            if (BattleControl_Ext.Instance.holoSkillID != -1)
            {
                id = MainManager.instance.playerdata[MainManager.battle.currentturn].trueid;
            }
        }
    }

    [HarmonyPatch(typeof(MainManager), "Update")]
    public class PatchMainManagerUpdate
    {
        static void Prefix()
        {
            //dirty fix for weird scale on text holder when big fable code
            if (MainManager.instance.message && MainManager.instance.flags[(int)NewCode.BIGFABLE])
            {
                if (MainManager.instance.textbox != null)
                {
                    MainManager.instance.textbox.transform.localScale = Vector3.one;
                }
            }
        }
    }


    [HarmonyPatch(typeof(MainManager), "ChangeMusic", new Type[] { typeof(string) })]
    public class PatchMainManagerChangeMusicCheck
    {
        static bool Prefix(string musicclip)
        {
            return MainManager_Ext.CheckNewMusic(musicclip, 0.1f, 0, false);
        }
    }

    [HarmonyPatch(typeof(MainManager), "ChangeMusic", new Type[] { typeof(string), typeof(float) })]
    public class PatchMainManagerChangeMusicCheck2
    {
        static bool Prefix(string musicclip, float fadespeed)
        {
            return MainManager_Ext.CheckNewMusic(musicclip, fadespeed, 0, false);
        }
    }

    [HarmonyPatch(typeof(MainManager), "ChangeMusic", new Type[] { typeof(string), typeof(float), typeof(int) })]
    public class PatchMainManagerChangeMusicCheck3
    {
        static bool Prefix(string musicclip, float fadespeed, int id)
        {
            return MainManager_Ext.CheckNewMusic(musicclip, fadespeed, id, false);
        }
    }

    [HarmonyPatch(typeof(MainManager), "ChangeMusic", new Type[] { typeof(string), typeof(float), typeof(int), typeof(bool) })]
    public class PatchMainManagerChangeMusicCheck4
    {
        static bool Prefix(string musicclip, float fadespeed, int id, bool seamless)
        {
            return MainManager_Ext.CheckNewMusic(musicclip, fadespeed, id, seamless);
        }
    }

    [HarmonyPatch(typeof(MainManager), "SamiraGotAll")]
    public class PatchMainManagerSamiraGotAll
    {
        static void Postfix(ref bool __result)
        {
            __result = MainManager.PurchasedMusicAmmount() >= Enum.GetNames(typeof(MainManager.Musics)).Length - 8 + Enum.GetNames(typeof(NewMusic)).Length - 4;
        }
    }

    [HarmonyPatch(typeof(MainManager), "RemoveCondition")]
    public class PatchMainManagerRemoveCondition
    {
        static bool Prefix(MainManager.BattleCondition condition, MainManager.BattleData entity)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.PermanentInk) && condition == MainManager.BattleCondition.Inked)
            {
                return false;
            }

            if (MainManager.BadgeIsEquipped((int)Medal.SturdyStrands) && condition == MainManager.BattleCondition.Sticky)
            {
                return false;
            }
            var entityExt = entity.battleentity == null ? null : Entity_Ext.GetEntity_Ext(entity.battleentity);
            if ((int)condition == (int)NewCondition.Tiny || (int)condition == (int)NewCondition.Huge && entity.battleentity != null)
            {
                BattleControl_Ext.Instance.ResetTinyHugeEffect(entity.battleentity, entityExt);
            }

            if ((int)condition == (int)NewCondition.Paintball && entity.battleentity != null)
            {
                entityExt.inkBubbleEnabled = false;
            }

            if ((int)condition == (int)NewCondition.Slugskin && entity.battleentity != null)
            {
                entityExt.slugskinActive = false;
            }

            if ((int)condition == (int)NewCondition.Vitiation && entity.battleentity != null)
            {
                entityExt.vitiation = false;
            }

            if ((int)condition == (int)NewCondition.Dizzy && entity.battleentity != null)
            {
                entityExt.isDizzy = false;
                entityExt.ResetDizzyAngle();
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MainManager), "CheckAchievement")]
    public class PatchMainManagerCheckAchievement
    {
        static void Prefix(MainManager __instance)
        {
            if (__instance.started)
            {
                if (!__instance.librarystuff[3, (int)NewAchievement.HDWGH] && __instance.flagvar[(int)NewFlagVar.MaxConditions] >= MainManager_Ext.HDWGH_CONDITIONS)
                {
                    MainManager.UpdateJounal(MainManager.Library.Logbook, (int)NewAchievement.HDWGH);
                }

                if (!__instance.librarystuff[3, (int)NewAchievement.OverKill] && __instance.flagvar[41] >= 100)
                {
                    MainManager.UpdateJounal(MainManager.Library.Logbook, (int)NewAchievement.OverKill);
                }

                if (!__instance.librarystuff[3, (int)NewAchievement.WellRested] && __instance.flags[913])
                {
                    MainManager.UpdateJounal(MainManager.Library.Logbook, (int)NewAchievement.WellRested);
                }

                if (!__instance.librarystuff[3, (int)NewAchievement.UndergroundExplorer] && __instance.flags[857])
                {
                    MainManager.UpdateJounal(MainManager.Library.Logbook, (int)NewAchievement.UndergroundExplorer);
                }
            }
        }
    }

    [HarmonyPatch(typeof(InputIO), "Achivement")]
    public class PatchInputIOAchivement
    {
        static bool Prefix(int id)
        {
            if (id < InputIO.achivements.Length)
            {
                return true;
            }
            return false;
        }
    }

    //force english
    [HarmonyPatch(typeof(InputIO), "LoadSettings")]
    public class PatchInputIOLoadSettings
    {
        static void Prefix()
        {
            MainManager.languageid = 0;
        }
    }


    [HarmonyPatch(typeof(MainManager), "ShowHUD")]
    public class PatchMainManagerShowHUD
    {
        static bool Prefix(bool show)
        {
            if (MainManager.instance.flags[916])
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MainManager), "UpdateArea")]
    public class PatchMainManagerUpdateArea
    {
        static bool Prefix(int newarea)
        {
            if (MainManager.instance.flags[916])
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MainManager), "ApplySettings")]
    public class PatchApplySettings
    {
        static void Postfix()
        {
            MainManager_Ext.Instance.ReloadSkillData();
        }
    }

    [HarmonyPatch(typeof(MainManager), "CheckIfCanExist")]
    public class PatchCheckIfCanExist
    {
        static bool Prefix(int[] requires, int[] limit, int regionalflag, ref bool __result)
        {
            __result = NewCheckIfCanExist(requires, limit, regionalflag);
            return false;
        }

        static bool NewCheckIfCanExist(int[] requires, int[] limit, int regionalflag)
        {
            if (CheckRequire(requires))
                return true;

            if (limit != null && limit.Length != 0)
            {
                bool hasPositiveLimitFlag = false;
                bool allLimitFlagsTrue = true;

                for (int i = 0; i < limit.Length; i++)
                {
                    int limitFlag = limit[i];

                    if (limitFlag < -1)
                    {
                        if (MainManager.instance.flags[Mathf.Abs(limitFlag)])
                            return true;
                    }
                    else if (limitFlag > -1)
                    {
                        hasPositiveLimitFlag = true;

                        if (!MainManager.instance.flags[limitFlag])
                        {
                            allLimitFlagsTrue = false;
                        }
                    }
                }

                if (hasPositiveLimitFlag && allLimitFlagsTrue)
                    return true;
            }
            return CheckRegionalFlags(regionalflag);
        }

        static bool CheckRequire(int[] requires)
        {
            return requires != null && requires.Length != 0 && requires[0] > -1
                && !MainManager.CheckAllBool(MainManager.instance.flags, requires, true);
        }

        static bool CheckRegionalFlags(int regionalflag)
        {
            return regionalflag > -1 && MainManager.map != null && MainManager.instance.regionalflags[regionalflag];
        }
    }
}


