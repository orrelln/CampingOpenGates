using Exiled.API.Enums;
using Exiled.API.Interfaces;
using Scanner.Structures;
using System.Collections.Generic;
using System.ComponentModel;

namespace CampingOpenGates
{
    public class Translation : ITranslation
    {
        [Description("Determines the C.A.S.S.I.E string to use when door is malfunctioned.")]
        public CassieMessage DoorMalfunctionMessage { get; set; } = new CassieMessage(
            "DANGER, MALFUNCTION DETECTED IN {DOORTYPE} NEAR {ROOM} . BACKUP SUBSYSTEM OPERATIONAL IN {SECONDS} SECONDS.",
            "Danger, malfunction detected in {DOORTYPE} near {ROOM}. Backup subsystem operational in {SECONDS} seconds."
        );

        public CassieMessage GateMalfunctionMessage { get; set; } = new CassieMessage(
            "DANGER, MALFUNCTION DETECTED IN {ROOM} . BACKUP SUBSYSTEM OPERATIONAL IN {SECONDS} SECONDS.",
            "Danger, malfunction detected in {ROOM}. Backup subsystem operational in {SECONDS} seconds."
        );

        [Description("Determines how C.A.S.S.I.E will pronounce rooms.")]
        public Dictionary<RoomType, CassieMessage> RoomPronounciation { get; set; } = new Dictionary<RoomType, CassieMessage>
        {
            [RoomType.EzGateA] = new CassieMessage("GATE A", "Gate A"),
            [RoomType.EzGateB] = new CassieMessage("GATE B", "Gate B"),
            [RoomType.Surface] = new CassieMessage("GATE A AND GATE B", "Gate A and Gate B"),
            [RoomType.HczHid] = new CassieMessage("MICRO H I D STORAGE", "Micro HID Storage"),
            [RoomType.HczArmory] = new CassieMessage("HEAVY CONTAINMENT ARMORY", "Heavy Containment Armory"),
            [RoomType.Hcz079] = new CassieMessage("SCP 0 7 9 CONTAINMENT ROOM", "SCP-079 Containment Room"),
            [RoomType.Hcz096] = new CassieMessage("SCP 0 9 6 CONTAINMENT ROOM", "SCP-096 Containment Room"),
            [RoomType.Hcz106] = new CassieMessage("SCP 1 0 6 CONTAINMENT ROOM", "SCP-106 Containment Room"),
            [RoomType.Hcz049] = new CassieMessage("SCP 0 4 9 CONTAINMENT ROOM", "SCP-049 Containment Room"),
            [RoomType.EzIntercom] = new CassieMessage("INTERCOM", "Intercom"),
        };

        [Description("Determines how C.A.S.S.I.E will pronounce doors/gates.")]
        public Dictionary<DoorType, CassieMessage> DoorPronounciation { get; set; } = new Dictionary<DoorType, CassieMessage>
        {
            [DoorType.GateA] = new CassieMessage("GATE", "gate"),
            [DoorType.GateB] = new CassieMessage("GATE", "gate"),
            [DoorType.HID] = new CassieMessage("DOOR", "door"),
            [DoorType.HczArmory] = new CassieMessage("DOOR", "door"),
            [DoorType.Scp079Armory] = new CassieMessage("DOOR", "door"),
            [DoorType.Scp096] = new CassieMessage("DOOR", "door"),
            [DoorType.Scp106Primary] = new CassieMessage("DOOR", "door"),
            [DoorType.Scp049Armory] = new CassieMessage("DOOR", "door"),
            [DoorType.Intercom] = new CassieMessage("DOOR", "door"),
        };
    }
}