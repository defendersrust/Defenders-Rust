﻿using System;
using Random = UnityEngine.Random;

namespace Oxide.Plugins
{
    [Info("QuickSmelt", "Wulf/lukespragg", "1.1.0", ResourceId = 1067)]
    [Description("Increases the speed of the furnace smelting.")]

    class QuickSmelt : RustPlugin
    {
        // Do NOT edit this file, instead edit QuickSmelt.json in server\<identity>\oxide\config

        public float ChancePerConsumption => Config.Get<float>("ChancePerConsumption");
        public float CharcoalChanceModifier => Config.Get<float>("CharcoalChanceModifier");
        public float CharcoalProductionModifier => Config.Get<float>("CharcoalProductionModifier");
        public bool DontOvercookMeat => Config.Get<bool>("DontOvercookMeat");
        public float ProductionModifier => Config.Get<float>("ProductionModifier");

        protected override void LoadDefaultConfig()
        {
            // This is *roughly* x2 production rate
            Config["ChancePerConsumption"] = 0.5f;
            Config["CharcoalChanceModifier"] = 1.5f;
            Config["CharcoalProductionModifier"] = 1f;
            Config["DontOvercookMeat"] = true;
            Config["ProductionModifier"] = 1f;

            SaveConfig();
        }

        void OnConsumeFuel(BaseOven oven, Item fuel, ItemModBurnable burnable)
        {
            if (oven == null) return;

            var byproductChance = burnable.byproductChance * CharcoalChanceModifier;

            if (oven.allowByproductCreation && burnable.byproductItem != null && Random.Range(0.0f, 1f) <= byproductChance)
            {
                var obj = ItemManager.Create(burnable.byproductItem, (int)Math.Round(burnable.byproductAmount * CharcoalProductionModifier));
                if (!obj.MoveToContainer(oven.inventory))
                    obj.Drop(oven.inventory.dropPosition, oven.inventory.dropVelocity);
            }

            for (var i = 0; i < oven.inventorySlots; i++)
            {
                try
                {
                    var slotItem = oven.inventory.GetSlot(i);
                    if (slotItem == null || !slotItem.IsValid()) continue;

                    var cookable = slotItem.info.GetComponent<ItemModCookable>();
                    if (cookable == null) continue;

                    if (cookable.becomeOnCooked.category == ItemCategory.Food &&
                        slotItem.info.shortname.Trim().EndsWith(".cooked") && DontOvercookMeat) continue;

                    // The chance of consumption is going to result in a 1 or 0
                    var consumptionAmount = (int)Math.Ceiling(ProductionModifier * (Random.Range(0f, 1f) <= ChancePerConsumption ? 1 : 0));

                    // Check how many are actually in the furnace, before we try removing too many
                    var inFurnaceAmount = slotItem.amount;
                    if (inFurnaceAmount < consumptionAmount)
                        consumptionAmount = inFurnaceAmount;

                    // Set consumption to however many we can pull from this actual stack
                    consumptionAmount = TakeFromInventorySlot(oven.inventory, slotItem.info.itemid, consumptionAmount, i);

                    // If we took nothing, then... we can't create any
                    if (consumptionAmount <= 0) continue;

                    // Create the item(s) that are now smelted
                    var smeltedItem = ItemManager.Create(cookable.becomeOnCooked, cookable.amountOfBecome * consumptionAmount);
                    if (!smeltedItem.MoveToContainer(oven.inventory))
                        smeltedItem.Drop(oven.inventory.dropPosition, oven.inventory.dropVelocity);
                }
                catch (InvalidOperationException) {}
            }
        }

        static int TakeFromInventorySlot(ItemContainer container, int itemId, int amount, int slot)
        {
            var item = container.GetSlot(slot);
            if (item.info.itemid != itemId || item.IsBlueprint()) return 0;

            if (item.amount > amount)
            {
                item.MarkDirty();
                item.amount -= amount;
                return amount;
            }

            amount = item.amount;
            item.RemoveFromContainer();
            return amount;
        }
    }
}
