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
                        Plugin.Info("====================================================");
                        Plugin.Info("-CustomGamemodes help-");
                        Plugin.Info("");
                        Plugin.Info("Usage: customgamemodes [gamemode]");
                        Plugin.Info("");
                        Plugin.Info("Gamemodes:");
                        Plugin.Info("default: Classic SCP:SL gamemode.");
                        Plugin.Info("deathmatch: Everyone is a Facility Guard with a USP.");
                        Plugin.Info("====================================================");
                        break;

                    default:
                        Plugin.Info("Select a valid gamemode.");
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

                        Plugin.Info("Loading deathmatch...");
                        Timing.RunCoroutine(DeathmatchCR(hub));

                        Plugin.Info("Done! :>");
                        break;

                    default:
                        break;
                }
            } 
        }

        //COROUTINES
        public IEnumerator<float> DeathmatchCR(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(3f);

            hub.characterClassManager.SetPlayersClass(RoleType.FacilityGuard, hub.gameObject, true);
            hub.inventory.Clear();
            hub.inventory.AddNewItem(ItemType.GunUSP);

            for (int i = 0; i < 3; i++)
            {
                PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(ItemType.Ammo9mm, -4.656647E+11f, hub.gameObject.transform.position, Quaternion.Euler(0, 0, 0), 0, 0, 0);
            }
        }
    }
}
