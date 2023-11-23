﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace app;

internal class TwitchBot {
    internal struct Settings {
        internal string ip;
        internal int port;
        internal string password;
        internal string username;
        internal string channelname;
        public Settings()
        {
            ip = String.Empty;
            port = 0;
            password = String.Empty;
            username = String.Empty;
            channelname = String.Empty;
        }
    }
    internal Settings mySettings = new Settings();
    private StreamReader sReader;
    private StreamWriter sWriter;

    private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();

    public event TwitchChatEventHandler OnMessage = delegate { };
    public delegate void TwitchChatEventHandler(object sender, TwitchChatMessage e);

    public class TwitchChatMessage : EventArgs
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public string Channel { get; set; }
        }

    public TwitchBot(string nick, string auth)
    {
        mySettings.ip = "irc.chat.twitch.tv";
        mySettings.port = 6667;
        mySettings.username = nick;
        mySettings.password = auth;
    }

    public async Task Start() {
        var chatClient = new TcpClient();
        await chatClient.ConnectAsync(this.mySettings.ip, this.mySettings.port);
        sReader = new StreamReader(chatClient.GetStream());
        sWriter = new StreamWriter(chatClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };
        //NewLine = "\r\n" automatically puts \r\n after every line which marks the end of a message
        //AutoFlush = true will call streamWriter.Flush(); after every write call
        //So instead of doing
        //await streamWriter.WriteLineAsync($"PASS {password}\r\n");
        //await streamWriter.FlushAsync();
        //
        //All we need now is
        ////await streamWriter.WriteLineAsync($"PASS {password}");
        await sWriter.WriteLineAsync($"PASS {mySettings.password}");
        await sWriter.WriteLineAsync($"NICK {mySettings.username}");
        connected.SetResult(0);

                while (true) {
            string line = await sReader.ReadLineAsync();
            Console.WriteLine(line);

            string[] split = line.Split(" ");

            //PING :tmi.twitch.tv
            //Respond with PONG :tmi.twitch.tv
            if (line.StartsWith("PING")) await sWriter.WriteLineAsync($"PONG {split[1]}");
            
            if (split.Length > 2 && split[1] == "PRIVMSG")
                {
                    //:mytwitchchannel!mytwitchchannel@mytwitchchannel.tmi.twitch.tv 
                    // ^^^^^^^^
                    //Grab this name here
                    int exclamationPointPosition = split[0].IndexOf("!");
                    string username = split[0].Substring(1, exclamationPointPosition - 1);
                    //Skip the first character, the first colon, then find the next colon
                    int secondColonPosition = line.IndexOf(':', 1);//the 1 here is what skips the first character
                    string message = line.Substring(secondColonPosition + 1);//Everything past the second colon
                    string channel = split[2].TrimStart('#');
                    
                    OnMessage(this, new TwitchChatMessage
                    {
                        Message = message,
                        Sender = username,
                        Channel = channel
                    });
                }
        }

    }

    internal async Task SendMessage(string message) {
        await connected.Task;
        await sWriter.WriteLineAsync($"PRIVMSG #{this.mySettings.channelname} :{message}");
    }

    internal async Task JoinChannel() {
        await connected.Task;
        await sWriter.WriteLineAsync($"JOIN #{this.mySettings.channelname}");
    }

}

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