using DiscordLayer;
using Memester;

//permisions: 2150755328
Client.Targets = new List<ulong>{
    ulong.Parse("936404347204542494"),
    ulong.Parse("936404347204542494")
};

var Server = await Connoisseur.Create();
var Piper  = await PipedPiper.Create(ulong.Parse("{{RootChannelID}}"));

Server.Start();
Piper.Start();

await Task.Delay(-1);

