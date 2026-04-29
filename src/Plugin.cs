using System;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;

namespace NoNegativeInterest
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log = null!;

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        // XOR EAX, EAX; RET — return false (bool)
        private static readonly byte[] ReturnFalse = { 0x31, 0xC0, 0xC3 };
        // RET — return void / early exit
        private static readonly byte[] ReturnVoid  = { 0xC3 };

        // RVAs of BankHelper method bodies in GameAssembly.dll, confirmed via static PE analysis.
        // If the game updates and these shift, the expected-bytes check below will catch it.
        private const long RVA_RunHourly                           = 0xBA21A0;
        private const long RVA_ShouldPerformNegativeInterestCharge = 0xBA2460;
        private const long RVA_PerformNegativeInterestCharge       = 0xBA2560;

        // First 4 bytes we expect at each site (IL2CPP sub rsp prologue).
        // If the game updates and these change, we refuse to patch rather than corrupting unknown code.
        private static readonly byte[] Expected_RunHourly                           = { 0x48, 0x81, 0xEC, 0xC8 };
        private static readonly byte[] Expected_ShouldPerformNegativeInterestCharge = { 0x48, 0x83, 0xEC, 0x28 };
        private static readonly byte[] Expected_PerformNegativeInterestCharge       = { 0x48, 0x81, 0xEC, 0xC8 };

        public override void Load()
        {
            Log = base.Log;
            try { PatchMethods(); }
            catch (Exception ex) { Log.LogError($"No Negative Interest failed: {ex}"); }
        }

        private static unsafe void PatchMethods()
        {
            IntPtr gameBase = GetModuleHandle("GameAssembly.dll");
            Log.LogInfo($"GameAssembly.dll loaded at 0x{gameBase:X}");

            bool ok = true;
            ok &= PatchRva(gameBase, RVA_RunHourly,                           ReturnVoid,  "RunHourly",                           Expected_RunHourly);
            ok &= PatchRva(gameBase, RVA_ShouldPerformNegativeInterestCharge, ReturnFalse, "ShouldPerformNegativeInterestCharge", Expected_ShouldPerformNegativeInterestCharge);
            ok &= PatchRva(gameBase, RVA_PerformNegativeInterestCharge,       ReturnVoid,  "PerformNegativeInterestCharge",       Expected_PerformNegativeInterestCharge);

            if (ok)
                Log.LogInfo("All patches applied — bank will never charge negative interest.");
            else
                Log.LogWarning("One or more patches were skipped because the game was updated. " +
                               "Check for a mod update at the workshop page.");
        }

        private static unsafe bool PatchRva(IntPtr gameBase, long rva, byte[] stub, string name, byte[] expected)
        {
            IntPtr fp = (IntPtr)((long)gameBase + rva);
            byte*  p  = (byte*)fp.ToPointer();

            Log.LogInfo($"{name}: RVA=0x{rva:X}  abs=0x{fp:X}  before=[{p[0]:X2} {p[1]:X2} {p[2]:X2} {p[3]:X2} {p[4]:X2} {p[5]:X2} {p[6]:X2} {p[7]:X2}]");

            // Verify we're looking at the right function before touching anything.
            for (int k = 0; k < expected.Length; k++)
            {
                if (p[k] != expected[k])
                {
                    Log.LogWarning($"{name}: expected byte[{k}]=0x{expected[k]:X2} but found 0x{p[k]:X2} — game may have updated, skipping patch.");
                    return false;
                }
            }

            if (!VirtualProtect(fp, (UIntPtr)stub.Length, PAGE_EXECUTE_READWRITE, out uint old))
            {
                Log.LogError($"{name}: VirtualProtect failed");
                return false;
            }
            for (int j = 0; j < stub.Length; j++) p[j] = stub[j];
            VirtualProtect(fp, (UIntPtr)stub.Length, old, out _);

            Log.LogInfo($"{name}: after=[{p[0]:X2} {p[1]:X2} {p[2]:X2} {p[3]:X2}]  OK");
            return true;
        }
    }
}
