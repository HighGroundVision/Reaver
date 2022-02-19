using HGV.Reaver.Models;
using Microsoft.Extensions.Options;

namespace HGV.Reaver.Factories
{

    public interface ISteamFactory
    {

    }

    public class SteamFactory : ISteamFactory
    {

        public SteamFactory(IOptions<ReaverSettings> settings)
        {
            //settings.Value.SteamKey
            //settings.Value.SteamUser
            //settings.Value.SteamPassword
        }

    }
}
