using System.Dynamic;
using System.Threading.Channels;
using DSharpPlus;

namespace DiscordLayer;
public class Client {
    private readonly string? tokenkey = Environment.GetEnvironmentVariable("$DiscordTokenApp0$");
    public static List<ulong>? Targets {get; set;}
    public DiscordClient? Discord {get; set;} = null;
    private Client(List<ulong> channelIds) {
        Discord = new(new DiscordConfiguration() {
                        Token = tokenkey,
                        TokenType = TokenType.Bot,
                        Intents = DiscordIntents.AllUnprivileged     
                    });
        
        Targets?.AddRange(channelIds);
    }
    public static async Task<Client> Create(List<ulong>? channelIds = null) {
        try
        {
            var client = new Client(channelIds ?? new List<ulong>());
            DiscordClient? discord = client.Discord;
            if(discord is not null){
                await discord.ConnectAsync();
                Console.WriteLine("Connection Completed");
                return client;
            }
            else throw new Exception("Discord Client is null");
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