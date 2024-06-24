using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public static class HttpClientFactory
{
    private static HttpClient client;

    private static WindowsProxyUsePolicy currentProxySettings;

    static HttpClientFactory()
    {
        currentProxySettings = WindowsProxyUsePolicy.UseWinInetProxy;
        client = new HttpClient(new WinHttpHandler
        {
            WindowsProxyUsePolicy = currentProxySettings
        });
    }

    public static HttpClient GetClient()
    {
        return client;
    }

    public static async Task CheckProxySettings()
    {
        try
        {
            string requestUri = "https://www.google.com";
            client = new HttpClient(new WinHttpHandler
            {
                WindowsProxyUsePolicy = WindowsProxyUsePolicy.UseWinHttpProxy
            });
            if ((await client.GetAsync(requestUri)).StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
            {
                SwitchProxyPolicy();
            }
        }
        catch (Exception)
        {
        }
    }

    public static void SwitchProxyPolicy()
    {
        currentProxySettings = ((currentProxySettings != WindowsProxyUsePolicy.UseWinHttpProxy) ? WindowsProxyUsePolicy.UseWinHttpProxy : WindowsProxyUsePolicy.UseWinInetProxy);
        client = new HttpClient(new WinHttpHandler
        {
            WindowsProxyUsePolicy = currentProxySettings
        });
    }
}
