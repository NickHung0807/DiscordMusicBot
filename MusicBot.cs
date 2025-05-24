using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;

public class MusicBot
{
    private readonly DiscordSocketClient _client;
    private IAudioClient _audioClient; // 新增這行，記錄目前的語音連線
    private readonly string _token;

    public MusicBot(string token)
    {
        _client = new();
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildVoiceStates
        });
        _token = token;
    }

    public async Task StartAsync()
    {

        _client.Log += LogAsync;
        _client.MessageReceived += HandleCommandAsync;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        var user = message.Author as SocketGuildUser;
        if (user == null) return;

        if (message.Content == "?join")
        {
            if (user.VoiceChannel == null)
            {
                await message.Channel.SendMessageAsync("你不在任何語音頻道！");
                return;
            }
            try
            {
                var botUser = user.Guild.GetUser(_client.CurrentUser.Id);
                if (botUser.VoiceChannel != null)
                {
                    await message.Channel.SendMessageAsync("我已經在語音頻道了！");
                    return;
                }
                _audioClient = await user.VoiceChannel.ConnectAsync();
                await message.Channel.SendMessageAsync($"已加入語音頻道：{user.VoiceChannel.Name}");
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync("加入語音頻道時發生錯誤：" + ex.Message);
            }
        }
        else if (message.Content == "?leave")
        {
            var guild = user.Guild;
            var connectedChannel = guild.VoiceChannels
                .FirstOrDefault(vc => vc.ConnectedUsers.Any(u => u.Id == _client.CurrentUser.Id));
            if (connectedChannel != null)
            {
                await connectedChannel.DisconnectAsync();
                _audioClient = null;
                await message.Channel.SendMessageAsync("已離開語音頻道。");
            }
            else
            {
                await message.Channel.SendMessageAsync("我沒有在任何語音頻道中。");
            }
        }
        else if (message.Content.StartsWith("?play"))
        {
            if (_audioClient == null)
            {
                await message.Channel.SendMessageAsync("請先讓我加入語音頻道（?join）！");
                return;
            }
            var parts = message.Content.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                await message.Channel.SendMessageAsync("請提供音樂檔案的路徑！");
                return;
            }
            string input = parts[1].Trim('"');
            // var process = Process.Start("D:\\download_music_exe\\downloadmusic.exe", $"{input}");
            // if(process == null)
            // {
            //     await message.Channel.SendMessageAsync("無法啟動下載音樂的程式。");
            //     return;
            // }
            // await process.WaitForExitAsync();
            string filePath = @"D:\Code\DiscordMusicBot\music\audio.webm"; // 改成你的檔案路徑
            // await PlayLocalAudioAsync(_audioClient, filePath);
            await message.Channel.SendMessageAsync("正在播放本地音樂！");
        }
    }

    // private async Task PlayLocalAudioAsync(IAudioClient audioClient, string filePath)
    // {
    //     using (var ffmpeg = CreateStream(filePath))
    //     using (var output = ffmpeg.StandardOutput.BaseStream)
    //     using (var discord = audioClient.CreatePCMStream(AudioApplication.Music))
    //     {
    //         try
    //         {
    //             await output.CopyToAsync(discord);
    //         }
    //         finally
    //         {
    //             await discord.FlushAsync();
    //         }
    //     }
    // }

    // private Process CreateStream(string path)
    // {
    //     return Process.Start(new ProcessStartInfo
    //     {
    //         FileName = "ffmpeg",
    //         Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
    //         UseShellExecute = false,
    //         RedirectStandardOutput = true
    //     });
    // }
}