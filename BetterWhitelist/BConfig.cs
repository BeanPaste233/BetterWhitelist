using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterWhitelist
{
    public class BConfig
    {
        public bool Disabled { get; set; }
        public List<string> WhitePlayers = new List<string>();
        public static BConfig Load(string path) {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<BConfig>(File.ReadAllText(path));
            }
            else {
                return new BConfig() { Disabled = false };
            }
        }
    }
}
