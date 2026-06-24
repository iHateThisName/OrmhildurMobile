using System;
using System.Collections.Generic;
using UnityEngine;
using Assets._Scripts.Utilities.Singleton;

public class InventoryManager : Singleton<InventoryManager>
{
    // Tracks how many charges each tool has left
    public Dictionary<EnumGridTool, int> ToolCharges { get; private set; } = new Dictionary<EnumGridTool, int>();

    // Fires whenever a tool is used or gained
    public static event Action<EnumGridTool, int> OnToolChargeChanged;

    public void InitializeLevelResources(int icePickCharges, int hammerCharges, int magGlassCharges)
    {
        ToolCharges.Clear();
        ToolCharges[EnumGridTool.IcePick] = icePickCharges;
        ToolCharges[EnumGridTool.Hammer] = hammerCharges;
        ToolCharges[EnumGridTool.MagnifyingGlass] = magGlassCharges;

        // Force UI to update immediately on level load
        OnToolChargeChanged?.Invoke(EnumGridTool.IcePick, icePickCharges);
        OnToolChargeChanged?.Invoke(EnumGridTool.Hammer, hammerCharges);
        OnToolChargeChanged?.Invoke(EnumGridTool.MagnifyingGlass, magGlassCharges);

        Debug.Log($"<color=green>[Inventory]</color> Level initialized: {icePickCharges} Picks, {hammerCharges} Hammers, {magGlassCharges} Scans.");
    }

    public bool TryConsumeToolCharge(EnumGridTool tool)
    {
        if (tool == EnumGridTool.Hand || tool == EnumGridTool.None) return true;

        if (ToolCharges.TryGetValue(tool, out int currentCharges) && currentCharges > 0)
        {
            ToolCharges[tool]--;
            OnToolChargeChanged?.Invoke(tool, ToolCharges[tool]);
            return true;
        }

        Debug.LogWarning($"<color=red>[Inventory]</color> Out of charges for {tool}!");
        return false;
    }

    // TreasureChestTile
    public void AddToolCharge(EnumGridTool tool, int amount)
    {
        if (tool == EnumGridTool.Hand || tool == EnumGridTool.None) return;

        if (ToolCharges.ContainsKey(tool))
        {
            ToolCharges[tool] += amount;
        }
        else
        {
            ToolCharges[tool] = amount;
        }

        OnToolChargeChanged?.Invoke(tool, ToolCharges[tool]);
        Debug.Log($"<color=gold>[Inventory]</color> Found {amount} {tool}(s)! Total is now {ToolCharges[tool]}.");
    }
}