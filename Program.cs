using DiscordLayer;
using Memester;

//permisions: 2150755328
Client.Targets = new List<ulong> {
        ulong.Parse("{{TargetsChannelsID}}"),
    };

var Server = await Connoisseur.Create();
var Piper  = await PipedPiper.Create(ulong.Parse("{{SourceChannelsID}}"));

Server.Start();
Piper.Start();

await Task.Delay(-1);

