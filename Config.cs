using Newtonsoft.Json;

namespace AntiAccessoryGlitch
{
    public class Config
    {
        public int timer = 1000;

        public void Write()
        {
            File.WriteAllText(AntiAccessoryGlitch.path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read()
        {
            if (!File.Exists(AntiAccessoryGlitch.path))
            {
                return new Config();
            }
            else
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(AntiAccessoryGlitch.path));
            }
        }
    }
}
