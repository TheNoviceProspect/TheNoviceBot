using System;
using System.IO;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace app;

class Program
{
    private const bool IsDebug = true;
  
    static string GetToken(string TokenType) {
        return File.ReadAllText($"{TokenType.ToUpper()}.SECRET");
    }

    static async Task Main(string[] args)
    {
        var myBot = new TwitchBot("TheNoviceBot",GetToken("twitch"));
        myBot.Start().SafeFireAndForget();
        await myBot.JoinChannel();
        await myBot.SendMessage($"'{myBot.mySettings.username}' has joined {myBot.mySettings.channelname} ");

        myBot.OnMessage += async (sender, twitchChatMessage) => {
            Console.WriteLine($"{twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");
            if (twitchChatMessage.Message.StartsWith("!test")) {
                await myBot.SendMessage($"Hey there {twitchChatMessage.Sender}");
            }
        };
        await Task.Delay(-1);
    }
}
