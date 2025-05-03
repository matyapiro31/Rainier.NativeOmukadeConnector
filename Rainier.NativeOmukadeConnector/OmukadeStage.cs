using ClientNetworking;
using ClientNetworking.Models.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainier.NativeOmukadeConnector
{
    public class OmukadeStage : IStage
    {
        public bool VerifySslCert
        {
            get => false;
        }

        public bool SupportsRouting
        {
            get => true;
        }

        public IRoute GlobalRoute
        { get; }

        public string Name
        { get; set; }

        public OmukadeStage(string subdomain)
        {
            this._subdomain = subdomain;
            this.Name = "OmukadeStage";
            this.GlobalRoute = new OmukadeRoute(true, false, this._subdomain, null);
        }
        public IRoute RouteForRegion(string region)
        {
            int _port;
            if (int.TryParse(region, out _port))
            {
                string subdomainNoport = this._subdomain.Split(':')[0];
                return new OmukadeRoute(false, false, subdomainNoport + ":" + _port, null);
            }
            string text = this._subdomain;
            return new OmukadeRoute(false, true, text, null);
        }

        public IRoute RouteForResponse(QueryRouteResponse route)
        {
            return new OmukadeRoute(false, false, this._subdomain, route.serviceGroup);
        }

        public override string ToString()
        {
            return this.Name;
        }
        private string _subdomain;
    }
    public class OmukadeSecureStage : IStage
    {
        public bool VerifySslCert
        {
            get => true;
        }

        public bool SupportsRouting
        {
            get => true;
        }

        public IRoute GlobalRoute
        { get; }

        public string Name
        { get; set; }

        public OmukadeSecureStage(string subdomain)
        {
            this._subdomain = subdomain;
            this.Name = "OmukadeSecureStage";
            this.GlobalRoute = new OmukadeRoute(true, true, this._subdomain, null);
        }
        public IRoute RouteForRegion(string region)
        {
            int _port;
            if (int.TryParse(region, out _port))
            {
                string subdomainNoport = this._subdomain.Split(':')[0];
                return new OmukadeRoute(false, true, subdomainNoport + ":" + _port, null);
            }
            string text = this._subdomain;
            return new OmukadeRoute(false, true, text, null);
        }

        public IRoute RouteForResponse(QueryRouteResponse route)
        {
            return new OmukadeRoute(false, true, this._subdomain, route.serviceGroup);
        }

        public override string ToString()
        {
            return this.Name;
        }
        private string _subdomain;
    }
}
