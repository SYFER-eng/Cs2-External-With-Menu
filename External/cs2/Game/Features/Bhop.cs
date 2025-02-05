using cs2.Config;
using cs2.Game.Objects;
using cs2.Offsets;
using System;
using System.Threading;

namespace cs2.Game.Features
{
    internal class Bhop
    {
        public static void Start()
        {
            _localPlayer = new LocalPlayer();
            _key = new Input.Key(32); // 32 is the key code for the spacebar
            new Thread(() =>
            {
                for (; ; )
                {
                    if (!Config.Configuration.Current.Bhop)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    _key.Update();
                    Update();
                    Thread.Sleep(Configuration.Current.THR_DELAY_BHOP);
                }
            }).Start();
        }

        public static void Update()
        {
            _localPlayer.AddressBase = _localPlayer.ReadAddressBase();
            int flags = Memory.Read<int>(_localPlayer.AddressBase + OffsetsLoader.C_BaseEntity.m_fFlags);

            // Check if the player is on the ground
            if (_key.state == Input.KeyState.DOWN && (flags & 1) == 0)
            {
                // If the player is not on the ground, force a jump
                Input.PressKey(Input.ScanCodeShort.LSHIFT);
                Thread.Sleep(10);
                Input.ReleaseKey(Input.ScanCodeShort.LSHIFT);
            }
        }

        private static Input.Key _key = null!;
        private static LocalPlayer _localPlayer = null!;
    }
}