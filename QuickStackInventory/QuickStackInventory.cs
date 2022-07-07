using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickStackInventory : Mod
{
    public void Start()
    {
        Debug.Log("Mod QuickStackInventory has been loaded!");
    }
    public void Update()
    {


        if (CanvasHelper.ActiveMenu == MenuType.Inventory && Input.GetKeyDown(KeyCode.Y))
        {

            Network_Player player = RAPI.GetLocalPlayer();
            if (player != null && player.Inventory.secondInventory != null)
            {

                QuickStackHelper(player);
            }

        }
    }

    private void QuickStackHelper(Network_Player player)
    {
        PlayerInventory inv = player.Inventory;

        List<Slot> nonHotbar = inv.allSlots.GetRange(player.Inventory.hotslotCount, player.Inventory.allSlots.Count - player.Inventory.hotslotCount);

        Stack(nonHotbar, player.Inventory.secondInventory.allSlots);
    }

    private void Stack(List<Slot> nonHotbar, List<Slot> storageSlots)
    {



        foreach (Slot slot in nonHotbar)
        {
            if (!slot.IsEmpty && IsItemInStorage(slot.itemInstance, storageSlots))
            {
                // non stackable items
                if (!slot.itemInstance.settings_Inventory.Stackable)
                {
                    Slot s = FindEmptySlot(storageSlots);
                    if (s)
                    {
                        s.SetItem(slot.itemInstance);
                        slot.Reset();
                    }


                }
                //stackable items
                else
                {
                    int maxAmount = slot.itemInstance.settings_Inventory.StackSize;
                    // search storage for stacks
                    foreach (Slot storageSlot in storageSlots)
                    {
                        if (!storageSlot.IsEmpty && storageSlot.itemInstance.UniqueName == slot.itemInstance.UniqueName)
                        {
                            if (slot.itemInstance.Amount + storageSlot.itemInstance.Amount > maxAmount)
                            {

                                slot.itemInstance.Amount -= maxAmount - storageSlot.itemInstance.Amount;
                                //incase we moved whole stack
                                if (slot.itemInstance.Amount > 0)
                                {
                                    slot.RefreshComponents();
                                }
                                else
                                {
                                    slot.Reset();
                                }
                                storageSlot.itemInstance.Amount = maxAmount;
                                storageSlot.RefreshComponents();


                            }
                            else
                            {
                                storageSlot.itemInstance.Amount += slot.itemInstance.Amount;
                                storageSlot.RefreshComponents();
                                slot.Reset();
                                break;
                            }
                        }
                    }
                    //if all stacks are full find empty space to place it
                    if (!slot.IsEmpty)
                    {
                        Slot s = FindEmptySlot(storageSlots);
                        if (s)
                        {
                            s.SetItem(slot.itemInstance);
                            slot.Reset();
                        }
                    }


                }
            }
        }

    }
    // check if item is in storage
    public Boolean IsItemInStorage(ItemInstance item, List<Slot> storageSlots)
    {

        foreach (Slot slot in storageSlots)
        {

            if (!slot.IsEmpty && slot.itemInstance.UniqueName == item.UniqueName)
            {
                return true;
            }

        }
        return false;
    }
    // find next empty slot
    public Slot FindEmptySlot(List<Slot> inventory)
    {
        foreach (Slot item in inventory)
        {
            if (item.IsEmpty)
            {
                return item;
            }
        }
        return null;
    }
    public void OnModUnload()
    {
        Debug.Log("Mod QuickStackInventory has been unloaded!");
    }

}
