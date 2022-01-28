using DSharpPlus;
using DiscordLayer;
using DSharpPlus.Entities;
using System;

namespace Memester;
public class PipedPiper : Outils.StartStop
{
    enum Commands {
        broadcast, fetch
    }
    private Client? client;
    private (ulong from, List<ulong> dest) pipes;
    private PipedPiper(ulong from, List<ulong> dest) 
        => this.pipes = (from, dest);

    public static async Task<PipedPiper> Create(ulong from , List<ulong>? dest = null) {
        var piper = new PipedPiper(from, dest ?? Client.Targets)
        {
            client = await Client.Create()
        };
        piper.client.Discord.MessageCreated += piper.Pipe;
        return piper;
    }
    private async Task Pipe(DiscordClient _, DSharpPlus.EventArgs.MessageCreateEventArgs e) {
        if(Enum.GetNames<Commands>().Select(c => $"-{c}").Any(prefix => e.Message.Content.StartsWith(prefix)))
            if (IsRunning && e.Channel.Id == pipes.from && client is not null) {
                var prefix = Enum.Parse<Commands>(e.Message.Content.Split(' ')[0][1..]);
                Func<Task> handler = prefix switch
                {
                    Commands.broadcast => async () =>
                    {
                        foreach (var id in pipes.dest.Where(chid => chid != pipes.from))
                        {
                            var channel = await client.GetChannelAsync(id);
                            await channel.SendMessageAsync(e.Message.Content[
                                (e.Message.Content.TakeWhile(c => c != ' ').Count() + 1)..]
                            );
                            if (e.Message.Attachments.Count > 0)
                            {
                                foreach (var emb in e.Message.Attachments)
                                {
                                    await channel.SendMessageAsync(emb.Url.ToString());
                                }
                            }
                        }
                    } ,
                    Commands.fetch => async () =>
                    {
                        var chunks = e.Message.Content.Split(' ')[1..].Chunk(3)
                            .Select(chunk => (chunk[0], Enum.Parse<Connoisseur.Mode>(chunk[1]), int.Parse(chunk[2])))
                            .ToList();
                        var server = await Connoisseur.Create(startImmediately: false);
                        await server.Serve(once: true, Custom: chunks);
                    } ,
                    _ => throw new NotImplementedException()
                };
                await handler();
            }
    } 
}
        