using System;
using HarmonyLib;
using StationeersMods.Interface;

namespace BundledCables {
  public class BundledCables : ModBehaviour {
    public override void OnLoaded(ContentHandler contentHandler) {
      UnityEngine.Debug.Log("Hello World!");
      Harmony harmony = new Harmony("BundledCables");
      harmony.PatchAll();
    }
  }
}