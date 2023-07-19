using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;

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
}