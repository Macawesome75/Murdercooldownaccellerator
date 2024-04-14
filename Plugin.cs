using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using m75murdercooldownplugin;
using Rewired.Demos;
using SOD.Common.BepInEx;
using SOD.Common;
using BepInEx.Configuration;
using UnityEngine;
using Il2CppSystem;
using System.Runtime.CompilerServices;


namespace Murdermore;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class M75_Murderplugin : BasePlugin
{
    public static ConfigEntry<float> configMurModifier;
    public static ConfigEntry<bool> configMurIsRand;
    public static ConfigEntry<float> configMurModMin;
    public static ConfigEntry<float> configMurModMax;
    public override void Load()
	{
		// Plugin startup logic
		//Logger = BepInEx.Logging.Logger.CreateLogSource("KILL KILL KILL");
		Log.LogInfo($"Murder cooldown mod loaded");
        M75_Murderplugin.configMurModifier = Config.Bind("General", "MurModifier", 1f, "This number will be added to the murder cooldown timer, higher numbers lowers the cooldown. A value of 1 will double the murder cooldown speed, a value of 0 will leave the default speed.\nNegative numbers slow down the timer, a value of -0.5 will halve the cooldown speed, and a value of -0.8 will slow the cooldown by 5 times. \nDefault murder timer is 12 hours, but may differ based on murder type\nNote that murders take time, so larger maps will take longer regardless.");

        M75_Murderplugin.configMurIsRand = Config.Bind("Random", "MurIsRand", false, "If set to true, the modifier will be set randomly between the two below settings, changing every murder.");
        M75_Murderplugin.configMurModMin = Config.Bind("Random", "MurModMin", -0.5f, "If random mode is on, the minimum range of the randomness.");
        M75_Murderplugin.configMurModMax = Config.Bind("Random", "MurModMax", 0.5f, "If random mode is on, the maximum range of the randomness.");
        var harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}");
		harmony.PatchAll();
	}
}
[HarmonyPatch(typeof(MurderController),"Tick")]
public class M75_MurderController_Patch
{
    static float murMult = M75_Murderplugin.configMurModifier.Value;
    static bool randomMode = M75_Murderplugin.configMurIsRand.Value;
    static float murRandMin = M75_Murderplugin.configMurModMin.Value;
    static float murRandMax = M75_Murderplugin.configMurModMax.Value;
    static string citySeed = CityData.Instance.seed;
    public static void Postfix(float timePassed)
	{
        float GetRandMurModifier() //I dont know what im doing
        {
            int murdersSoFar = MurderController.Instance.activeMurders.Count + MurderController.Instance.inactiveMurders.Count;
            float finalValue = Toolbox.Instance.GetPsuedoRandomNumber(murRandMin, murRandMax, (citySeed + murdersSoFar.ToString()), true);
            //SOD.Common.Plugin.Log.LogInfo("Chosen modifier" + finalValue);
            return finalValue;
        }
        if (MurderController.Instance.pauseBetweenMurders >= 0f)
		{
            if (!randomMode) { MurderController.Instance.pauseBetweenMurders -= timePassed * murMult; }
            if (randomMode) { MurderController.Instance.pauseBetweenMurders -= timePassed * GetRandMurModifier(); }
            //SOD.Common.Plugin.Log.LogInfo("Murder Timer is" + MurderController.Instance.pauseBetweenMurders); //Debug
		}
    }
}


/*[HarmonyPatch(typeof(MurderController.Murder), "SetMurderState")]
public class M75_MurderController_Murder_Patch
{
    public static void Prefix(MurderController.MurderState newState)
    {
        SOD.Common.Plugin.Log.LogInfo("Murder State is" + newState.ToString()); //Debug
    }
}*/
