using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon_Showdown_Bot
{
    class Config
    {
        public string user;
        public string password;
        public string team;
        public string teamtypes;
        public string leadPokemon;

        public static Config loadConfig()
        {
            StreamReader sr = new StreamReader(File.OpenRead("config.json"));
            string json = sr.ReadToEnd();
            sr.Close();
            Config config = JsonConvert.DeserializeObject<Config>(json);
            return config;
        }
    }
}
