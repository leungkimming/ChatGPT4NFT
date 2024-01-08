using Microsoft.Extensions.Logging;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Windows.Services.Maps;
using Microsoft.Extensions.Configuration;
using ChatMAUI;


namespace MAUIBC;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.RegisterServices()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
            .Configuration.AddJsonStream(stream);

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder) {
		builder.Services
			.AddSingleton<INFTService, NFTService>()
			.AddSingleton<INFTPlugin, NFTPlugin>()
			.AddSingleton<IOpenAI, OpenAI>()
			.AddSingleton<MainPage>()
			.AddSingleton<MintNFTPage>()
			.AddSingleton<ReleaseBookingPage>()
			.AddSingleton<ReClaimBookingPage>()
			.AddSingleton<NFTListPage>()
			.AddSingleton<HouseKeepPage>()
			.AddSingleton<BlazorWebView>();
        return builder;
    }
}
