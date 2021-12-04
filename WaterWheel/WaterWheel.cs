using System.Collections;
using Steamworks;
using UnityEngine;

namespace WaterWheel
{
    public class WaterWheelMod : Mod
    {
        public IEnumerator Start()
        {
            var notification = FindObjectOfType<HNotify>()
                .AddNotification(HNotify.NotificationType.spinning, "Loading WaterWheel...");
            var bundleLoadRequest = AssetBundle.LoadFromMemoryAsync(GetEmbeddedFileBytes("waterwheel.assets"));
            yield return bundleLoadRequest;

            AssetBundle assetBundle = bundleLoadRequest.assetBundle;
            if (assetBundle == null)
            {
                LogError("Failed to load Asset Bundle. This mod will not work.");
                notification.Close();
                yield return null;
            }

            Item_Base waterWheelItem = assetBundle.LoadAsset<Item_Base>("Placeable_WaterWheel_Item");
            Material colorMat = ItemManager.GetItemByName("Block_Wall_Fence_Plank").settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Renderer>().material;
            waterWheelItem.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Renderer>().material = colorMat;

            GameObject prefab = waterWheelItem.settings_buildable.GetBlockPrefab(0).gameObject;
            prefab.transform.Find("_waterTargetCollider").gameObject.AddComponent<WaterWheelTarget>();
            prefab.transform.Find("_model").Find("wheel").gameObject.AddComponent<WaterWheelRotor>();

            RAPI.AddItemToBlockQuadType(waterWheelItem, RBlockQuadType.quad_foundation_empty);
            RAPI.RegisterItem(waterWheelItem);

            notification.Close();
            Info("Mod was loaded successfully!");
            FollowUpLog("This mod is still in development, please make a backup before you use it!");
        }

        public void OnModUnload()
        {
            LogError("This mod can not be unloaded!");
        }

        private static void Info(string message)
        {
            Debug.Log("<color=#3498db>[info]</color>\t<b>traxam's WaterWheels:</b> " + message);
        }

        private static void FollowUpLog(string message)
        {
            Debug.Log("\t" + message);
        }

        private static void LogError(string message)
        {
            Debug.LogError("<color=#e74c3c>[error]</color>\t<b>traxam's WaterWheels:</b> " + message);
        }
    }

    public class WaterWheelTarget : MonoBehaviour
    {
        private float _timer;
        public float timePerFill = 20;

        public void OnTriggerStay(Collider other)
        {
            CookingStand_Purifier target = other.gameObject.GetComponent<CookingStand_Purifier>();
            if (target == null)
            {
                target = other.gameObject.transform.parent.gameObject.GetComponent<CookingStand_Purifier>();
            }

            if (target != null)
            {
                _timer += Time.deltaTime;
                if (_timer > timePerFill)
                {
                    Item_Base waterCup = ItemManager.GetItemByIndex(40);
                    CookingSlot[] possibleSlots = target.GetCookingSlotsForItem(waterCup);
                    if (possibleSlots != null && possibleSlots.Length > 0)
                    {
                        Network_Player networkPlayer = ComponentManager<Network_Player>.Value;
                        CookingStandManager cookingStandManager = networkPlayer.CookingStandManager;
                        Message_CookingStand_Insert message = new Message_CookingStand_Insert(
                            Messages.CookingStandManager_InsertItem, cookingStandManager, waterCup, target,
                            possibleSlots);
                        if (Semih_Network.IsHost)
                        {
                            networkPlayer.Network.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable,
                                NetworkChannel.Channel_Game);
                            target.InsertItem(waterCup, possibleSlots,
                                false); // if localPlayer was true, the local player would lose an item
                        }
                    }

                    _timer -= timePerFill;
                }
            }
        }
    }

    public class WaterWheelRotor : MonoBehaviour
    {
        public void Update()
        {
            transform.Rotate(Vector3.right * (Time.deltaTime * -15));
        }
    }
}