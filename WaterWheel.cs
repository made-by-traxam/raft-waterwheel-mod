using System;
using System.Collections;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using Steamworks;

[ModTitle("WaterWheel")]
[ModDescription("Waterwheels automatically collect water and fill it into nearby Purifiers.\n\nPlease visit https://raft-mods.trax.am/mods/waterwheel for updates and support.")]
[ModAuthor("traxam")]
[ModIconUrl("https://traxam.s-ul.eu/ICvBJvOy.png")]
[ModWallpaperUrl("https://traxam.s-ul.eu/gJaS13Dk.png")]
[ModVersionCheckUrl("https://raft-mods.trax.am/api/v1/mods/waterwheel/version.txt")]
[ModVersion("0.0.1")]
[RaftVersion("Update 9.05")]
[ModIsPermanent(true)]
public class WaterWheelMod : Mod
{   
    public IEnumerator Start()
    {
        RNotification notification = FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.spinning, "Loading WaterWheel...");

        var bundleLoadRequest = AssetBundle.LoadFromFileAsync("mods\\ModData\\WaterWheel\\waterwheel.assets");
        yield return bundleLoadRequest;

        AssetBundle assetBundle = bundleLoadRequest.assetBundle;
        if (assetBundle == null)
        {
            Error("Failed to load Asset Bundle. This mod will not work.");
            notification.Close();
            yield return null;
        }

        Item_Base placeable_testblock = assetBundle.LoadAsset<Item_Base>("Placeable_WaterWheel_Item");
        List<Item_Base> list = Traverse.Create(typeof(ItemManager)).Field("allAvailableItems").GetValue<List<Item_Base>>();
        list.Add(placeable_testblock);
        GameObject prefab = placeable_testblock.settings_buildable.GetBlockPrefab(0).gameObject;
        prefab.transform.Find("_waterTargetCollider").gameObject.AddComponent<WaterWheelTarget>();
        Traverse.Create(typeof(ItemManager)).Field("allAvailableItems").SetValue(list);

        List<Item_Base> sobject = Traverse.Create(Resources.Load<ScriptableObject>("blockquadtype/quad_foundation_empty")).Field("acceptableBlockTypes").GetValue<List<Item_Base>>();
        sobject.Add(placeable_testblock);
        Traverse.Create(Resources.Load<ScriptableObject>("blockquadtype/quad_foundation_empty")).Field("acceptableBlockTypes").SetValue(sobject);
        notification.Close();
        Info("Mod was loaded successfully!");
        FollowUpLog("This mod is still in development, please make a backup before you use it!");
    }
    
    public void OnModUnload()
    {
        Error("This mod can not be unloaded!");
    }

    private void Info(string message) {
        RConsole.Log("<color=#3498db>[info]</color>\t<b>traxam's WaterWheels:</b> " + message);
    }

    private void FollowUpLog(string message) {
        RConsole.Log("\t" + message);
    }

    private void Error(string message) {
        RConsole.LogError("<color=#e74c3c>[error]</color>\t<b>traxam's WaterWheels:</b> " + message);
    }
}

public class WaterWheelTarget : MonoBehaviour {
    private float timer = 0;
    public float timePerFill = 20;
    public void OnTriggerStay(Collider other) {
        CookingStand_Purifier target = other.gameObject.GetComponent<CookingStand_Purifier>();
        if (target == null) {
            target = other.gameObject.transform.parent.gameObject.GetComponent<CookingStand_Purifier>();
        }
        if (target != null) {

            timer += Time.deltaTime;
            if (timer > timePerFill) {
                Item_Base waterCup = ItemManager.GetItemByIndex(40);
                CookingSlot[] possibleSlots = target.GetCookingSlotsForItem(waterCup);
                if (possibleSlots != null && possibleSlots.Length > 0) {
                    Network_Player networkPlayer = ComponentManager<Network_Player>.Value;
                    CookingStandManager cookingStandManager = networkPlayer.CookingStandManager;
                    Message_CookingStand_Insert message = new Message_CookingStand_Insert(Messages.CookingStandManager_InsertItem, cookingStandManager, waterCup, target, possibleSlots);
                    if (Semih_Network.IsHost)
                    {
                        networkPlayer.Network.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                        target.InsertItem(waterCup, possibleSlots, false); // if localPlayer was true, the local player would lose an item
                    } else {
                        message.emptySlotIndexes = null;
                        networkPlayer.Network.SendP2P(networkPlayer.Network.HostID, message, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                    }
                }
                timer -= timePerFill;
            }
        }
    }
}
