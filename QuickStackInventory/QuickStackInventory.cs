
using System;
using System.Collections.Generic;
using UnityEngine;


public class QuickStackInventory : Mod
{

    public void Start()
    {
        Debug.Log("Mod QuickStackInventory has been loaded!");
    }

    public void OnModUnload()
    {
        Debug.Log("Mod QuickStackInventory has been unloaded!");
    }


    /*
     *  Check for transfer button being pressed.
     */

    public void Update()
    {

        if (CanvasHelper.ActiveMenu != MenuType.Inventory)
            return;

        if (Input.GetKeyDown(KeyCode.Y))
        {

            Network_Player player = RAPI.GetLocalPlayer();

            if (player == null)
                return;

            if (player.Inventory.secondInventory == null)
                return;

            transferItems(player);
        }
    }


    /*
     *  Transfer a players items into an opened storage container.
     */

    private void transferItems(Network_Player player)
    {

        PlayerInventory inventory = player.Inventory;

        int hotslots = inventory.hotslotCount;

        List<Slot> nonHotbar = inventory.allSlots
            .GetRange(hotslots, inventory.allSlots.Count - hotslots);

        transferInventory(nonHotbar, inventory.secondInventory.allSlots);
    }


    /*
     *  Transfer an inventory into a storage container.
     */

    private void transferInventory(List<Slot> inventory, List<Slot> storage)
    {

        foreach (Slot slot in inventory)
        {

            if (slot.IsEmpty)
                continue;

            var item = slot.itemInstance;

            if (IsItemInStorage(item, storage))
                if (item.settings_Inventory.Stackable)
                {

                    int maxAmount = item.settings_Inventory.StackSize;

                    // Search storage for stacks

                    foreach (Slot storageSlot in storage)
                    {

                        var stack = storageSlot.itemInstance;

                        if (storageSlot.IsEmpty)
                            continue;

                        if (stack.UniqueName != item.UniqueName)
                            continue;

                        if (item.Amount + stack.Amount > maxAmount)
                        {

                            item.Amount -= maxAmount - stack.Amount;

                            //  Incase we moved whole stack

                            if (item.Amount > 0)
                                slot.RefreshComponents();
                            else
                                slot.Reset();

                            stack.Amount = maxAmount;
                            storageSlot.RefreshComponents();

                        }
                        else
                        {

                            stack.Amount += item.Amount;
                            storageSlot.RefreshComponents();
                            slot.Reset();

                            break;
                        }
                    }


                    //If all stacks are full find empty space to place it

                    if (!slot.IsEmpty)
                    {

                        Slot empty = FindEmptySlot(storage);

                        if (empty)
                        {
                            empty.SetItem(item);
                            slot.Reset();
                        }
                    }

                }
                else
                {

                    //  Non stackable items

                    Slot empty = FindEmptySlot(storage);

                    if (empty)
                    {
                        empty.SetItem(item);
                        slot.Reset();
                    }
                }
        }
    }


    /*
     *  Check storage contains item
     */

    public Boolean IsItemInStorage(ItemInstance item, List<Slot> slots)
    {

        foreach (Slot slot in slots)
            if (!slot.IsEmpty && slot.itemInstance.UniqueName == item.UniqueName)
                return true;

        return false;
    }


    /*
     *  Find next empty slot
     */

    public Slot FindEmptySlot(List<Slot> inventory)
    {

        foreach (Slot item in inventory)
            if (item.IsEmpty)
                return item;

        return null;
    }
}
