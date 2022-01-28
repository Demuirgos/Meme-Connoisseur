using DiscordLayer;
using Memester;

var Server = await Connoisseur.Create();
var Piper  = await PipedPiper.Create();

Server.Start();
Piper.Start();

await Task.Delay(-1);

