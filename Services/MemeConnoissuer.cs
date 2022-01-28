using System.Collections;
using Newtonsoft.Json;
using DiscordLayer;
using DSharpPlus;
namespace Memester;
public class Connoisseur
{
    public enum Mode {
        Top, Hot, New
    }
    private (HttpClient http, Client discord) clients;
    public static async Task<Connoisseur> Create(){
        var server = new Connoisseur();
        server.clients = (new HttpClient(), await Client.Create());
        new Thread(async () => await server.Serve()).Start();
        return server;
    }
    PeriodicTimer timer = new(TimeSpan.FromHours(24));
    private List<(string Subreddit, Mode Mode, int Count)> Query = new() {
        ("dankmemes", Mode.Hot, 23), ("me_irl", Mode.Hot, 23)
    };
    private List<string> urls => 
        Query.Select(q => $"https://www.reddit.com/r/{q.Subreddit}/{q.Mode.ToString().ToLower()}/.json?limit={q.Count}").ToList();

    private bool isMeme(string url) => url.StartsWith("https://i.redd.it");

    public async Task<List<string>> GetPostsAsync(){
        var Total = new List<string>();
        foreach (var url in urls)
        {
            var response = await clients.http.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            dynamic dict = JsonConvert.DeserializeObject(content);
            var posts =
                    (from post in ((IEnumerable)dict.data.children).Cast<dynamic>()
                    let source = post.data.url
                    where isMeme((string)source)
                    select (string)post.data.url).ToList();
            Total.AddRange(posts);
        }
        return Total;
    } 
    private bool isRunning = false;
    public void Start() => isRunning = true;
    public void Stop() => isRunning = false;
    public async Task Serve(){
        while (await timer.WaitForNextTickAsync())
        {
            if(isRunning){
                var posts = await GetPostsAsync();
                posts.ForEach(post => System.Console.WriteLine(post));
                Client.Targets.ForEach(async id => {
                    var channel = await clients.discord.GetChannelAsync(id);
                    posts.ForEach(async p => await channel.SendMessageAsync(p));
                });
            }
        }
    }
}
        