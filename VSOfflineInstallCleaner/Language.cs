using Newtonsoft.Json;
using System.IO;

namespace VSOfflineInstallCleaner
{
    public class Language
    {
        internal dynamic GetLanguage(string languageFileName)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                };

                string jsonString = File.ReadAllText(languageFileName);
                dynamic dynamicObj = JsonConvert.DeserializeObject(jsonString, settings);
                return dynamicObj;
            }
            catch
            {
                return null;
            }
        }
    }
}
