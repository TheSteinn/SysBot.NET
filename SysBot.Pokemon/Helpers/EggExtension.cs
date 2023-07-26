using System;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon.Helpers;

public class EggExtension<T> where T : PKM
{
    public static IBattleTemplate? MapToTemplate(string species, out string msg, out ShowdownSet? set)
    {
        var lines = species.Split("\n");
        if (lines.Length is > 1 or 0 || species.Length == 0)
        {
            msg = "The provided text was invalid. Only provide the species name";
            set = null;
            return null;
        }

        species = species.Trim();
        var request = $"{species}\nLevel: 1\nShiny: Yes\n~=EggEncounter=true";
        set = new ShowdownSet(request);
        var template = AutoLegalityWrapper.GetTemplate(set);
        if (set.InvalidLines.Count != 0)
        {
            msg = $"Unable to parse text:\n{string.Join("\n", set.InvalidLines)}";
            return null;
        }

        msg = "SUCCESS";
        return template;
    }

    public static void ConvertToEgg(PKM pk, IBattleTemplate set)
    {
        pk.IsNicknamed = true;
        pk.Nickname = pk.Language switch
        {
            1 => "タマゴ",
            3 => "Œuf",
            4 => "Uovo",
            5 => "Ei",
            7 => "Huevo",
            8 => "알",
            9 or 10 => "蛋",
            _ => "Egg",
        };

        pk.IsEgg = true;
        pk.HeldItem = 0;
        pk.CurrentLevel = 1;
        pk.EXP = 0;
        pk.Met_Level = 1;
        pk.Met_Location = pk switch
        {
            // Met location for unhatched eggs is different for BDSP
            PB8 => 65535,
            _ => 0, // PK8 and PK9
        };
        pk.Egg_Location = pk switch
        {
            PB8 => 60010,
            PK9 => 30023,
            _ => 60002, //PK8
        };
        pk.CurrentHandler = 0;
        pk.OT_Friendship = 1;
        pk.HT_Name = "";
        pk.HT_Friendship = 0;
        pk.ClearMemories();
        pk.StatNature = pk.Nature;
        pk.SetEVs(new int[] { 0, 0, 0, 0, 0, 0 });
        
        pk.SetMarking(0, 0);
        pk.SetMarking(1, 0);
        pk.SetMarking(2, 0);
        pk.SetMarking(3, 0);
        pk.SetMarking(4, 0);
        pk.SetMarking(5, 0);
        
        pk.ClearInvalidMoves();
        pk.SetRelearnMoves(new Moveset());
        
        if (pk is PK8 pk8)
        {
            pk8.HT_Language = 0;
            pk8.HT_Gender = 0;
            pk8.HT_Memory = 0;
            pk8.HT_Feeling = 0;
            pk8.HT_Intensity = 0;
        }
        else if (pk is PB8 pb8)
        {
            pb8.HT_Language = 0;
            pb8.HT_Gender = 0;
            pb8.HT_Memory = 0;
            pb8.HT_Feeling = 0;
            pb8.HT_Intensity = 0;
        }
        else if (pk is PK9 pk9)
        {
            pk9.HT_Language = 0;
            pk9.HT_Gender = 0;
            pk9.HT_Memory = 0;
            pk9.HT_Feeling = 0;
            pk9.HT_Intensity = 0;
            pk9.Obedience_Level = 1;
            pk9.Version = 0;
            pk9.BattleVersion = 0;
            pk9.TeraTypeOverride = (MoveType)19;
        }

        var la = new LegalityAnalysis(pk);
        var enc = la.EncounterMatch;
        pk.CurrentFriendship = enc is EncounterStatic s ? s.EggCycles : pk.PersonalInfo.HatchCycles;
        
        Span<ushort> relearn = stackalloc ushort[4];
        la.GetSuggestedRelearnMoves(relearn, enc);
        pk.SetRelearnMoves(relearn);
        
        pk.SetSuggestedMoves();
        pk.Move1_PPUps = pk.Move2_PPUps = pk.Move3_PPUps = pk.Move4_PPUps = 0;
        pk.SetMaximumPPCurrent(pk.Moves);
        pk.SetSuggestedHyperTrainingData();
        pk.SetSuggestedRibbons(set, enc);
    }
}