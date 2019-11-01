using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using IRP = Rocket.API.IRocketPlayer;
using Math = System.Math;
using UP = Rocket.Unturned.Player.UnturnedPlayer;

namespace Greenorine.GreenCoords
{
    public class Coords : RocketPlugin<GreenCoordsConfiguration>
    {
        public static Coords Instance = null;
        public static Dictionary<CSteamID, short> dict = new Dictionary<CSteamID, short>();
        private static readonly DateTime c = DateTime.Now;

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "ShowCoords", "You enabled coords." },
            { "HideCoords", "You hid coords." },
            { "Usage", "Use: /coords." }
        };

        protected override void Load()
        {
            Instance = this;
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerConnected(UP player)
        {
            var steamid = player.CSteamID;
            dict.Add(steamid, 1);
            player.Player.gameObject.AddComponent<PlayerComponent>();
        }

        private void OnPlayerDisconnected(UP player)
        {
            var steamid = player.CSteamID;
            dict.Remove(steamid);
        }

        public class PlayerComponent : MonoBehaviour
        {
            private Player player;
            private Vector3 Pos => player.transform.position;
            private Vector3 lastPos;

            void Awake()
            {
                player = GetComponent<Player>();
                lastPos = Pos;
            }

            void FixedUpdate()
            {
                if ((DateTime.Now - c).TotalSeconds >= 0.5)
                {
                    var untplayer = UP.FromPlayer(player);
                    var steamid = untplayer.CSteamID;
                    if (Pos != lastPos)
                    {
                        dict.TryGetValue(steamid, out var value);
                        lastPos = Pos;
                        var x = Math.Ceiling(untplayer.Position.x).ToString();
                        var y = Math.Ceiling(untplayer.Position.y).ToString();
                        var z = Math.Ceiling(untplayer.Position.z).ToString();
                        if (value == 1)
                        {
                            EffectManager.sendUIEffect(56843, 3457, steamid, true, x, y, z);
                        }
                    }
                }
            }
        }

        [RocketCommand("scoords", "Show/hide coords", "/scoords", AllowedCaller.Player)]
        public void ExecuteCommandAlert(IRP caller, string[] command)
        {
            if (caller is UP player)
            {
                CSteamID steamid = player.CSteamID;
                if (command.Length == 0)
                {
                    dict.TryGetValue(steamid, out var value);
                    if (value == 1)
                    {
                        dict[steamid] = 0;
                        EffectManager.askEffectClearByID(56843, steamid);
                        UnturnedChat.Say(player, Translate("ShowCoords"), Color.green);
                    }
                    else
                    {
                        dict[steamid] = 1;
                        var x = Math.Ceiling(player.Position.x).ToString();
                        var y = Math.Ceiling(player.Position.y).ToString();
                        var z = Math.Ceiling(player.Position.z).ToString();
                        EffectManager.sendUIEffect(56843, 3457, steamid, true, x, y, z);
                        UnturnedChat.Say(player, Translate("HideCoords"), Color.green);
                    }
                }
                else
                {
                    UnturnedChat.Say(player, Translate("Usage"), Color.red);
                }
            }
        }
    }
}
