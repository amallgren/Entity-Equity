using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace EntityEquity.Common
{
    public class CookieBridgeHubConnection
    {
        private NavigationManager _navigationManager { get; set; }
        private CookieBridge _cookieBridge { get; set; }
        public CookieBridgeHubConnection(NavigationManager navigationManager, CookieBridge cookieBridge)
        {
            _navigationManager = navigationManager;
            _cookieBridge = cookieBridge;
        }
        public HubConnection GetHubConnection(string hubUrl, string cookie)
        {
            _cookieBridge.Value = cookie;
            return new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri(hubUrl), options =>
            {
                options.Cookies = _cookieBridge.CookieContainer;
            })
            .Build();
        }
    }
}
