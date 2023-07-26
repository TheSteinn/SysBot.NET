using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.Discord;

[Summary("Extra/custom trade functionality")]
public class TradeAdditions<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    // Giveaway functionality borrowed from Zyro's NotForkBot with some refactoring to remove redundant code
    [Command("giveawaypool")]
    [Alias("gap")]
    [Summary("Show a list of Pokémon available for giveaway.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task DisplayGiveawayPoolCountAsync()
    {
        var pool = Info.Hub.Ledy.Pool;
        if (pool.Count > 0)
        {
            var test = pool.Files;
            var lines = pool.Files.Select((z, i) => $"{i + 1}: {z.Key} = {(Species)z.Value.RequestInfo.Species}");
            var msg = string.Join("\n", lines);
            await ReplyAsync("Giveaway Pool Details:\n" + msg).ConfigureAwait(false);
        }
        else await ReplyAsync("Giveaway pool is empty.").ConfigureAwait(false);
    }

    [Command("giveaway")]
    [Alias("ga", "preset", "ps")]
    [Summary("Makes the bot trade you the specified giveaway Pokémon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task TradePresetAsync([Summary("Trade Code")] int code, [Remainder] string content)
    {
        T pk;
        content = ReusableActions.StripCodeBlock(content);
        var pool = Info.Hub.Ledy.Pool;

        if (pool.Count == 0)
        {
            await ReplyAsync("There are no available giveaways to choose from").ConfigureAwait(false);
            return;
        }

        if (Info.Hub.Ledy.Distribution.TryGetValue(content, out var val))
            pk = val.RequestInfo;
        else
        {
            await ReplyAsync(
                    $"The requested giveaway Pokémon not available, use \"{Info.Hub.Config.Discord.CommandPrefix}giveawaypool\" for the full list of available giveaways!")
                .ConfigureAwait(false);
            return;
        }

        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade,
            PokeTradeType.Specific).ConfigureAwait(false);
    }

    [Command("giveaway")]
    [Alias("ga", "preset", "ps")]
    [Summary("Makes the bot trade you the specified giveaway Pokémon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task TradePresetAsync([Remainder] string content)
    {
        var code = Info.GetRandomTradeCode();
        await TradePresetAsync(code, content).ConfigureAwait(false);
    }

    [Command("egg")]
    [Summary("Makes the bot trade you an egg for the requested Pokémon")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task RequestEggAsync([Remainder] string content)
    {
        var code = Info.GetRandomTradeCode();
        await RequestEggAsync(code, content).ConfigureAwait(false);
    }

    [Command("egg")]
    [Summary("Makes the bot trade you an egg for the requested Pokémon")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task RequestEggAsync([Summary("Trade Code")] int code,
        [Summary("The Pokémon species you want")] [Remainder]
        string content)
    {
        if (typeof(T) == typeof(PA8))
        {
            // Bail early for PLA
            await ReplyAsync("Cannot obtain eggs in Legends: Arceus").ConfigureAwait(false);
            return;
        }

        var template = EggExtension<T>.MapToTemplate(content, out var templResult, out var set);
        if (!"SUCCESS".Equals(templResult))
        {
            await ReplyAsync(templResult).ConfigureAwait(false);
            return;
        }

        if (template == null || set == null)
        {
            await ReplyAsync("Unable to convert the request to a viable template for an Egg").ConfigureAwait(false);
            return;
        }

        try
        {
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pkm = sav.GetLegal(template, out var result);
            EggExtension<T>.ConvertToEgg(pkm, template);

            var la = new LegalityAnalysis(pkm);
            var spec = GameInfo.Strings.Species[template.Species];
            pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
            if (pkm is not T pk || !la.Valid)
            {
                var reason = result switch
                {
                    "Timeout" => $"That {spec} set took too long to generate.",
                    "VersionMismatch" => "Request refused: PKHeX and Auto-Legality Mod version mismatch.",
                    _ => $"I wasn't able to create a {spec} egg from that."
                };
                var imsg = $"Oops! {reason}";
                if (result == "Failed")
                    imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";
                await ReplyAsync(imsg).ConfigureAwait(false);
                return;
            }

            pk.ResetPartyStats();

            if (!pk.CanBeTraded())
            {
                await ReplyAsync("Provided Pokémon content is blocked from trading!").ConfigureAwait(false);
                return;
            }

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk,
                PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(TradeAdditions<T>));
            var msg =
                $"Oops! An unexpected problem happened with this Set:\n```{string.Join("\n", set.GetSetLines())}```";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
    }
}