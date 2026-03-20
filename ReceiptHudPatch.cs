using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShopReceipt;

[HarmonyPatch(typeof(RoundDirector))]
public static class ReceiptHudPatch
{
	public static GameObject? textObject;
	public static TextMeshProUGUI? textMesh;

	[HarmonyPostfix, HarmonyPatch(nameof(RoundDirector.Update))]
	public static void UpdateReceiptHud()
	{
		if (!SemiFunc.RunIsShop())
		{
			return;
		}

		bool extractionActive = RoundDirector.instance.extractionPointActive;
		bool showReceipt = extractionActive;

		if (textObject == null)
		{
			GameObject hud = GameObject.Find("Game Hud");
			GameObject tax = GameObject.Find("Tax Haul");

			if (hud == null || tax == null)
			{
				return;
			}

			textObject = new GameObject();
			textObject.SetActive(false);
			textObject.name = "Shop Receipt";
			textObject.AddComponent<TextMeshProUGUI>();
			textObject.transform.SetParent(hud.transform, false);

			ContentSizeFitter? sizeFitter = textObject.AddComponent<ContentSizeFitter>();
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

			textMesh = textObject.GetComponent<TextMeshProUGUI>();
			textMesh!.font = tax.GetComponent<TMP_Text>().font;
			textMesh.color = new Color(1f, 1f, 1f, 1f);
			textMesh.fontSize = 24f;
			textMesh.enableWordWrapping = false;
			textMesh.horizontalAlignment = HorizontalAlignmentOptions.Right;
			textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
			textMesh.alignment = TextAlignmentOptions.MidlineRight;

			RectTransform rect = textObject.GetComponent<RectTransform>();
			rect.pivot = new Vector2(1f, 0.5f);
			rect.anchorMin = new Vector2(0f, 0.5f);
			rect.anchorMax = new Vector2(1f, 0.5f);
			rect.anchoredPosition = new Vector2(0f, 0f);
		}

		if (textMesh == null)
		{
			return;
		}

		string message = showReceipt ? ShopReceiptLogic.GetExtractionReceipt() : "";
		bool hasMessage = message.Length > 0;

		textObject!.SetActive(hasMessage);

		if (hasMessage)
		{
			int lineCount = 1;
			for (int i = 0; i < message.Length; i++)
			{
				if (message[i] == '\n') lineCount++;				
			}

			int step = Mathf.Clamp((lineCount - 3) / 3, 0, 3);
			float[] sizes = { 24f, 18f, 14f, 11f };
			float fontSize = sizes[step];

			textMesh.SetText(message, true);
			textMesh.fontSize = fontSize;
			textMesh.lineSpacing = -fontSize * 2f;
		}
	}
}
