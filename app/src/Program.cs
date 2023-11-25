using System;
using System.IO;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace app;

class Program
{
    internal const bool IsDebug = true;
  
    static string GetToken(string TokenType) {
        return File.ReadAllText($"{TokenType.ToUpper()}.SECRET");
    }

    static async Task Main(string[] args)
    {
        var myBot = new TwitchBot(nick: "TheNoviceBot", auth: GetToken("twitch"), channel: "thenoviceprospect");
        myBot.Start().SafeFireAndForget();
        await myBot.JoinChannel();
        await myBot.SendMessage($"'{myBot.mySettings.username}' has joined {myBot.mySettings.channelname} ");

        myBot.OnMessage += async (sender, twitchChatMessage) => {
            Console.WriteLine($"{twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");
            if (twitchChatMessage.Message.StartsWith("!test")) {
                await myBot.SendMessage($"Hey there {twitchChatMessage.Sender}");
            }
            if (twitchChatMessage.Message.StartsWith("!stop")) {
                await myBot.SendMessage($"'{myBot.mySettings.username}' has left {myBot.mySettings.channelname} ");
                Environment.Exit(0);
            }
        };
        await Task.Delay(-1);
    }
}
