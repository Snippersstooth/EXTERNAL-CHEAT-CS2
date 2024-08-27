using RealMasters;
using Swed64;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

class Aimbot
{
    private static List<Entity> entities = new List<Entity>();
    private static Entity localPlayer = new Entity();
    private Swed swed;
    private IntPtr client;
    private Renderer renderer;
    private Vector2 screenSize;
    private static readonly string logFilePath = "aimbot_log.txt";

    public bool aimOnClosest { get; set; } = true; // Default to true; adjust as needed

    public Aimbot(Swed swed, IntPtr client, Renderer renderer, Vector2 screenSize)
    {
        this.swed = swed ?? throw new ArgumentNullException(nameof(swed));
        this.client = client;
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.screenSize = screenSize;
    }

    private int MapHotkeyToVirtualKey(string hotkey)
    {
        return hotkey switch
        {
            "CTRL" => 0x11,
            "SHIFT" => 0x10,
            "ALT" => 0x12,
            "MOUSE 1" => 0x01,
            "MOUSE 2" => 0x02,
            "MOUSE 4" => 0x05,
            "MOUSE 5" => 0x06,
            _ => 0 // Default or invalid key
        };
    }

    public void RunAimbot()
    {
        while (true)
        {
            entities.Clear();

            IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);
            IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

            localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
            localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
            localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
            localPlayer.view = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);

            for (int i = 0; i < 64; i++)
            {
                if (listEntry == IntPtr.Zero)
                    continue;

                IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
                if (currentController == IntPtr.Zero)
                    continue;

                int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
                if (pawnHandle == 0)
                    continue;

                IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
                IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

                if (currentPawn == localPlayer.pawnAddress)
                    continue;

                int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
                int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
                uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

                if (lifeState != 256 || (team == localPlayer.team && !renderer.aimOnTeam))
                    continue;

                Entity entity = new Entity
                {
                    pawnAddress = currentPawn,
                    controllerAddress = currentController,
                    health = health,
                    lifeState = lifeState,
                    origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin),
                    view = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset),
                    distance = Vector3.Distance(localPlayer.origin, swed.ReadVec(currentPawn, Offsets.m_vOldOrigin))
                };

                entities.Add(entity);
            }

            // Log entities and aiming state
            using (StreamWriter sw = new StreamWriter(logFilePath, append: true))
            {
                sw.WriteLine($"Entities Count: {entities.Count}");
                sw.WriteLine($"Aim On Closest: {aimOnClosest}");

                if (aimOnClosest)
                {
                    entities = entities.OrderBy(o => o.distance).ToList();
                    sw.WriteLine("Aiming at closest entity.");
                }
                else
                {
                    sw.WriteLine("Aiming at non-closest entities.");
                }

                int hotkeyCode = MapHotkeyToVirtualKey(renderer.GetSelectedHotkey());

                if (entities.Count > 0 && GetAsyncKeyState(hotkeyCode) < 0 && renderer.aimbot)
                {
                    Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view);
                    Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);

                    Vector2 newAngles = Calculate.CalculateAngles(playerView, entityView);
                    Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.1f);

                    swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);
                    sw.WriteLine("Aiming at entity.");
                }
                else
                {
                    sw.WriteLine("Not aiming at any entity.");
                }
            }

            Thread.Sleep(10);
        }
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
