using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DumbQQ.Client;
using HttpResponse = System.Net.Http.HttpResponseMessage;


namespace DumbQQCoreConsoleApp
{
    public class Program
    {
        private static readonly DumbQQClient Client = new DumbQQClient { CacheTimeout = TimeSpan.FromDays(1) };
        private const string CookiePath = "dump.json";

        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 好友消息回调
            Client.FriendMessageReceived += (sender, message) =>
            {
                var s = message.Sender;
                Console.WriteLine($"{s.Alias ?? s.Nickname}:{message.Content}");
            };
            // 群消息回调
            Client.GroupMessageReceived += (sender, message) =>
            {
                var s = message.Sender;
                Console.WriteLine($"[{message.Group.Name}]{s.Alias ?? s.Nickname}:{message.Content}");
            };
            // 讨论组消息回调
            Client.DiscussionMessageReceived += (sender, message) =>
            {
                Console.WriteLine($"[{message.Discussion.Name}]{message.Sender.Nickname}:{message.Content}");
            };
            // 消息回显
            Client.MessageEcho += (sender, e) =>
            {
                Console.WriteLine($"{e.Target.Name}>{e.Content}");
            };
            if (File.Exists(CookiePath))
            {
                // 尝试使用cookie登录
                if (Client.Start(File.ReadAllText(CookiePath)) != DumbQQClient.LoginResult.Succeeded)
                {
                    // 登录失败，退回二维码登录
                    QrLogin();
                }
            }
            else
            {
                QrLogin();
            }
            Console.WriteLine($"Login Success，{Client.Nickname}!");
            // 导出cookie
            try
            {
                File.WriteAllText(CookiePath, Client.DumpCookies());
            }
            catch
            {
                // Ignored
            }
            // 防止程序终止
            Console.ReadLine();
        }

        private static void QrLogin()
        {
            while (true)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Client.Start(path => { }))
                {
                    case DumbQQClient.LoginResult.Succeeded:
                        return;
                    case DumbQQClient.LoginResult.QrCodeExpired:
                        continue;
                    default:
                        Console.WriteLine("登录失败，需要重试吗？(y/n)");
                        var response = Console.ReadLine();
                        if (response.Contains("y"))
                        {
                            continue;
                        }
                        Environment.Exit(1);
                        return;
                }
            }
        }
    }
}