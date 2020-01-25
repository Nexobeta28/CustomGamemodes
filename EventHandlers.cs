using System.Collections.Generic;
using System;
using EXILED;
using Grenades;
using MEC;
using UnityEngine;
using System.Linq;

namespace CustomGamemodes
{
    public class EventHandlers
    {
        public Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        uint gamemode = 0;
        //0: Default
        //1: Deathmatch
        //2: Team Conqueror

        //COMMANDS AND GAMEMODE SELECTION
        public void OnCommand(ref RACommandEvent ev)
        {
            string[] args = ev.Command.Split(' ');

            if (args[0].ToLower() == "customgamemodes")
            {
                ev.Allow = false;
                switch (args[1].ToLower())
                {
                    case "default":
                        ev.Sender.RAMessage("Default gamemode will be loaded in the next round!");
                        gamemode = 0;
                        break;

                    case "deathmatch":
                        ev.Sender.RAMessage("Deathmatch gamemode will be loaded in the next round!");
                        gamemode = 1;
                        break;

                    case "teamconquer":
                        ev.Sender.RAMessage("Team Conqueror gamemode will be loaded in the next round!");
                        gamemode = 2;
                        break;

                    case "help":
                        ev.Sender.RAMessage("====================================================");
                        ev.Sender.RAMessage("-CustomGamemodes help-");
                        ev.Sender.RAMessage("");
                        ev.Sender.RAMessage("Usage: customgamemodes [gamemode]");
                        ev.Sender.RAMessage("");
                        ev.Sender.RAMessage("Gamemodes:");
                        ev.Sender.RAMessage("default: Classic SCP:SL gamemode.");
                        ev.Sender.RAMessage("deathmatch: Everyone is a Facility Guard with a USP.");
                        ev.Sender.RAMessage("teamconquer: Teams vs teams. When someone is killed, it");
                        ev.Sender.RAMessage("will converted to the team of the attacker.");
                        ev.Sender.RAMessage("====================================================");
                        break;

                    default:
                        ev.Sender.RAMessage("Select a valid gamemode.");
                        break;
                }
            }
        }

        public void OnRoundStart()
        {
            foreach (ReferenceHub hub in Plugin.GetHubs())
            {
                switch (gamemode)
                {
                    case 0://default
                        Plugin.Info("Default gamemode loaded");
                        break;

                    case 1://deathmatch
                        Plugin.Info("Deathmatch gamemode selected");

                        if (!ServerConsole.FriendlyFire)
                        {
                            ServerConsole.FriendlyFire = true;
                        }
                        Plugin.Info("Loading deathmatch...");
                        Timing.RunCoroutine(DeathmatchCR(hub));

                        Plugin.Info("Done! :>");
                        break;

                    case 2://team conquer
                        Plugin.Info("Team conquer gamemode selected");
                        if (ServerConsole.FriendlyFire)
                        {
                            ServerConsole.FriendlyFire = false;
                        }
                        Plugin.Info("Loading team conquer...");
                        Timing.RunCoroutine(TeamConquerCR(hub));
                        Plugin.Info("Done! :>");
                        break;

                    default:
                        break;
                }
            }
        }

        public void OnPlayerDeath(ref PlayerDeathEvent ev)
        {
            switch (gamemode)
            {
                case 0://default
                    break;

                case 1://deathmatch
                    if (RoundSummary.singleton.CountRole(RoleType.FacilityGuard) <= 1)
                    {
                        foreach (ReferenceHub hub in Plugin.GetHubs())
                        {
                            RoundSummary.RoundLock = false;
                            string winner = hub.nicknameSync.Network_myNickSync;
                            hub.Broadcast(5, winner + " has won the round!");
                        }
                    }
                    break;

                case 2://team conquer
                    if(ev.Killer.characterClassManager.CurClass == RoleType.Scp096)
                    {
                        ev.Player.characterClassManager.SetPlayersClass(RoleType.Scp0492, ev.Player.gameObject, true);
                    }
                    else
                    {
                        RoleType KillerRole = ev.Killer.characterClassManager.CurClass;
                        ev.Player.characterClassManager.SetPlayersClass(KillerRole, ev.Player.gameObject, true);
                    }

                    var classes = new List<RoleType>();
                    foreach (ReferenceHub hub in Plugin.GetHubs())
                    {
                        classes.Add(hub.characterClassManager.CurClass);
                    }

                    if (classes.Any(o => o != classes[0])) //if all the elements in the list are the same
                    {
                        RoundSummary.RoundLock = false;
                    }

                    break;

                default:
                    break;
            }
        }

