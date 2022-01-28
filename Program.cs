using DiscordLayer;
using Memester;

Client.Targets = new List<ulong> {
        ulong.Parse("{{target_channel_id}}"),
    };

var Server = await Connoisseur.Create();
var Piper  = await PipedPiper.Create(ulong.Parse("{{source_channel_id}}"));

Server.Start();
Piper.Start();

await Task.Delay(-1);

