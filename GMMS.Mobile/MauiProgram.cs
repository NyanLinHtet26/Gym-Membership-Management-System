using GMMS.Mobile.Configuration;
using GMMS.Mobile.Handlers;
using GMMS.Mobile.Pages;
using GMMS.Mobile.Services;
using GMMS.Mobile.Storage;
using GMMS.Mobile.ViewModels;
using Microsoft.Extensions.Logging;

namespace GMMS.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<TokenStorage>();

		builder.Services.AddTransient<AuthMessageHandler>();
		builder.Services.AddHttpClient<AuthApiService>(client =>
		{
			client.BaseAddress = new Uri(ApiSettings.BaseUrl);
			client.Timeout = ApiSettings.RequestTimeout;
		})
		.AddHttpMessageHandler<AuthMessageHandler>();

		builder.Services.AddHttpClient<MemberApiService>(client =>
		{
			client.BaseAddress = new Uri(ApiSettings.BaseUrl);
			client.Timeout = ApiSettings.RequestTimeout;
		})
		.AddHttpMessageHandler<AuthMessageHandler>();

		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<MemberViewModel>();

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<MemberListPage>();
		builder.Services.AddTransient<MemberDetailPage>();

		return builder.Build();
	}
}
