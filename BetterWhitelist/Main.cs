using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TerrariaApi.Server;
using Newtonsoft.Json;
using System.IO;
using TShockAPI.Hooks;

namespace BetterWhitelist
{
    [ApiVersion(2, 1)]
    public class Main : TerrariaPlugin
    {
        public Main(Terraria.Main game) : base(game){ }
        public override string Name => "BetterWhitelist";
        public override Version Version => new Version(2,0);
        public override string Author => "Bean_Paste";
        public override string Description => "A whitelist of players by checking their names";
        public static string config_path = TShock.SavePath + "/BetterWhitelist/config.json";
        public static string translation_path = TShock.SavePath + "/BetterWhitelist/language.json";
        public static BConfig _config;
        public static Translation _translation;
        public static Dictionary<string, TSPlayer> players = new Dictionary<string, TSPlayer>();//用于存储TSPlayer对象的字典

        public override void Initialize()
        {
            if (Directory.Exists(TShock.SavePath + "/BetterWhitelist"))
            {
                Load();
            }
            else {
                Directory.CreateDirectory(TShock.SavePath + "/BetterWhitelist");
                Load();
            }
            Commands.ChatCommands.Add(new Command("bwl.use",bwl,"bwl"));
            ServerApi.Hooks.ServerJoin.Register(this,OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this,OnLeave);
            
        }
        private void OnLeave(LeaveEventArgs args)
        {
            TSPlayer plr = new TSPlayer(args.Who);//实例化一个玩家对象
            players.Remove(plr.Name);//将玩家以TSPlayer对象从players字典中删除
        }

        private void bwl(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage(_translation.language["HelpText"]);
                return;
            }
                switch (args.Parameters[0])
                {
                    case "add":
                    if (_config.Disabled)//检测配置文件的Disabled选项
                    {
                        args.Player.SendErrorMessage(_translation.language["NotEnabled"]);
                    }
                    else {
                        _config.WhitePlayers.Add(args.Parameters[1]);
                        args.Player.SendSuccessMessage(_translation.language["SuccessfullyAdd"]);
                        File.WriteAllText(config_path,JsonConvert.SerializeObject(_config,Formatting.Indented));
                            }
                        break;
                    case "del":
                    if (_config.Disabled)//检测配置文件的Disabled选项
                    {
                        args.Player.SendErrorMessage(_translation.language["NotEnabled"]);
                    }
                    else {
                        if (_config.WhitePlayers.Contains(args.Parameters[1]))
                        {
                            _config.WhitePlayers.Remove(args.Parameters[1]);
                            args.Player.SendSuccessMessage(_translation.language["SuccessfullyDelete"]);
                            File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                            if (players[args.Parameters[1]].Active==true)//检测玩家是否在线
                            {
                                players[args.Parameters[1]].Disconnect(_translation.language["DisconnectReason"]);//从白名单删除后，如果在线踢出玩家
                            }
                        }
                    }
                    break;
                    case "list":
                    foreach (var item in _config.WhitePlayers)
                    {
                        args.Player.SendInfoMessage(item);
                    }
                        break;
                    case "help":
                    args.Player.SendInfoMessage("-------[BetterWhitelist]-------");
                    args.Player.SendInfoMessage(_translation.language["AllHelpText"]);
                       break;
                    case "true"://通过指令打开白名单时，检测玩家是否在已保存的白名单内，不在则踢出服务器//这里还是有问题
                    if (_config.Disabled == false)
                    {
                        args.Player.SendErrorMessage(_translation.language["FailedEnable"]);
                    }
                    else {
                        _config.Disabled = false;
                        args.Player.SendSuccessMessage(_translation.language["SuccessfullyEnable"]);
                        if (players.Count > 0)
                        {
                            if (_config.WhitePlayers.Count > 0)
                            {
                                for (int i = 0; i < players.Count; i++)
                                {
                                    if (!_config.WhitePlayers.Contains(players.Keys.ToList()[i]))
                                    {
                                        players[players.Keys.ToList()[i]].Disconnect(_translation.language["NotOnList"]);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var item in players.Values)
                                {
                                    item.Disconnect(_translation.language["NotOnList"]);
                                }
                            }
                        }
                        File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                    }
                        break;
                    case "false":
                    if (_config.Disabled == true)
                    {
                        args.Player.SendErrorMessage(_translation.language["FailedDisable"]);
                    }
                    else
                    {
                        _config.Disabled = true;
                        args.Player.SendSuccessMessage(_translation.language["SuccessfullyDisable"]);
                        File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
                    }
                    break;
                case "reload":
                    _config = JsonConvert.DeserializeObject<BConfig>(File.ReadAllText(config_path));
                    _translation = JsonConvert.DeserializeObject<Translation>(File.ReadAllText(translation_path));
                    args.Player.SendSuccessMessage(_translation.language["SuccessfullyReload"]);
                    break;
                }
        }

        private void OnJoin(JoinEventArgs args)
        {
            TSPlayer plr = new TSPlayer(args.Who);//实例化一个玩家对象
            string name = plr.Name;
            players.Add(name, new TSPlayer(args.Who));//将玩家以TSPlayer对象存储在players字典中
            if (_config.Disabled == true)//检测插件是否开启
            {
                TShock.Log.ConsoleInfo(_translation.language["NotEnabled"]);
            }
            else
            {
                if (!_config.WhitePlayers.Contains(name))//检测玩家是否在白名单
                {
                    plr.Disconnect(_translation.language["NotOnList"]);//阻止非白名单玩家进入服务器
                }
            }
        }
        private void Load() {//加载配置文件函数
            _config = BConfig.Load(config_path);
            _translation = Translation.Load(translation_path);
            File.WriteAllText(config_path, JsonConvert.SerializeObject(_config, Formatting.Indented));
            File.WriteAllText(translation_path, JsonConvert.SerializeObject(_translation, Formatting.Indented));
        }
     

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerJoin.Deregister(this,OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this,OnLeave);
                Commands.ChatCommands.Remove(new Command("bwl.use", bwl,"bwl"));
            }
            base.Dispose(disposing);
        }
    }
}
