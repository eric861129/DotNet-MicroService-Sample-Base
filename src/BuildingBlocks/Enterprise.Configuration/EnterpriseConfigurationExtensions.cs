using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Enterprise.Configuration;

public static class EnterpriseConfigurationExtensions
{
    public static WebApplicationBuilder AddEnterpriseConfiguration(this WebApplicationBuilder builder)
    {
        var endpoint = builder.Configuration["Azure:AppConfiguration:Endpoint"];

        // 沒有設定 Endpoint 時，就代表現在走本機模式。
        // 這時候直接用 appsettings 與環境變數即可。
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return builder;
        }

        // 一旦有 Azure App Configuration，就把集中設定、refresh 與 Key Vault 一起接上。
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options
                .Connect(new Uri(endpoint), new DefaultAzureCredential())
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register("shared:sentinel", refreshAll: true);
                    refresh.SetRefreshInterval(TimeSpan.FromMinutes(5));
                })
                .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
                .ConfigureKeyVault(x => x.SetCredential(new DefaultAzureCredential()));
        });

        return builder;
    }
}
