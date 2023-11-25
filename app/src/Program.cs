using System;
using System.IO;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Microsoft.VisualBasic;

namespace app;

class Program
{

    private enum BotCommands
    {
        Null,
        Message,
        TwitchCommand,
        Stop
    }

    private enum TwitchCommands {
        Null,
        Clear,
        MeSay,
        Mod,
        Whisper
    }
    internal const bool IsDebug = true;

    /// <summary>
    /// This will parse incoming messages for certain keywords/commands and return a message string
    /// </summary>
    /// <param name="myMessage">This is the entire incoming irc msg</param>
    /// <param name="myBot">handle to our bot-instance</param>
    /// <param name="botCommand">this sets which internal "command" we are using</param>
    /// <param name="twitchCommand">this is set in-case we are triggering twitch commands (such as /help)</param>
    /// <returns>A fully "formatted" message to send into chat</returns>
    static string handleMessage(TwitchBot.TwitchChatMessage myMessage, ref TwitchBot myBot, out BotCommands botCommand, out TwitchCommands twitchCommand) {
        string msg = String.Empty;
        botCommand = BotCommands.Null;
        twitchCommand = TwitchCommands.Null;
        Console.WriteLine($"{myMessage.Sender} said '{myMessage.Message}'");
        if (myMessage.Message.StartsWith("!test")) {
            msg = $"Hey there {myMessage.Sender}";
            botCommand = BotCommands.Message;
            twitchCommand = TwitchCommands.Null;
        }
        if (myMessage.Message.StartsWith("!stop")) {
            msg = $"'{myBot.mySettings.username}' has left {myBot.mySettings.channelname} ";
            botCommand = BotCommands.Stop;
            twitchCommand = TwitchCommands.Null;
        }
        if (myMessage.Message.StartsWith("!clear")) {
            msg = $"'{myBot.mySettings.username}' is absolving chat of all sins :D";
            botCommand = BotCommands.TwitchCommand;
            twitchCommand = TwitchCommands.Clear;
        }
        return msg;
    }

    /// <summary>
    /// Here we handle twitch commands, parse the message and return a fully formed twitch command.
    /// For example !clear should send /clear but !mod <user> will convert this to a /mod (or /unmod) plus the user id
    /// </summary>
    /// <param name="myCommand">which twitch command was triggered</param>
    /// <returns>a fully formed slash-command to send to twitch.</returns>
    static string handleTwitchCommand(TwitchCommands myCommand) {
        switch (myCommand)
        {
            case TwitchCommands.Null:
                break;
            case TwitchCommands.Clear:
                return "/clear";
            default:
                break;
        }
        return String.Empty;
    }

    /// <summary>
    /// Read a local token file and return its content as string
    /// </summary>
    /// <param name="TokenType"></param>
    /// <returns></returns>
    static string GetToken(string TokenType) {
        return File.ReadAllText($"{TokenType.ToUpper()}.SECRET");
    }

    static async Task Main(string[] args)
    {
        // Make sure all commands are set to "nothing"
        BotCommands currentCommand = BotCommands.Null;
        TwitchCommands currentTwitchCommand = TwitchCommands.Null;
        // Initialize the bot with a "nickname", token and channel to connect to
        var tnpBot = new TwitchBot(nick: "TheNoviceBot", auth: GetToken("twitch"), channel: "thenoviceprospect");
        // This helps us run start() as async task until we are connected.
        tnpBot.Start().SafeFireAndForget();
        // once connected try to join the channel
        await tnpBot.JoinChannel();
        // announce that we joined.
        await tnpBot.SendMessage($"'{tnpBot.mySettings.username}' has joined {tnpBot.mySettings.channelname} ");
        // our message handler
        tnpBot.OnMessage += async (sender, twitchChatMessage) => {
            var result = handleMessage(twitchChatMessage, ref tnpBot, out currentCommand, out currentTwitchCommand);
            switch (currentCommand)
            {
                case BotCommands.Null:
                    break;
                case BotCommands.Message:
                    await tnpBot.SendMessage(result);
                    break;
                case BotCommands.TwitchCommand:
                    await tnpBot.SendMessage(result);
                    await tnpBot.SendMessage(handleTwitchCommand(currentTwitchCommand));
                    break;
                case BotCommands.Stop:
                    await tnpBot.SendMessage(result);
                    Environment.Exit(0);
                    break; // <-- theoretically this will never be reached but the compiler insists on it.
                default:
                    break;
            }
        };
        await Task.Delay(-1);
    }
}
