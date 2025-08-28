using System.Configuration;

namespace ProfessionalSystemUtility
{
    public interface IConfigurationProvider { string Get(string key); }
    public class AppConfigConfigurationProvider : IConfigurationProvider
    {
        public string Get(string key) { return ConfigurationManager.AppSettings[key]; }
    }
}