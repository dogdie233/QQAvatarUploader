using System.Diagnostics.CodeAnalysis;

using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message.Entity;

using Net.Codecrete.QrCodeGenerator;

var done = false;
Guard(args.Length == 1, "请把图片拖到这个程序上");
var avatarFile = new FileInfo(args[0]);
Guard(avatarFile.Exists, "图片文件不存在");

var config = new BotConfig();
var deviceInfo = new BotDeviceInfo
{
    DeviceName = "QQAvatarUploader",
};
var keystore = new BotKeystore();
var bot = BotFactory.Create(config, deviceInfo, keystore);

bot.Invoker.OnBotOnlineEvent += OnBotOnline;
bot.Invoker.OnBotLogEvent += (_, e) =>
{
    if (done || e.Level < LogLevel.Information) return;
    Console.WriteLine($"  [Lgr][{e.EventTime}][{e.Level}][{e.Tag}] {e.EventMessage}");
};

var qrCode = await bot.FetchQrCode();
Guard(qrCode.HasValue, "获取二维码失败");
PrintQr(qrCode.Value.Url, true);

await bot.LoginByQrCode();


return;

async void OnBotOnline(BotContext botSender, BotOnlineEvent e)
{
    try
    {
        Console.WriteLine("登录成功");
        var uploadResult = await botSender.SetAvatar(new ImageEntity(avatarFile.FullName));
        Guard(uploadResult, "上传头像失败");
        Console.WriteLine("上传头像成功，可以关闭这个程序了");
    }
    catch
    {
        Guard(false, "头像上传失败了");
    }
}

static void Guard([DoesNotReturnIf(false)] bool pass, string message)
{
    if (pass) return;
    
    Console.WriteLine($"错误：{message}{Environment.NewLine}运行结束了，按回车键退出");
    Console.ReadLine();
    Environment.Exit(0);
    throw new Exception();
}

// This part of the code is from "https://github.com/eric2788/Lagrange.Core/blob/fd20a5aec81cacd56d60f3130cf057461300fd3f/Lagrange.OneBot/Utility/QrCodeHelper.cs#L30C52-L30C52"
// Thanks to "https://github.com/eric2788"
static void PrintQr(string text, bool compatibilityMode)
{
    var segments = QrSegment.MakeSegments(text);
    var qrCode = QrCode.EncodeSegments(segments, QrCode.Ecc.Low);

    const string emptyBlock = "  ";
    const string fullBlock = "██";

    Console.WriteLine(Environment.NewLine);
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine(Environment.NewLine);
    for (var y = 0; y < qrCode.Size; y++)
    {
        for (var x = 0; x < qrCode.Size; x++)
            Console.Write(qrCode.GetModule(x, y) ? fullBlock : emptyBlock);
        Console.Write("\n");
    }
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine(Environment.NewLine);

    if (compatibilityMode)
    {
        Console.WriteLine("Please scan this QR code from a distance with your smart phone.\nScanning may fail if you are too close.");
    }
}