using DSharpPlus;
using DiscordLayer;
using DSharpPlus.Entities;
using System;

namespace Memester;
public class PipedPiper
{
    enum Commands {
        broadcast, fetch
    }
    private Client client;
    private bool isRunning = false;
    private (ulong from, List<ulong> dest) pipes;
    public void Start() => isRunning = true;
    public void Stop() => isRunning = false;
    private PipedPiper(ulong from, List<ulong> dest) 
        => this.pipes = (from, dest);
    public static async Task<PipedPiper> Create(ulong from , List<ulong> dest = null) {
        var piper = new PipedPiper(from, dest ?? Client.Targets)
        {
            client = await Client.Create()
        };
        piper.client.Discord.MessageCreated += piper.Pipe;
        return piper;
    }
    private async Task Pipe(DiscordClient _, DSharpPlus.EventArgs.MessageCreateEventArgs e) {
        if(Enum.GetNames<Commands>().Select(c => $"-{c}").Any(prefix => e.Message.Content.StartsWith(prefix)))
            if (isRunning && e.Channel.Id == pipes.from) {
                var prefix = Enum.Parse<Commands>(e.Message.Content.Split(' ')[0][1..]);
                Func<Task> handler = prefix switch
                {
                    Commands.broadcast => async () =>
                    {
                        foreach (var id in pipes.dest.Where(chid => chid != pipes.from))
                        {
                            var channel = await client.GetChannelAsync(id);
                            await channel.SendMessageAsync(e.Message.Content[
                                (e.Message.Content.SkipWhile(c => c != ' ').Count() + 1)..]
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
                        var server = await Connoisseur.Create(startImmediately: false);
                        await server.Serve(once: true);
                    } ,
                    _ => throw new NotImplementedException()
                };
                await handler();
            }
    } 
}
        