        public void OnPlayerHurt(ref PlayerHurtEvent ev)
        {
            switch (gamemode)
            {
                case 0:
                    break;

                case 1:
                    break;

                case 2:
                    if(ev.Info.Amount >= ev.Player.playerStats.health)
                    {
                        ev.Player.inventory.Clear(); //don't drop items
                    }

                    break;
            }
        }

        public void OnTeamRespawn(ref TeamRespawnEvent ev)
        {
            switch (gamemode)
            {
                case 0:
                    break;

                case 1:
                    ev.ToRespawn.Clear();
                    break;

                case 2:
                    ev.ToRespawn.Clear();
                    break;
            }
        }

        //COROUTINES
        public IEnumerator<float> DeathmatchCR(ReferenceHub hub)
        {
            //Round start
            yield return Timing.WaitForSeconds(3f);

            hub.characterClassManager.SetPlayersClass(RoleType.FacilityGuard, hub.gameObject, true);
            hub.inventory.Clear();
            hub.inventory.AddNewItem(ItemType.GunUSP);
            hub.gameObject.GetComponent<AmmoBox>().SetOneAmount(2, "250");
            RoundSummary.RoundLock = true;
            hub.Broadcast(5, "<color=#ff0000>Deathmatch: Kill everyone and stay alive.</color>");
        }

        public IEnumerator<float> TeamConquerCR(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(3f);

            hub.inventory.Clear();

            //Assign items to each class and set scps to dclass
            switch (hub.characterClassManager.CurClass)
            {
                case RoleType.ClassD:
                    hub.inventory.AddNewItem(ItemType.GunUSP);
                    hub.gameObject.GetComponent<AmmoBox>().SetOneAmount(2, "250");
                    hub.inventory.AddNewItem(ItemType.KeycardGuard);
                    break;

                case RoleType.FacilityGuard:
                    hub.inventory.AddNewItem(ItemType.GunProject90);
                    hub.gameObject.GetComponent<AmmoBox>().SetOneAmount(2, "500");
                    hub.inventory.AddNewItem(ItemType.Medkit);
                    hub.inventory.AddNewItem(ItemType.KeycardGuard);
                    break;

                case RoleType.Scientist:
                    hub.inventory.AddNewItem(ItemType.GunE11SR);
                    hub.gameObject.GetComponent<AmmoBox>().SetOneAmount(2, "500");
                    hub.inventory.AddNewItem(ItemType.Medkit);
                    hub.inventory.AddNewItem(ItemType.KeycardGuard);
                    break;

                case RoleType.Scp049 | RoleType.Scp079 | RoleType.Scp096 | RoleType.Scp106 | RoleType.Scp173 | RoleType.Scp93953 | RoleType.Scp93989:
                    //if player is scp set class to dclass and give items
                    hub.characterClassManager.SetPlayersClass(RoleType.ClassD, hub.gameObject, true);
                    hub.inventory.AddNewItem(ItemType.GunUSP);
                    hub.gameObject.GetComponent<AmmoBox>().SetOneAmount(2, "250");
                    hub.inventory.AddNewItem(ItemType.KeycardGuard);
                    break;
            }

            var dclasses = new List<ReferenceHub>();
            var rand = new System.Random();
            int index = rand.Next(dclasses.Count);

            foreach (ReferenceHub p in Plugin.GetHubs())
            {
                if (p.characterClassManager.CurClass == RoleType.ClassD)
                {
                    dclasses.Add(p);
                }
            }

            ReferenceHub scp = dclasses[index];
            scp.characterClassManager.SetPlayersClass(RoleType.Scp096, scp.gameObject, true);
            scp.Broadcast(5, "<color=ff0000>Kill to recruit zombies to your team.</color>");

            RoundSummary.RoundLock = true;
            hub.Broadcast(5, "<color=#ff0000>Team Conquer: Kill players from the other teams for recruit them in your team. </color><color=blue>[FF = Off]</color>");
        }
    }
}
