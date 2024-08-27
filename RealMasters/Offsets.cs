using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealMasters
{
    public static class Offsets
    {
        // offsets.cs
        public static int dwViewAngles = 0x19D98E8;
        public static int dwLocalPlayerPawn = 0x17D37F0;
        public static int dwEntityList = 0x19684F8;

        public static int dwViewMatrix = 0x19CA480;

        // client.dll.cs
        public static int m_hPlayerPawn = 0x7EC;
        public static int m_iHealth = 0x324;
        public static int m_vOldOrigin = 0x1274;
        public static int m_iTeamNum = 0x3C3;
        public static int m_vecViewOffset = 0xC50;
        public static int m_lifeState = 0x328;
        public static int m_modelState = 0x170;
        public static int m_pGameSceneNode = 0x308;
        public static int m_iszPlayerName = 0x640;
        public static int m_entitySpottedState = 0x2288;
        public static int m_bSpotted = 0x8;
        public static int m_pCameraServices = 0x1130;
        public static int m_iFOV = 0x210;
        public static int m_flFlashBangTime = 0x1348;
        public static int m_bIsScoped = 0x22A0;
        public static int m_iIDEntIndex = 0x13A8;
    }
}
