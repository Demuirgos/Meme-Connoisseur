using System.Collections;
using Newtonsoft.Json;
using DiscordLayer;
using DSharpPlus;
using System.Reflection;

namespace Memester;
public class Connoisseur : Outils.StartStop
{
    public enum Mode {
        Top, Hot, New
    }
    private (HttpClient http, Client discord) clients;
    public static async Task<Connoisseur> Create(bool startImmediately = true) {
        var server = new Connoisseur();
        server.clients = (new HttpClient(), await Client.Create());
        if(startImmediately)
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
    public async Task Serve(bool once = false){
        var Body = async () => {
            if(IsRunning || once){
                var posts = await GetPostsAsync();
                posts.ForEach(post => System.Console.WriteLine(post));
                Client.Targets.ForEach(async id => {
                    var channel = await clients.discord.GetChannelAsync(id);
                    posts.ForEach(async p => await channel.SendMessageAsync(p));
                });
            }
        };

        if(once)
            await Body();
        else while (await timer.WaitForNextTickAsync())
        {
            await Body();
        }
    }
}
        