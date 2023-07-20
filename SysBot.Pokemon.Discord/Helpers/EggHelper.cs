using System;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

public static class EggHelper
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
        var request = $"{species}\nLevel: 1\n.IsEgg=true";
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
}