using System.Dynamic;
using System.Threading.Channels;
using DSharpPlus;

namespace DiscordLayer;
public class Client {
    private string tokenkey = Environment.GetEnvironmentVariable("$DiscordTokenApp$");
    public static List<ulong> Targets {get; set;} = new(){
        ulong.Parse("{{ChannelsIDs}}")
    };
    public DiscordClient Discord {get; set;} = null;
    private Client(List<ulong> channelIds) {
        Discord = new(new DiscordConfiguration() {
                        Token = tokenkey,
                        TokenType = TokenType.Bot,
                        Intents = DiscordIntents.AllUnprivileged     
                    });
        
        Targets.AddRange(channelIds);
    }
    public static async Task<Client> Create(List<ulong> channelIds = null) {
        try
        {
            var client = new Client(channelIds ?? new List<ulong>());
            await client.Discord.ConnectAsync();
            Console.WriteLine("Connection Completed");
            return client;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection Failed : {e.Message}");
            throw;
        }
    }
    public async Task<DSharpPlus.Entities.DiscordChannel> GetChannelAsync(ulong id) => await Discord.GetChannelAsync(id);
}