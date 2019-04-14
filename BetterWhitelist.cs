﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public override Version Version => new Version(1, 1);
        public  string ConfigPath { get { return Path.Combine(TShock.SavePath, "BetterWhitelist.json"); } }
        public string pname = "[BetterWhitelist]";
        public List<string> userNames = new List<string>();
        public BetterWhitelist(Main game) : base(game) { }
        public override void Initialize()
        {
            //钩子注册
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            Commands.ChatCommands.Add(new Command("bwl.use", betterwhitelist, "bwl"));
            //检测白名单配置文件是否存在
            if (!File.Exists(ConfigPath))
            {
                userNames.Add("example");
                Players plr = new Players();
                plr.UserNames = userNames.ToArray();
                File.WriteAllText(ConfigPath,JsonConvert.SerializeObject(plr,Formatting.Indented));
            }
            else//存在，执行读取
            {
                Players ply = JsonConvert.DeserializeObject<Players>(File.ReadAllText(ConfigPath));
                userNames = ply.UserNames.ToList();
            }
    }

        private void betterwhitelist(CommandArgs args)
        {
            //new一个对象进行配置文件的序列化以及数据操作
            Players players = JsonConvert.DeserializeObject<Players>(File.ReadAllText(ConfigPath));
            switch (args.Parameters[0])
            {
                
                case "true":
                    //检测配置文件中的Enabled是否为true
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
                    //检测配置文件中的Enabled是否为false
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
                    //给玩家显示help
                    if (players.Enabled == false)
                    {
                        args.Player.SendErrorMessage("{0}插件处于关闭状态，输入/bwl true 来开启", pname);
                    }
                    else {
                        args.Player.SendInfoMessage("----------[BetterWhitelist]----------\n/bwl help 查看帮助\n/bwl true 开启插件\n/bwl false 关闭插件\n/bwl add 玩家ID 添加玩家进入白名单\n/bwl del 玩家ID 从白名单中删除玩家\n/bwl list 列出白名单列表");
                    }
                    break;
                case "list":
                    //给玩家列出白名单里的用户名
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
                    //添加玩家昵称至白名单
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
                    //删除玩家在白名单里的昵称
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
            //new 一个对象进行字段判断以及数据操作
            Players players = JsonConvert.DeserializeObject<Players>(File.ReadAllText(ConfigPath));
            TSPlayer ts = TShock.Players[args.Who];
            //判断Enabled属性是否开启，否就关闭插件
            if (!players.Enabled==false)
            {
                //判断userNames列表中是否包含玩家昵称,是就允许进入服务器
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