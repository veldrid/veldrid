using System.IO;
using Newtonsoft.Json;

namespace Veldrid.RenderDemo
{
    public class Preferences
    {
        public bool AllowDebugContexts { get; set; }

        private static Preferences _instance;
        public static Preferences Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadPreferences();
                }

                return _instance;
            }
        }

        private static Preferences LoadPreferences()
        {
            string preferencesFile = Path.Combine(SpecialFolders.VeldridConfigFolder, "config.json");
            System.Diagnostics.Debug.WriteLine("Loading preferences from " + preferencesFile);
            if (File.Exists(preferencesFile))
            {
                string json = File.ReadAllText(preferencesFile);
                return JsonConvert.DeserializeObject<Preferences>(json);
            }

            else
            {
                var ret = new Preferences();
                string json = JsonConvert.SerializeObject(ret);
                if (!Directory.Exists(SpecialFolders.VeldridConfigFolder))
                {
                    Directory.CreateDirectory(SpecialFolders.VeldridConfigFolder);
                }
                File.WriteAllText(preferencesFile, json);
                return ret;
            }
        }
    }
}