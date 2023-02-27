using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace EntityEquity.Common
{
    public class CookieBridgeConnection
    {
        private IHttpClientFactory _httpClientFactory;
        private NavigationManager _navigationManager { get; set; }
        private CookieBridge _cookieBridge { get; set; }
        public CookieBridgeConnection(NavigationManager navigationManager, CookieBridge cookieBridge, IHttpClientFactory httpClientFactory)
        {
            _navigationManager = navigationManager;
            _cookieBridge = cookieBridge;
            _httpClientFactory = httpClientFactory;
        }
        public HubConnection GetHubConnection(string hubUrl, string cookie)
        {
            _cookieBridge.Value = cookie;
            return new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri(hubUrl), options =>
            {
                options.Cookies = _cookieBridge.CookieContainer;
            })
            .AddNewtonsoftJsonProtocol()
            .Build();
        }
        public HttpClient GetHttpClient(string cookie)
        {
            _cookieBridge.Value = cookie;
            var httpClient = _httpClientFactory.CreateClient("DefaultHttpClient");
            httpClient.DefaultRequestHeaders.Add("Cookie", $"{_cookieBridge.Name}={_cookieBridge.Value}");
            return httpClient;
        }
    }
}
