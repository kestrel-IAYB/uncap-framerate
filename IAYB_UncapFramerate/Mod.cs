using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;

namespace IAYB_UncapFramerate
{
    [BepInPlugin(pluginGuid, pluginName, pluginversion)]
    public class Mod : BaseUnityPlugin
    {
        public const string pluginGuid = "kestrel.iamyourbeast.uncapframerate";
        public const string pluginName = "Uncap Framerate";
        public const string pluginversion = "1.0.1";

        public static ConfigEntry<ulong> minFps;
        public static ConfigEntry<ulong> maxFps;

        public void Awake() {

            minFps = Config.Bind("Options", "Minimum Framerate", 10ul, "The minimum framerate shown on the slider (minimum value: 1)");
            maxFps = Config.Bind("Options", "Maximum Framerate", 240ul, "The maximum framerate shown on the slider");

            Logger.LogInfo("Hiiiiiiiiiiii :3");
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }
    }

    //Make SaveSettings use the raw value from sliderFramerate rather than adjusting for the weird slider scale
    [HarmonyPatch(typeof(UISettingsSubMenuVisual), nameof(UISettingsSubMenuVisual.SaveSettings))]
    public class PatchVisualSave
    {
        [HarmonyPostfix]
        public static void Postfix(UISettingsOptionSlider ___sliderFramerate) {
            GameManager.instance.saveManager.CurrentSettings.FramerateCap = Mathf.RoundToInt(___sliderFramerate.GetValue());
        }
    }

    //Make RevertSettings use the raw value from sliderFramerate rather than adjusting for the weird slider scale
    [HarmonyPatch(typeof(UISettingsSubMenuVisual), nameof(UISettingsSubMenuVisual.RevertSettings))]
    public class PatchVisualRevert
    {
        [HarmonyPostfix]
        public static void Postfix(UISettingsOptionSlider ___sliderFramerate) {
            ___sliderFramerate.SetValue(GameManager.instance.saveManager.CurrentSettings.FramerateCap);
        }
    }



    //On start, set value of slider to -1 (special value to indicate slider needs to be adjusted)
    [HarmonyPatch(typeof(UISettingsSubMenuVisual), "Start")]
    public class PatchVisualStart
    {
        [HarmonyPostfix]
        public static void Postfix(UISettingsOptionSlider ___sliderFramerate) {
            ___sliderFramerate.SetValue(-1);
            ___sliderFramerate.SetValue(GameManager.instance.saveManager.CurrentSettings.FramerateCap); //clean up after ourselves
        }
    }

    //Adjust slider if -1 is passed to SetValue
    [HarmonyPatch(typeof(UISettingsOptionSlider), nameof(UISettingsOptionSlider.SetValue))]
    public class PatchSlider
    {
        [HarmonyPrefix]
        public static void Prefix(float value, ref UnityEngine.UI.Slider ___slider, ref float ___valueMultiplier) {

            if (value == -1) {
                ___valueMultiplier = 1;
                ___slider.minValue = Mathf.Clamp(Mod.minFps.Value, 1, Mod.maxFps.Value);
                ___slider.maxValue = Mod.maxFps.Value;
            }
        }
    }
}
