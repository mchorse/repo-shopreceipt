using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShopReceipt;

public static class ShopReceiptLogic
{
	private static readonly Color ColorCheap = new Color(0x22 / 255f, 1f, 0f);
	private static readonly Color ColorMid   = new Color(1f, 0xdd / 255f, 0f);
	private static readonly Color ColorExpensive = new Color(1f, 0f, 0x22 / 255f);
	
	public static string GetExtractionReceipt()
	{
		if (!SemiFunc.RunIsShop() || ShopManager.instance == null)
		{
			return "";
		}

		var list = ShopManager.instance.shoppingList;
		
		if (list == null || list.Count == 0)
		{
			return "";			
		}
		
		Dictionary<string, (int count, int totalPrice)> byName = new Dictionary<string, (int, int)>();

		foreach (ItemAttributes attrs in list)
		{
			if (attrs == null || attrs.roomVolumeCheck == null)
			{
				continue;
			}
				
			attrs.roomVolumeCheck.CheckSet();

			if (!attrs.roomVolumeCheck.inExtractionPoint)
			{
				continue;
			}

			string name = attrs.itemName ?? attrs.item?.name ?? "?";
			int value = attrs.value;

			if (byName.TryGetValue(name, out var existing))
			{
				byName[name] = (existing.count + 1, existing.totalPrice + value);
			}
			else
			{
				byName[name] = (1, value);
			}
		}

		if (byName.Count == 0)
		{
			return "";
		}

		var ordered = byName.OrderByDescending(kv => kv.Value.totalPrice).ToList();

		int minPrice = ordered.Min(kv => kv.Value.totalPrice);
		int maxPrice = ordered.Max(kv => kv.Value.totalPrice);
		int range = maxPrice - minPrice;

		IEnumerable<string> lines = ordered.Select(kv =>
		{
			(int count, int totalPrice) = kv.Value;
			string amount = count == 1 ? "" : " x " + count;
			string priceStr = "$" + SemiFunc.DollarGetString(totalPrice);
			float t = range > 0 ? (float)(totalPrice - minPrice) / range : 1f;
			string colorHex = PriceGradientHex(t);
			
			return $"{kv.Key}{amount} - <color=#{colorHex}>{priceStr}K</color>";
		});

		return string.Join("\n", lines);
	}
	
	private static string PriceGradientHex(float t)
	{
		t = Mathf.Clamp01(t);
		Color c = t <= 0.5f
			? Color.Lerp(ColorCheap, ColorMid, t * 2f)
			: Color.Lerp(ColorMid, ColorExpensive, (t - 0.5f) * 2f);
		
		return ColorUtility.ToHtmlStringRGB(c);
	}
}
