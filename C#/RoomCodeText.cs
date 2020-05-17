using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000806 RID: 2054
public class RoomCodeText : MonoBehaviour
{
	// Token: 0x06003C31 RID: 15409 RVA: 0x00035E13 File Offset: 0x00034013
	private void Start()
	{
		base.StartCoroutine(this.Loop());
		base.StartCoroutine(this.ModLoop());
	}

	// Token: 0x06003C32 RID: 15410 RVA: 0x00035E2F File Offset: 0x0003402F
	private IEnumerator Loop()
	{
		for (;;)
		{
			yield return Yielders.Seconds(10f);
			this._RoomCodeText.DOFade(0f, 1f);
			yield return Yielders.Seconds(1f);
			this._RoomCodeText.text = "wordsgame.lol";
			this._RoomCodeText.DOFade(1f, 1f);
			yield return Yielders.Seconds(6f);
			this._RoomCodeText.DOFade(0f, 1f);
			yield return Yielders.Seconds(1f);
			this._RoomCodeText.text = "room code";
			this._RoomCodeText.DOFade(1f, 1f);
			yield return Yielders.Seconds(1f);
		}
		yield break;
	}

	// Token: 0x06003C33 RID: 15411 RVA: 0x0013BD7C File Offset: 0x00139F7C
	private void CreateModWarning()
	{
		if (GameObject.Find("Mod warning") == null){

			GameObject modWarning = new GameObject("Mod warning");

			modWarning.transform.SetParent(this.transform);

			modWarning.AddComponent<CanvasRenderer>();
			RectTransform modWarningRT = modWarning.AddComponent<RectTransform>();
			Image modWarningImage = modWarning.AddComponent<Image>();

			modWarningRT.position = base.gameObject.GetComponent<RectTransform>().position;
			modWarningRT.position = new Vector3(modWarningRT.position.x - 225f, modWarningRT.position.y - 215f, modWarningRT.position.z);
			modWarningRT.sizeDelta = new Vector2(220f, 220f);
			modWarningRT.Rotate(new Vector3(25f, 0f, 0f));

			byte[] data = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/ModData/modWarning");
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
			Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), 100f);

			modWarningImage.sprite = sprite;
			modWarningImage.preserveAspect = true;

			GameObject modWarningText = new GameObject("Mod warning text");
			modWarningText.transform.SetParent(this.transform);

			RectTransform modWarningTextRT = modWarningText.AddComponent<RectTransform>();
			this._WarningTxt = modWarningText.AddComponent<TextMeshProUGUI>();
			modWarningText.AddComponent<CanvasRenderer>();
			RectTransform rcTxtRect = base.GetComponent<RectTransform>();

			modWarningTextRT.position = new Vector3(rcTxtRect.position.x - 225f, rcTxtRect.position.y - 210f, rcTxtRect.position.z);

			this._WarningTxt.font = this._RoomCodeText.font;
			this._WarningTxt.fontSize = this._RoomCodeText.fontSize - 12.5f;
			this._WarningTxt.alignment = this._RoomCodeText.alignment;
			this._WarningTxt.text = "MODDED\nGAME";
			this._WarningTxt.lineSpacing = -5f;
			}
	}

	// Token: 0x06003C34 RID: 15412 RVA: 0x00035E3E File Offset: 0x0003403E
	private IEnumerator ModLoop()
	{
		this.CreateModWarning();
		yield return Yielders.Seconds(5f);
		for (;;)
		{
			yield return Yielders.Seconds(10f);
			this._WarningTxt.DOFade(0f, 1f);
			yield return Yielders.Seconds(1f);
			this._WarningTxt.text = "UNOFFICIAL\nCONTENT";
			this._WarningTxt.DOFade(1f, 1f);
			yield return Yielders.Seconds(6f);
			this._WarningTxt.DOFade(0f, 1f);
			yield return Yielders.Seconds(1f);
			this._WarningTxt.text = "MODDED\nGAME";
			this._WarningTxt.DOFade(1f, 1f);
			yield return Yielders.Seconds(1f);
		}
		yield break;
	}

	// Token: 0x0400276A RID: 10090
	[SerializeField]
	private TextMeshProUGUI _RoomCodeText;

	// Token: 0x0400276B RID: 10091
	private TextMeshProUGUI _WarningTxt;
}
