using System;

using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Loader;
using Handlers = Exiled.Events.Handlers;

namespace CampingOpenGates
{
    public class Plugin : Plugin<Config, Translation>
    {
        public static Plugin Singleton;
        private EventHandlers handler;
        public static bool ScanInProgress = false;
        public static bool Force = false;
        public override void OnEnabled()
        {
            Singleton = this;
            handler = new EventHandlers(this);

            Handlers.Server.RoundStarted += handler.OnRoundStarted;
            Handlers.Server.RoundEnded += handler.OnRoundEnded;
            Handlers.Player.Spawned += handler.OnSpawned;
            Handlers.Warhead.Detonated += handler.OnDetonated;
            Handlers.Player.TogglingFlashlight += handler.OnToggling;
            Handlers.Player.InteractingDoor += handler.OnInteractingDoor;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Handlers.Server.RoundStarted -= handler.OnRoundStarted;
            Handlers.Server.RoundEnded -= handler.OnRoundEnded;
            Handlers.Player.Spawned -= handler.OnSpawned;
            Handlers.Warhead.Detonated -= handler.OnDetonated;
            Handlers.Player.TogglingFlashlight -= handler.OnToggling;
            Handlers.Player.InteractingDoor -= handler.OnInteractingDoor;

            handler = null;
            Singleton = null;

            base.OnDisabled();
        }

        public override string Name => "Camping Open Gates";
        public override string Author => "Nick";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(7, 2, 0);
        public override PluginPriority Priority => PluginPriority.High;
    }
}