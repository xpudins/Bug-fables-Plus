using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using static MainManager;

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{
    public class PatchAreaBattleMusic : PatchBaseStartBattle
    {
        public PatchAreaBattleMusic()
        {
            priority = 1463;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, j => j.MatchLdfld(AccessTools.Field(typeof(MainManager), "areaid")), j => j.MatchStloc(out _));

            int cursorIndex = cursor.Index;
            ILLabel label = null;
            cursor.GotoPrev(i => i.MatchBr(out label));
            cursor.Goto(cursorIndex);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchAreaBattleMusic), "CheckNewAreaMusic"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool CheckNewAreaMusic()
        {
            string fightRemix = "";
            string tigsRemix = "";

            switch (MainManager.instance.areaid)
            {
                case (int)MainManager.Areas.RubberPrison:
                    fightRemix = NewMusic.BattleRubberPrison.ToString();
                    tigsRemix = NewMusic.TigsPrison.ToString();
                    break;

                case (int)MainManager.Areas.GoldenHills:
                case (int)MainManager.Areas.GoldenWay:
                case (int)MainManager.Areas.GoldenSettlement:
                    tigsRemix = NewMusic.TigsHills.ToString();
                    fightRemix = NewMusic.BattleGoldenHills.ToString();
                    break;

                case (int)MainManager.Areas.Desert:
                case (int)MainManager.Areas.DefiantRoot:
                case (int)MainManager.Areas.BanditHideout:
                    fightRemix = NewMusic.BattleLostSands.ToString();
                    tigsRemix = NewMusic.TigsDesert.ToString();
                    break;

                case (int)MainManager.Areas.HoneyFactory:
                    fightRemix = NewMusic.BattleFactory.ToString();
                    tigsRemix = NewMusic.TigsFactory.ToString();
                    break;

                case (int)MainManager.Areas.BugariaOutskirts:
                    fightRemix = NewMusic.BattleOutskirts.ToString();
                    tigsRemix = NewMusic.TigsOutskirts.ToString();
                    break;

                case (int)MainManager.Areas.MetalLake:
                    fightRemix = NewMusic.BattleMetalLake.ToString();
                    tigsRemix = NewMusic.TigsLake.ToString();
                    break;

                case (int)MainManager.Areas.BarrenLands:
                    fightRemix = NewMusic.BattleForsakenLands.ToString();
                    tigsRemix = NewMusic.TigsForsaken.ToString();
                    break;

                case (int)MainManager.Areas.Snakemouth:
                case (int)MainManager.Areas.ChomperCaves:
                case (int)MainManager.Areas.StreamMountain:
                    fightRemix = NewMusic.BattleCaves.ToString();
                    tigsRemix = NewMusic.TigsCave.ToString();
                    break;

                case (int)MainManager.Areas.SandCastle:
                    fightRemix = NewMusic.BattleSandCastle.ToString();
                    tigsRemix = NewMusic.TigsCastle.ToString();
                    break;

                case (int)MainManager.Areas.FarGrasslands:
                    fightRemix = NewMusic.BattleFarGrasslands.ToString();
                    tigsRemix = NewMusic.TigsGrasslands.ToString();
                    break;

                case (int)MainManager.Areas.WildGrasslands:
                    fightRemix = NewMusic.BattleSwamplands.ToString();
                    tigsRemix = NewMusic.TigsSwamp.ToString();
                    break;

                case (int)MainManager.Areas.UpperSnakemouth:
                    fightRemix = NewMusic.BattleSnakemouthLab.ToString();
                    tigsRemix = NewMusic.TigsLab.ToString();
                    break;
            }


            int newAreaId = MainManager_Ext.GetNewAreaId((int)MainManager.map.mapid);
            if (newAreaId > -1)
            {
                switch (newAreaId)
                {
                    //powerPlant
                    case 1:
                        fightRemix = NewMusic.BattleFactory.ToString();
                        tigsRemix = NewMusic.TigsFactory.ToString();
                        break;

                    // irontower
                    case 2:
                        fightRemix = NewMusic.BattleForsakenLands.ToString();
                        tigsRemix = NewMusic.TigsForsaken.ToString();
                        break;

                    //leafbug village
                    case 6:
                        fightRemix = NewMusic.BattleSwamplands.ToString();
                        tigsRemix = NewMusic.TigsSwamp.ToString();
                        break;

                    // playroom
                    case 7:
                        fightRemix = NewMusic.BattleRubberPrison.ToString();
                        tigsRemix = NewMusic.TigsPrison.ToString();
                        break;
                }
            }

            if (MainManager.map.mapid == Maps.GoldenPathTunnel || MainManager.map.mapid == Maps.GoldenPathTunnel2)
            {
                fightRemix = NewMusic.BattleCaves.ToString();
                tigsRemix = NewMusic.TigsCave.ToString();
            }

            if (fightRemix != "" && MainManager_Ext.musicOption != MusicSetting.Off)
            {
                if(MainManager_Ext.musicOption == MusicSetting.Mix)
                {
                    Areas[] tigsAreas = { 
                        Areas.BarrenLands, Areas.FarGrasslands, Areas.WildGrasslands, 
                        Areas.RubberPrison, Areas.MetalLake, Areas.StreamMountain    
                    };

                    if(MainManager.instance.areaid == (int)MainManager.Areas.UpperSnakemouth || newAreaId == 1 || newAreaId == 5 || newAreaId == 7
                        || (MainManager.instance.flags[348] && tigsAreas.Contains((Areas)instance.areaid)))
                        MainManager.ChangeMusic(tigsRemix, 1f);
                    else
                        MainManager.ChangeMusic(fightRemix, 1f);
                }
                else if(MainManager_Ext.musicOption == MusicSetting.OnlyFight)
                    MainManager.ChangeMusic(fightRemix, 1f);
                else
                    MainManager.ChangeMusic(tigsRemix, 1f);
                return true;
            }

            //i forgor why i put that here, might be dumb or a prophet who knows
            if (MainManager.instance.areaid == (int)MainManager.Areas.GiantLair)
            {
                MainManager.ChangeMusic("Battle6", 1f);
                return true;
            }

            return false;
        }
    }
}
