using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using Exiled.API.Enums;

namespace CampingOpenGates
{
    public class Config : IConfig
    {
        [Description("Determines whether or not the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether or not to show debug logs.")]
        public bool Debug { get; set; } = true;

        [Description("The number of non-SCP players still alive to check for camping.")]
        public int NumberOfPlayers { get; set; } = 2;

        [Description("Number of seconds of a player in a camping location required to detect camping.")]
        public int CampingLimit { get; set; } = 60;

        [Description("Number of seconds a door is uninteractable and stuck open after camping is detected.")]
        public int DoorFrozenTime { get; set; } = 60;

        [Description("Determines if the door automatically closes or not after door frozen time is up.")]
        public bool CloseDoor { get; set; } = true;

        [Description("Determines which rooms (and their doors) are unlocked on camping detected.")]
        public Dictionary<RoomType, List<DoorType>> CampRooms { get; set; } = new Dictionary<RoomType, List<DoorType>>
        {
            { RoomType.Surface, new List<DoorType> { DoorType.GateA, DoorType.GateB } },
            { RoomType.EzGateA, new List<DoorType> { DoorType.GateA } },
            { RoomType.EzGateB, new List<DoorType> { DoorType.GateB } },
            { RoomType.EzIntercom, new List<DoorType> { DoorType.Intercom } },
            { RoomType.HczArmory, new List<DoorType> { DoorType.HczArmory } },
            { RoomType.HczHid, new List<DoorType> { DoorType.HID } },
            { RoomType.Hcz049, new List < DoorType > { DoorType.Scp049Armory } },
            { RoomType.Hcz079, new List < DoorType > { DoorType.Scp079Armory } },
            { RoomType.Hcz096, new List < DoorType > { DoorType.Scp096 } },
            { RoomType.Hcz106, new List < DoorType > { DoorType.Scp106Primary, DoorType.Scp106Secondary } },
            
        };
    }
}