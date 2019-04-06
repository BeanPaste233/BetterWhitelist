using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BWL_RW
{
    [ApiVersion(2, 1)]

    public class BetterWhitelist : TerrariaPlugin
    {
        public override string Author => "LuoCloud";
        public override string Description => "A better whitelist plugin!";
        public override string Name => "BetterWhitelist";
        public override Version Version => new Version(1, 0);
        public  string ConfigPath { get { return Path.Combine(TShock.SavePath, "BetterWhitelist.json"); } }
        public string pname = "[BetterWhitelist]";
        public List<string> userNames = new List<string>();
        public BetterWhitelist(Main game) : base(game) { }
        public override void Initialize()
        {
            //钩子注册
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            Commands.ChatCommands.Add(new Command("bwl.use", betterwhitelist, "bwl"));
            if (!File.Exists(ConfigPath))
            {
                userNames.Add("example");
                Players plr = new Players();
                plr.UserNames = userNames.ToArray();
                File.WriteAllText(ConfigPath,JsonConvert.SerializeObject(plr,Formatting.Indented));
            }
    }

        private void betterwhitelist(CommandArgs args)
        {
            Players players = JsonConvert.DeserializeObject<Players>(File.ReadAllText(ConfigPath));
            switch (args.Parameters[0])
            {
                
                case "true":
                    if (players.Enabled == false)
                    {
                        players.Enabled = true;
                        args.Player.SendSuccessMessage("{0}已成功打开插件", pname);
                        File.WriteAllText(ConfigPath,JsonConvert.SerializeObject(players, Formatting.Indented));
                    }
                    else {
                        args.Player.SendErrorMessage("{0}插件处于开启状态", pname);
                    }
                    break;
                case "false":
                    if (players.Enabled == true)
                    {
                        players.Enabled = false;
                        args.Player.SendSuccessMessage("{0}已成功关闭插件", pname);
                        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(players, Formatting.Indented));
                    }
                    else
                    {
                        args.Player.SendErrorMessage("{0}插件处于关闭状态", pname);
                    }
                    break;
                case "help":
                    if (players.Enabled == false)
                    {
                        args.Player.SendErrorMessage("{0}插件处于关闭状态，输入/bwl true 来开启", pname);
                    }
                    else {
                        args.Player.SendInfoMessage("----------[BetterWhitelist]----------\n/bwl help 查看帮助\n/bwl true 开启插件\n/bwl false 关闭插件\n/bwl add 玩家ID 添加玩家进入白名单\n/bwl del 玩家ID 从白名单中删除玩家\n/bwl list 列出白名单列表");
                    }
                    break;
                case "list":
                    if (players.Enabled == false)
                    {
                        args.Player.SendErrorMessage("{0}插件处于关闭状态，输入/bwl true 来开启", pname);
                    }
                    else
                    {
                        foreach (string i in userNames)
                        {
                            args.Player.SendMessage(i, Color.Pink);
                        }
                    }
                    break;
                case "add":
                    if (players.Enabled == false)
                    {
                        args.Player.SendErrorMessage("{0}插件处于关闭状态，输入/bwl true 来开启", pname);
                    }
                    else
                    {
                        if (userNames.Contains(args.Parameters[1]))
                        {
                            args.Player.SendSuccessMessage("{0}白名单内已存在玩家", pname);
                        }
                        else
                        {
                            userNames.Add(args.Parameters[1]);
                            players.UserNames = userNames.ToArray();
                            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(players, Formatting.Indented));
                            args.Player.SendSuccessMessage("{0}添加成功", pname);
                        }
                    }
                    break;
                case "del":
                    if (players.Enabled == false)
                    {
                        args.Player.SendErrorMessage("{0}插件处于关闭状态，输入/bwl true 来开启", pname);
                    }
                    else
                    {
                        if (!userNames.Contains(args.Parameters[1]))
                        {
                            args.Player.SendSuccessMessage("{0}白名单内不存在该玩家", pname);
                        }
                        else
                        {
                            userNames.Remove(args.Parameters[1]);
                            players.UserNames = userNames.ToArray();
                            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(players, Formatting.Indented));
                            args.Player.SendSuccessMessage("{0}删除成功", pname);
                        }
                    }
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            //注销
            if (disposing)
            {
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
            }
            base.Dispose(disposing);
        }
        private void OnJoin(JoinEventArgs args)
        {
            Players players = JsonConvert.DeserializeObject<Players>(File.ReadAllText(ConfigPath));
            TSPlayer ts = TShock.Players[args.Who];
            if (!players.Enabled==false)
            {
                if (!userNames.Contains(ts.Name))
                {
                    ts.Disconnect("未在服务器白名单中");
                }
            }
        }
    }
    public class Players {
        public string[] UserNames = new string[] { };
        public bool Enabled = false;
    }
}
