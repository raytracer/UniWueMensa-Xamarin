using System;
using System.IO;
using System.Reactive;
using System.Reactive.Subjects;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional;

namespace uniwuemensa
{
    public enum JsonPrice {
        Student, Staff, Guest
    }
    public class JsonSettings {
        public JsonPrice Price;
        public string[] Cafeterias;
        public int version;
    }


    public class Settings {
        public static readonly string[] AllCafeterias = new string[] { "Hubland Mensa", "Hubland Mensateria" };
        private static string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "settings.json");
        private JsonSettings _settings;

        private const int Version = 1;
        public JsonSettings settings
        {
            get {
                return _settings;
            }

            set
            {
                _settings = value;
                WriteSettings();
                settingsObs.OnNext(value);
            }
        }
        public Subject<JsonSettings> settingsObs = new Subject<JsonSettings>();

        private static Settings instance;

        private Settings() {
            if (File.Exists(filename)) {
                settings = JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(filename));
                if (settings.version < Version)
                {
                    settings = getDefaultSettings();
                }
            } else {
                settings = getDefaultSettings();
            }
        }

        private static JsonSettings getDefaultSettings()
        {
            return new JsonSettings { Price = JsonPrice.Student, Cafeterias = AllCafeterias };
        }

        public void WriteSettings() {
            var content = JsonConvert.SerializeObject(settings);
            File.WriteAllText(filename, content);
        }

        public static Settings Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new Settings();
                }
                return instance;
            }
        }
    }
}