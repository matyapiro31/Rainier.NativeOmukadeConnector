using ClientNetworking;
using ClientNetworking.Models.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainier.NativeOmukadeConnector
{
    public class OmukadeRoute : IRoute
    {
        public bool IsGlobal { get; }
        public Uri WebsocketUrl { get; }
        public string Region { get; }
        public string ServiceGroup { get; set; }
        public Uri ApiUrl(string api)
        {
            return new Uri(this.HttpPrefix + api);
        }

        public void SetServiceGroup(string serviceGroup)
        {
            this.ServiceGroup = serviceGroup;
        }
        public readonly string HttpPrefix;

        public readonly string WssPrefix;

        public readonly string WssSuffix;
        public OmukadeRoute(bool isGlobal, bool enableSsl, string domain, string? serviceGroup)
        {
            if (enableSsl)
            {
                this.HttpPrefix = "https://" + domain;
                this.WssPrefix = "wss://" + domain;
            }
            else
            {
                this.HttpPrefix = "http://" + domain;
                this.WssPrefix = "ws://" + domain;
            }
            this.WssSuffix = "websocket/v1/external/stomp";
            this.IsGlobal = isGlobal;
            this.ServiceGroup = serviceGroup!;
            this.WebsocketUrl = new Uri(this.WssPrefix + "/" + this.WssSuffix);
            this.Region = "";
        }

    }
}
