using HarmonyLib;
using StationeersMods.Interface;

namespace BundledCables {
  class BundledCables : ModBehaviour {
    public override void OnLoaded(ContentHandler contentHandler) {
      //READ THE README FIRST!

      // Harmony harmony = new Harmony("BundledCables");
      // harmony.PatchAll();
      UnityEngine.Debug.Log("BundledCables Loaded!");
    }
  }
}