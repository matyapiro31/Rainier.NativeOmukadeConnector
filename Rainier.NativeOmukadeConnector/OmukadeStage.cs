using ClientNetworking;
using ClientNetworking.Models.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainier.NativeOmukadeConnector
{
    public class OmukadeStage : Stage
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

        public string name
        { get; set; }

        public OmukadeStage(string subdomain)
        {
            this._subdomain = subdomain;
            this.name = "OmukadeStage";
            this.GlobalRoute = new DomainRoute(true, false, this._subdomain, null);
        }
        public IRoute RouteForRegion(string region)
        {
            int _port;
            if (int.TryParse(region, out _port))
            {
                string subdomainNoport = this._subdomain.Split(':')[0];
                return new DomainRoute(false, false, subdomainNoport + ":" + _port, null);
            }
            string text = this._subdomain;
            return new DomainRoute(false, true, text, null);
        }

        public IRoute RouteForResponse(QueryRouteResponse route)
        {
            return new DomainRoute(false, false, this._subdomain, route.serviceGroup);
        }

        public override string ToString()
        {
            return this.name;
        }
        private string _subdomain;
    }
    public class OmukadeSecureStage : Stage
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

        public string name
        { get; set; }

        public OmukadeSecureStage(string subdomain)
        {
            this._subdomain = subdomain;
            this.name = "OmukadeSecureStage";
            this.GlobalRoute = new DomainRoute(true, true, this._subdomain, null);
        }
        public IRoute RouteForRegion(string region)
        {
            int _port;
            if (int.TryParse(region, out _port))
            {
                string subdomainNoport = this._subdomain.Split(':')[0];
                return new DomainRoute(false, true, subdomainNoport + ":" + _port, null);
            }
            string text = this._subdomain;
            return new DomainRoute(false, true, text, null);
        }

        public IRoute RouteForResponse(QueryRouteResponse route)
        {
            return new DomainRoute(false, true, this._subdomain, route.serviceGroup);
        }

        public override string ToString()
        {
            return this.name;
        }
        private string _subdomain;
    }
}
