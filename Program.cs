using DiscordLayer;
using Memester;

static async Task setup() {
    try {
        var Server = await Connoisseur.Create();
        var Piper  = await PipedPiper.Create();

        Server.Start();
        Piper.Start();
    } catch (Exception e){
        System.Console.WriteLine(e.Message);
        await setup();
    }
}

await setup();
await Task.Delay(-1);

