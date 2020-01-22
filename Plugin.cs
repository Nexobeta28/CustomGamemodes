using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;

namespace CustomGamemodes
{
    public class Plugin : EXILED.Plugin
    {
        //Instance variable for eventhandlers
        public EventHandlers EventHandlers;

        public override void OnEnable()
        {
            try
            {
                Info($"CustomGamemodes loading...");

                EventHandlers = new EventHandlers(this);
                Events.RoundStartEvent += EventHandlers.OnRoundStart;
                Events.RemoteAdminCommandEvent += EventHandlers.OnCommand;
                Info($"CustomGamemodes loaded!");
            }
            catch (Exception e)
            {
                Error($"CustomGamemodes error: {e}");
            }
        }

        public override void OnDisable()
        {
            Events.RoundStartEvent -= EventHandlers.OnRoundStart;
            Events.RemoteAdminCommandEvent -= EventHandlers.OnCommand;

            EventHandlers = null;
        }

        public override void OnReload()
        {
            
        }

        public override string getName { get; } = "CustomGamemodes";
    }
}
