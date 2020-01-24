using System.Collections.Generic;
using EXILED;
using Grenades;
using MEC;
using UnityEngine;

namespace CustomGamemodes
{
    public class EventHandlers
    {
        public Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        uint gamemode = 0; //Gamemode selection

        //COMMANDS AND GAMEMODE SELECTION
        public void OnCommand(ref RACommandEvent ev)
        {
            string[] args = ev.Command.Split(' ');

            if(args[0].ToLower() == "customgamemodes")
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

                    case "help":
                        ev.Sender.RAMessage("====================================================");
                        ev.Sender.RAMessage("-CustomGamemodes help-");
                        ev.Sender.RAMessage("");
                        ev.Sender.RAMessage("Usage: customgamemodes [gamemode]");
                        ev.Sender.RAMessage("");
                        ev.Sender.RAMessage("Gamemodes:");
                        ev.Sender.RAMessage("default: Classic SCP:SL gamemode.");
                        ev.Sender.RAMessage("deathmatch: Everyone is a Facility Guard with a USP.");
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
                    case 0:
                        Plugin.Info("Default gamemode loaded");
                        break;

                    case 1:
                        Plugin.Info("Deathmatch gamemode selected");
                        hub.Broadcast(5, "<color=#ff0000>Deathmatch: Kill everyone and stay alive.</color>");

                        ServerConsole.FriendlyFire = true;
                        Plugin.Info("Loading deathmatch...");
                        Timing.RunCoroutine(DeathmatchCR(hub));   

                        Plugin.Info("Done! :>");
                        break;

                    default:
                        break;
                }
            } 
        }

        //TODOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO: onenable ondisable.
        public void OnPlayerDeath(ref PlayerDeathEvent ev)
        {
            if (RoundSummary.singleton.CountRole(RoleType.FacilityGuard) <= 1)
            {
                foreach (ReferenceHub hub in Plugin.GetHubs())
                {
                    RoundSummary.RoundLock = false;
                    string winner = hub.nicknameSync.Network_myNickSync;
                    hub.Broadcast(5, winner + " has won the round!");
                }
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

        }
    }
}
