using DSharpPlus;
using DiscordLayer;
using DSharpPlus.Entities;
using System;
using System.Linq;

namespace Memester;
public class PipedPiper : Outils.StartStop
{
    enum Commands {
        broadcast, fetch, setup
    }
    private Client? client;
    private (ulong? from, List<ulong>? dest) pipes => (Client.Targets?.ElementAt(0), Client.Targets);
    private bool isInitialized => pipes.from is not null && pipes.dest is not null;
    public static async Task<PipedPiper> Create() {
        Console.Write("Pipe Hooks :");
        var piper = new PipedPiper
        {
            client = await Client.Create()
        };
        Client.Discord.MessageCreated += piper.Pipe;
        return piper;
    }
    private async Task Pipe(DiscordClient _, DSharpPlus.EventArgs.MessageCreateEventArgs e) {
        if(Enum.GetNames<Commands>().Select(c => $"-{c}").Any(prefix => e.Message.Content.StartsWith(prefix))) {
            var prefix = Enum.Parse<Commands>(e.Message.Content.Split(' ')[0][1..]);
            if (client is not null && (prefix == Commands.setup || (IsRunning && e.Channel.Id == pipes.from))) {
                Func<Task> handler = (prefix, isInitialized) switch
                {
                    (Commands.broadcast, true) => async () =>
                    {
                        foreach (var id in pipes.dest.Where(chid => chid != pipes.from))
                        {
                            var channel = await client.GetChannelAsync(id);
                            await channel.SendMessageAsync(e.Message.Content[
                                (e.Message.Content.TakeWhile(c => c != ' ').Count() + 1)..]
                            );
                            if (e.Message.Attachments.Count > 0)
                            {
                                foreach (var embed in e.Message.Attachments.Where(a => a.Url.EndsWith(".png") || a.Url.EndsWith(".jpg") || a.Url.EndsWith(".gif")))
                                {
                                    await channel.SendMessageAsync(embed.Url.ToString());
                                }
                            }
                        }
                    } ,
                    (Commands.fetch, true)  => async () =>
                    {
                        var chunks = e.Message.Content.Split(' ')[1..].Chunk(3)
                            .Select(chunk => (chunk[0], Enum.Parse<Connoisseur.Mode>(chunk[1]), int.Parse(chunk[2])))
                            .ToList();
                        var server = await Connoisseur.Create(startImmediately: false);
                        await server.Serve(once: true, Custom: chunks.Count > 0 ? chunks : null);
                    } ,
                    (Commands.setup, _)     => async () => 
                    {
                        var channels = e.Message.Content.Split(' ')[1..]
                            .Select(id => ulong.Parse(id))
                            .ToList();
                        if(channels.Count > 0 ) {
                            Client.Targets = channels;
                            var names = channels.Select(id => client.GetChannelAsync(id).Result)
                                                .Select(ch => ch.Name).ToList();
                            await e.Channel.SendMessageAsync($"Bot Setup Completed : Wired {names[0]} to {names.Aggregate("", (a, b) => $"{a} {b}")}");
                        }
                        else { 
                            await e.Channel.SendMessageAsync($"Please Provide Rooms Ids");
                            throw new Exception("Room Ids not provided");
                        }
                    },
                    (_, false) => async () => await e.Channel.SendMessageAsync($"Please Setup the bot before usage"),
                    _ => throw new Exception("Pattern not matched")
                };
                await handler();
            }
        }
    } 
}
        