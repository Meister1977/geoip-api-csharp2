using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;

namespace LMI.GeoIp
{
    public static class GeoIpLookup
    {
        private const string _geoIpCityFile = "c:\\geocity.dat";
        private static readonly object LockObject = new object();
        private static LookupService _cityLookup;

        private static LookupService CityLookup
        {
            get
            {
                LookupService instance = _cityLookup;
                if (instance != null)
                    return instance;

                lock (LockObject)
                {
                    if (instance != null)
                        return instance;
                    instance = new LookupService(_geoIpCityFile);
                    Thread.MemoryBarrier();

                    _cityLookup = instance;
                    return instance;
                }
            }
        }

        public static bool TryGetCountryFromIp(string ip, out string countryCode)
        {
            IPAddress ipa;
            countryCode = "XX";
            if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out ipa))
                return false;

            try
            {
                countryCode = CityLookup.getCountry(ipa).getCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Country GetCountryFromIp(IPAddress ip)
        {

            Contract.Requires<ArgumentNullException>(ip != null, "ip");
            return CityLookup.getCountry(ip);
        }
    }
}
