using System.Dynamic;
using System.Threading.Channels;
using DSharpPlus;

namespace DiscordLayer;
public class Client {
    private static readonly string? tokenkey = "{{DiscordToken}}";
    public static List<ulong>? Targets {get; set;}
    private static bool IsConnected = false;
    private static DiscordClient? discord {get; set;} = null;
    public static DiscordClient? Discord {
        get {
            if (discord is null)
                discord =  new(new DiscordConfiguration() {
                                Token = tokenkey,
                                TokenType = TokenType.Bot,
                                Intents = DiscordIntents.AllUnprivileged     
                            });
            return discord;
        }
    }
    private Client(List<ulong> channelIds) {
        Targets?.AddRange(channelIds);
    }
    public static async Task<Client> Create(List<ulong>? channelIds = null) {
        try
        {
            var client = new Client(channelIds ?? new List<ulong>());
            if(!IsConnected){
                await Discord.ConnectAsync();
                IsConnected  = true;
            }
            Console.WriteLine("Connection Completed");
            return client;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection Failed : {e.Message}");
            throw;
        }
    }
    public async Task<DSharpPlus.Entities.DiscordChannel?> GetChannelAsync(ulong id) 
        =>  Discord is not null ? await Discord.GetChannelAsync(id) : null;
}