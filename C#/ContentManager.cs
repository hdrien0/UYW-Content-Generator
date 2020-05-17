using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine;

// Token: 0x0200073E RID: 1854
public class ContentManager : Singleton<ContentManager>
{
	// Token: 0x17000AA8 RID: 2728
	// (get) Token: 0x0600377B RID: 14203 RVA: 0x00033BDE File Offset: 0x00031DDE
	public List<ContentManager.Package> Packages
	{
		get
		{
			return new List<ContentManager.Package>(this._Packages);
		}
	}

	// Token: 0x17000AA9 RID: 2729
	// (get) Token: 0x0600377C RID: 14204 RVA: 0x00033BEB File Offset: 0x00031DEB
	public List<ContentManager.Content> AvailableContent
	{
		get
		{
			return this._AvailableContent;
		}
	}

	// Token: 0x17000AAA RID: 2730
	// (get) Token: 0x0600377D RID: 14205 RVA: 0x0012AFB8 File Offset: 0x001291B8
	public List<ContentManager.Content> AllContent
	{
		get
		{
			List<ContentManager.Content> list = new List<ContentManager.Content>();
			for (int i = 0; i < this._Packages.Count; i++)
			{
				list.AddRange(this._Packages[i].Content);
			}
			return list;
		}
	}

	// Token: 0x0600377E RID: 14206 RVA: 0x0012B000 File Offset: 0x00129200
	private void Start()
	{
		Singleton<DLCManager>.Instance.OnDLCPurchased += this.HandleDLCPurchased;
		UnityEngine.Random.InitState(DateTime.Now.Millisecond);
		this.ReloadContent();
	}

	// Token: 0x0600377F RID: 14207 RVA: 0x00033BF3 File Offset: 0x00031DF3
	protected override void OnWillBeDestroyed()
	{
		base.OnWillBeDestroyed();
		Singleton<DLCManager>.Instance.OnDLCPurchased -= this.HandleDLCPurchased;
	}

	// Token: 0x06003780 RID: 14208 RVA: 0x0012B03C File Offset: 0x0012923C
	public void ReloadContent()
	{
		string newContentJsonPath = Application.persistentDataPath + "/NewContent/nc.json";
		bool newContent = File.Exists(newContentJsonPath);
		JsonData newJsonData = null;
		if (newContent){
			newJsonData = JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/NewContent/nc.json"));
		}

		JsonData jsonData = JsonMapper.ToObject(Resources.Load<TextAsset>("Content/Manifest/Manifest").text)["packages"];
		this._AvailableContent = new List<ContentManager.Content>();
		this._Packages = new List<ContentManager.Package>();
		this._UsedContent = new List<ContentManager.Content>();
		if (Singleton<GameManager>.Instance.CurrentLanguage == GameManager.LanguageSettings.English)
		{
			for (int i = 0; i < jsonData.Count; i++)
			{
				if (newContent){
					for (int z = 0; z < newJsonData["remove"].Count; z++)
					{
						jsonData[i][(string)newJsonData["remove"][z]].Clear();
					}
				}

				ContentManager.Package package = new ContentManager.Package();
				package.LoadFromJSON(jsonData[i], false);
				this._Packages.Add(package);
				if (package.ID.Equals("base") || package.ID.Equals("addon") || Singleton<DLCManager>.Instance.IsDLCPurchased(package.ID))
				{
					this._AvailableContent.AddRange(package.Content);
				}
			}
		}

		jsonData = JsonMapper.ToObject(Resources.Load<TextAsset>("Content/Manifest/Manifest_Addon").text)["packages"];
		for (int j = 0; j < jsonData.Count; j++)
		{
			if (newContent){
				for (int k = 0; k < newJsonData["remove"].Count; k++)
				{
					jsonData[j][(string)newJsonData["remove"][k]].Clear();
				}
			}
			
			ContentManager.Package package2 = new ContentManager.Package();
			package2.LoadFromJSON(jsonData[j], true);
			this._Packages.Add(package2);
			this._AvailableContent.AddRange(package2.Content);
		}
		if (newContent){
			JsonData jsonData3 = newJsonData["packages"];
			for (int l = 0; l < jsonData3.Count; l++)
			{
				ContentManager.Package package3 = new ContentManager.Package();
				package3.LoadFromJSON(jsonData3[l], true);
				this._Packages.Add(package3);
				this._AvailableContent.AddRange(package3.Content);
			}
		}
		UnityEngine.Debug.Log("[ContentManager] Content Reloaded!");
		base.StartCoroutine(this.LoadUsedContent());
	}

	// Token: 0x06003781 RID: 14209 RVA: 0x0012B2CC File Offset: 0x001294CC
	public void ExcludeContent(string manifest)
	{
		JsonData jsonData = JsonMapper.ToObject(manifest);
		JsonData jsonData2 = jsonData["subTheTitle"];
		if (jsonData2.IsArray)
		{
			for (int i = 0; i < jsonData2.Count; i++)
			{
				this.ExcludeContent(ContentManager.GameType.SubTheTitle, (int)jsonData2[i]);
			}
		}
		JsonData jsonData3 = jsonData["extraExtra"];
		if (jsonData3.IsArray)
		{
			for (int j = 0; j < jsonData3.Count; j++)
			{
				this.ExcludeContent(ContentManager.GameType.ExtraExtra, (int)jsonData3[j]);
			}
		}
		JsonData jsonData4 = jsonData["blank-o-matic"];
		if (jsonData4.IsArray)
		{
			for (int k = 0; k < jsonData4.Count; k++)
			{
				this.ExcludeContent(ContentManager.GameType.BlankoMatic, (int)jsonData4[k]);
			}
		}
		JsonData jsonData5 = jsonData["bookIt"];
		if (jsonData5.IsArray)
		{
			for (int l = 0; l < jsonData5.Count; l++)
			{
				this.ExcludeContent(ContentManager.GameType.BookIt, (int)jsonData5[l]);
			}
		}
		JsonData jsonData6 = jsonData["surveySays"];
		if (jsonData6.IsArray)
		{
			for (int m = 0; m < jsonData6.Count; m++)
			{
				this.ExcludeContent(ContentManager.GameType.SurveySays, (int)jsonData6[m]);
			}
		}
	}

	// Token: 0x06003782 RID: 14210 RVA: 0x0012B448 File Offset: 0x00129648
	public ContentManager.Content FindRandomContent(ContentManager.GameType gameType)
	{
		if (Singleton<GameManager>.Instance.CurrentMode == GameManager.GameMode.Debug)
		{
			return Singleton<GameManager>.Instance.ContentOverride;
		}
		Debug.Log("ContentManager.cs : 367");
		List<ContentManager.Content> list = this._AvailableContent.FindAll((ContentManager.Content x) => x.GameType == gameType && (!Singleton<GameManager>.Instance.FamilyMode || x.FamilyMode));
		Debug.Log("ContentManager.cs : 370");
		if (list.Count == 0)
		{
			Debug.Log("ContentManager.cs : 375");
			List<ContentManager.Content> collection = this._UsedContent.FindAll((ContentManager.Content x) => x.GameType == gameType && (!Singleton<GameManager>.Instance.FamilyMode || x.FamilyMode));
			Debug.Log("ContentManager.cs : 378");
			list.AddRange(collection);
			Debug.Log("ContentManager.cs : 381");
			this._AvailableContent.AddRange(this._UsedContent.FindAll((ContentManager.Content x) => x.GameType == gameType));
			Debug.Log("ContentManager.cs : 384");
			this._UsedContent.RemoveAll((ContentManager.Content x) => x.GameType == gameType);
		}
		Debug.Log("ContentManager.cs : 388");
		Debug.Log("ContentManager.cs : COUNT " + list.Count);
		int num = UnityEngine.Random.Range(0, list.Count);
		if (num >= list.Count)
		{
			num = list.Count - 1;
		}
		Debug.Log("ContentManager.cs : RANDIDX " + num);
		ContentManager.Content content = list[num];
		this._AvailableContent.Remove(content);
		this._UsedContent.Add(content);
		base.StartCoroutine(this.SaveUsedContent());
		return content;
	}

	// Token: 0x06003783 RID: 14211 RVA: 0x0012B5C4 File Offset: 0x001297C4
	private IEnumerator LoadUsedContent()
	{
		if (File.Exists(Application.persistentDataPath + "/UsedContent.json"))
		{
			this.LoadUsedContentFromJSON(JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/UsedContent.json")));
		}
		yield break;
	}

	// Token: 0x06003784 RID: 14212 RVA: 0x0012B5E0 File Offset: 0x001297E0
	private IEnumerator SaveUsedContent()
	{
		StringBuilder stringBuilder = new StringBuilder();
		JsonWriter jsonWriter = new JsonWriter(stringBuilder);
		jsonWriter.PrettyPrint = true;
		jsonWriter.WriteObjectStart();
		ContentManager.GameType type;
		for (type = ContentManager.GameType.SubTheTitle; type < ContentManager.GameType.Max; type++)
		{
			jsonWriter.WritePropertyName(type.ToString());
			jsonWriter.WriteArrayStart();
			List<ContentManager.Content> list = this._UsedContent.FindAll((ContentManager.Content x) => x.GameType == type);
			for (int i = 0; i < list.Count; i++)
			{
				jsonWriter.Write(list[i].ID);
			}
			jsonWriter.WriteArrayEnd();
		}
		jsonWriter.WriteObjectEnd();
		File.WriteAllText(Application.persistentDataPath + "/UsedContent.json", stringBuilder.ToString());
		yield break;
	}

	// Token: 0x06003785 RID: 14213 RVA: 0x0012B5FC File Offset: 0x001297FC
	private void LoadUsedContentFromJSON(JsonData jsonData)
	{
		ContentManager.GameType type;
		for (type = ContentManager.GameType.SubTheTitle; type < ContentManager.GameType.Max; type++)
		{
			JsonData usedGameTypeContentData = jsonData[type.ToString()];
			if (usedGameTypeContentData != null && usedGameTypeContentData.IsArray)
			{
				int i;
				for (i = 0; i < usedGameTypeContentData.Count; i++)
				{
					if (usedGameTypeContentData[i].IsInt)
					{
						ContentManager.Content content = this._AvailableContent.Find((ContentManager.Content x) => x.GameType == type && x.ID == (int)usedGameTypeContentData[i]);
						if (content != null)
						{
							this._AvailableContent.Remove(content);
							this._UsedContent.Add(content);
						}
					}
				}
			}
		}
	}

	// Token: 0x06003786 RID: 14214 RVA: 0x0012B70C File Offset: 0x0012990C
	private void ExcludeContent(ContentManager.GameType contentType, int contentID)
	{
		ContentManager.Content content = this._AvailableContent.Find((ContentManager.Content x) => x.GameType == contentType && x.ID == contentID);
		if (content != null)
		{
			this._AvailableContent.Remove(content);
		}
		else
		{
			content = this._UsedContent.Find((ContentManager.Content x) => x.GameType == contentType && x.ID == contentID);
			if (content != null)
			{
				this._UsedContent.Remove(content);
			}
		}
	}

	// Token: 0x06003787 RID: 14215 RVA: 0x0012B788 File Offset: 0x00129988
	private void HandleDLCPurchased(string contentID)
	{
		for (int i = 0; i < this._Packages.Count; i++)
		{
			ContentManager.Package package = this._Packages[i];
			if (package.ID.Equals(contentID))
			{
				this._AvailableContent.AddRange(package.Content);
			}
		}
	}

	// Token: 0x0400230B RID: 8971
	private List<ContentManager.Package> _Packages = new List<ContentManager.Package>();

	// Token: 0x0400230C RID: 8972
	private List<ContentManager.Content> _AvailableContent = new List<ContentManager.Content>();

	// Token: 0x0400230D RID: 8973
	private List<ContentManager.Content> _UsedContent = new List<ContentManager.Content>();

	// Token: 0x0200073F RID: 1855
	public enum GameType
	{
		// Token: 0x0400230F RID: 8975
		SubTheTitle,
		// Token: 0x04002310 RID: 8976
		ExtraExtra,
		// Token: 0x04002311 RID: 8977
		BlankoMatic,
		// Token: 0x04002312 RID: 8978
		BookIt,
		// Token: 0x04002313 RID: 8979
		SurveySays,
		// Token: 0x04002314 RID: 8980
		Max
	}

	// Token: 0x02000740 RID: 1856
	public class Package
	{
		// Token: 0x17000AAB RID: 2731
		// (get) Token: 0x06003789 RID: 14217 RVA: 0x00033C2F File Offset: 0x00031E2F
		// (set) Token: 0x0600378A RID: 14218 RVA: 0x00033C37 File Offset: 0x00031E37
		public string ID { get; private set; }

		// Token: 0x17000AAC RID: 2732
		// (get) Token: 0x0600378B RID: 14219 RVA: 0x00033C40 File Offset: 0x00031E40
		public List<ContentManager.Content> Content
		{
			get
			{
				return (Singleton<GameManager>.Instance.CurrentLanguage != GameManager.LanguageSettings.French) ? this._Content : this._ContentFR;
			}
		}

		// Token: 0x0600378C RID: 14220 RVA: 0x0012B7E0 File Offset: 0x001299E0
		public void LoadFromJSON(JsonData jsonData, bool french = false)
		{
			this.ID = (string)jsonData["id"];
			this._Content = new List<ContentManager.Content>();
			JsonData jsonData2 = jsonData["subTheTitle"];
			for (int i = 0; i < jsonData2.Count; i++)
			{
				ContentManager.SubTheTitleContent subTheTitleContent = new ContentManager.SubTheTitleContent();
				subTheTitleContent.LoadFromJSON(jsonData2[i]);
				this._Content.Add(subTheTitleContent);
			}
			JsonData jsonData3 = jsonData["extraExtra"];
			for (int j = 0; j < jsonData3.Count; j++)
			{
				ContentManager.ExtraExtraContent extraExtraContent = new ContentManager.ExtraExtraContent();
				extraExtraContent.LoadFromJSON(jsonData3[j]);
				this._Content.Add(extraExtraContent);
			}
			JsonData jsonData4 = jsonData["blank-o-matic"];
			for (int k = 0; k < jsonData4.Count; k++)
			{
				ContentManager.BlankoMaticContent blankoMaticContent = new ContentManager.BlankoMaticContent();
				blankoMaticContent.LoadFromJSON(jsonData4[k]);
				this._Content.Add(blankoMaticContent);
			}
			JsonData jsonData5 = jsonData["surveySays"];
			for (int l = 0; l < jsonData5.Count; l++)
			{
				ContentManager.SurveySaysContent surveySaysContent = new ContentManager.SurveySaysContent();
				surveySaysContent.LoadFromJSON(jsonData5[l]);
				this._Content.Add(surveySaysContent);
			}
			if (french)
			{
				this._ContentFR = new List<ContentManager.Content>();
				jsonData2 = jsonData["subTheTitle_FR"];
				for (int m = 0; m < jsonData2.Count; m++)
				{
					ContentManager.SubTheTitleContent subTheTitleContent2 = new ContentManager.SubTheTitleContent();
					subTheTitleContent2.LoadFromJSON(jsonData2[m]);
					this._ContentFR.Add(subTheTitleContent2);
				}
				jsonData3 = jsonData["extraExtra_FR"];
				for (int n = 0; n < jsonData3.Count; n++)
				{
					ContentManager.ExtraExtraContent extraExtraContent2 = new ContentManager.ExtraExtraContent();
					extraExtraContent2.LoadFromJSON(jsonData3[n]);
					this._ContentFR.Add(extraExtraContent2);
				}
				jsonData4 = jsonData["blank-o-matic_FR"];
				for (int num = 0; num < jsonData4.Count; num++)
				{
					ContentManager.BlankoMaticContent blankoMaticContent2 = new ContentManager.BlankoMaticContent();
					blankoMaticContent2.LoadFromJSON(jsonData4[num]);
					this._ContentFR.Add(blankoMaticContent2);
				}
				jsonData5 = jsonData["surveySays_FR"];
				for (int num2 = 0; num2 < jsonData5.Count; num2++)
				{
					ContentManager.SurveySaysContent surveySaysContent2 = new ContentManager.SurveySaysContent();
					surveySaysContent2.LoadFromJSON(jsonData5[num2]);
					this._ContentFR.Add(surveySaysContent2);
				}
			}
		}

		// Token: 0x04002315 RID: 8981
		private List<ContentManager.Content> _Content = new List<ContentManager.Content>();

		// Token: 0x04002316 RID: 8982
		private List<ContentManager.Content> _ContentFR = new List<ContentManager.Content>();
	}

	// Token: 0x02000741 RID: 1857
	public abstract class Content
	{
		// Token: 0x17000AAD RID: 2733
		// (get) Token: 0x0600378E RID: 14222 RVA: 0x00033C63 File Offset: 0x00031E63
		// (set) Token: 0x0600378F RID: 14223 RVA: 0x00033C6B File Offset: 0x00031E6B
		public int ID { get; private set; }

		// Token: 0x17000AAE RID: 2734
		// (get) Token: 0x06003790 RID: 14224 RVA: 0x00033C74 File Offset: 0x00031E74
		// (set) Token: 0x06003791 RID: 14225 RVA: 0x00033C7C File Offset: 0x00031E7C
		public List<string> HouseAnswers { get; private set; }

		// Token: 0x17000AAF RID: 2735
		// (get) Token: 0x06003792 RID: 14226 RVA: 0x00033C85 File Offset: 0x00031E85
		// (set) Token: 0x06003793 RID: 14227 RVA: 0x00033C8D File Offset: 0x00031E8D
		public bool FamilyMode { get; private set; }

		// Token: 0x17000AB0 RID: 2736
		// (get) Token: 0x06003794 RID: 14228 RVA: 0x00033C96 File Offset: 0x00031E96
		// (set) Token: 0x06003795 RID: 14229 RVA: 0x00033C9E File Offset: 0x00031E9E
		public List<string> FamilyModeHouseAnswers { get; private set; }

		// Token: 0x17000AB1 RID: 2737
		// (get) Token: 0x06003796 RID: 14230
		public abstract ContentManager.GameType GameType { get; }

		// Token: 0x06003797 RID: 14231 RVA: 0x0012BA74 File Offset: 0x00129C74
		public virtual void LoadFromJSON(JsonData jsonData)
		{
			this.ID = (int)jsonData["id"];
			this.HouseAnswers = new List<string>();
			this.FamilyMode = ((string)jsonData["familyMode"]).Equals("true");
			this.FamilyModeHouseAnswers = new List<string>();
			for (int i = 0; i < jsonData["houseAnswers"].Count; i++)
			{
				string text = Regex.Unescape((string)jsonData["houseAnswers"][i]);
				if (!text.StartsWith("^^"))
				{
					this.FamilyModeHouseAnswers.Add(text);
				}
				else
				{
					text = text.Substring(2, text.Length - 2);
				}
				this.HouseAnswers.Add(text);
			}
		}
	}

	// Token: 0x02000742 RID: 1858
	public class SubTheTitleContent : ContentManager.Content
	{
		// Token: 0x17000AB2 RID: 2738
		// (get) Token: 0x06003799 RID: 14233 RVA: 0x0001140B File Offset: 0x0000F60B
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.SubTheTitle;
			}
		}

		// Token: 0x17000AB3 RID: 2739
		// (get) Token: 0x0600379A RID: 14234 RVA: 0x00033CAF File Offset: 0x00031EAF
		// (set) Token: 0x0600379B RID: 14235 RVA: 0x00033CB7 File Offset: 0x00031EB7
		public double Start { get; private set; }

		// Token: 0x17000AB4 RID: 2740
		// (get) Token: 0x0600379C RID: 14236 RVA: 0x00033CC0 File Offset: 0x00031EC0
		// (set) Token: 0x0600379D RID: 14237 RVA: 0x00033CC8 File Offset: 0x00031EC8
		public double End { get; private set; }

		// Token: 0x17000AB5 RID: 2741
		// (get) Token: 0x0600379E RID: 14238 RVA: 0x00033CD1 File Offset: 0x00031ED1
		// (set) Token: 0x0600379F RID: 14239 RVA: 0x00033CD9 File Offset: 0x00031ED9
		public Vector2 Position { get; private set; }

		// Token: 0x17000AB6 RID: 2742
		// (get) Token: 0x060037A0 RID: 14240 RVA: 0x00033CE2 File Offset: 0x00031EE2
		// (set) Token: 0x060037A1 RID: 14241 RVA: 0x00033CEA File Offset: 0x00031EEA
		public Vector2 Dimensions { get; private set; }

		// Token: 0x060037A2 RID: 14242 RVA: 0x0012BB54 File Offset: 0x00129D54
		public override void LoadFromJSON(JsonData jsonData)
		{
			base.LoadFromJSON(jsonData);
			if (jsonData["start"].IsDouble)
			{
				this.Start = (double)jsonData["start"];
			}
			else
			{
				this.Start = (double)((int)jsonData["start"]);
			}
			if (jsonData["end"].IsDouble)
			{
				this.End = (double)jsonData["end"];
			}
			else
			{
				this.End = (double)((int)jsonData["end"]);
			}
			Debug.Log("[ContentManager] Start : " + this.Start);
			Debug.Log("[ContentManager] End : " + this.End);
			string text = (string)jsonData["position"];
			Debug.Log("[ContentManager] Positions : " + text);
			string[] array = text.Split(new char[]
			{
				','
			});
			this.Position = new Vector2(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture));
			if (array.Length > 2)
			{
				this.Dimensions = new Vector2(float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
			}
			else
			{
				this.Dimensions = Vector2.zero;
			}
		}
	}

	// Token: 0x02000743 RID: 1859
	public class ExtraExtraContent : ContentManager.Content
	{
		// Token: 0x17000AB7 RID: 2743
		// (get) Token: 0x060037A4 RID: 14244 RVA: 0x0001A7E0 File Offset: 0x000189E0
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.ExtraExtra;
			}
		}
	}

	// Token: 0x02000744 RID: 1860
	public class BlankoMaticContent : ContentManager.Content
	{
		// Token: 0x17000AB8 RID: 2744
		// (get) Token: 0x060037A6 RID: 14246 RVA: 0x00025051 File Offset: 0x00023251
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.BlankoMatic;
			}
		}

		// Token: 0x17000AB9 RID: 2745
		// (get) Token: 0x060037A7 RID: 14247 RVA: 0x00033CF3 File Offset: 0x00031EF3
		// (set) Token: 0x060037A8 RID: 14248 RVA: 0x00033CFB File Offset: 0x00031EFB
		public string Message { get; private set; }

		// Token: 0x060037A9 RID: 14249 RVA: 0x00033D04 File Offset: 0x00031F04
		public override void LoadFromJSON(JsonData jsonData)
		{
			base.LoadFromJSON(jsonData);
			this.Message = Regex.Unescape((string)jsonData["prompt"]);
			this.Message = this.Message.Replace("%answer%", "_______");
		}
	}

	// Token: 0x02000745 RID: 1861
	public class BookItContent : ContentManager.Content
	{
		// Token: 0x17000ABA RID: 2746
		// (get) Token: 0x060037AB RID: 14251 RVA: 0x00033D43 File Offset: 0x00031F43
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.BookIt;
			}
		}
	}

	// Token: 0x02000746 RID: 1862
	public class SurveySaysContent : ContentManager.Content
	{
		// Token: 0x17000ABB RID: 2747
		// (get) Token: 0x060037AD RID: 14253 RVA: 0x0001EB14 File Offset: 0x0001CD14
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.SurveySays;
			}
		}

		// Token: 0x17000ABC RID: 2748
		// (get) Token: 0x060037AE RID: 14254 RVA: 0x00033D46 File Offset: 0x00031F46
		// (set) Token: 0x060037AF RID: 14255 RVA: 0x00033D4E File Offset: 0x00031F4E
		public string[] Prompts { get; private set; }

		// Token: 0x17000ABD RID: 2749
		// (get) Token: 0x060037B0 RID: 14256 RVA: 0x00033D57 File Offset: 0x00031F57
		// (set) Token: 0x060037B1 RID: 14257 RVA: 0x00033D5F File Offset: 0x00031F5F
		public List<string>[] AllHouseAnswers { get; private set; }

		// Token: 0x17000ABE RID: 2750
		// (get) Token: 0x060037B2 RID: 14258 RVA: 0x00033D68 File Offset: 0x00031F68
		// (set) Token: 0x060037B3 RID: 14259 RVA: 0x00033D70 File Offset: 0x00031F70
		public List<string>[] AllFamilyModeHouseAnswers { get; private set; }

		// Token: 0x060037B4 RID: 14260 RVA: 0x0012BCC0 File Offset: 0x00129EC0
		public override void LoadFromJSON(JsonData jsonData)
		{
			base.LoadFromJSON(jsonData);
			this.Prompts = new string[3];
			this.Prompts[0] = Regex.Unescape((string)jsonData["prompt"]);
			this.Prompts[1] = Regex.Unescape((string)jsonData["prompt2"]);
			this.Prompts[2] = Regex.Unescape((string)jsonData["prompt3"]);
			this.AllHouseAnswers = new List<string>[3];
			this.AllHouseAnswers[0] = base.HouseAnswers;
			this.AllFamilyModeHouseAnswers = new List<string>[3];
			this.AllFamilyModeHouseAnswers[0] = base.FamilyModeHouseAnswers;
			this.AllHouseAnswers[1] = new List<string>();
			this.AllFamilyModeHouseAnswers[1] = new List<string>();
			for (int i = 0; i < jsonData["houseAnswers2"].Count; i++)
			{
				string text = Regex.Unescape((string)jsonData["houseAnswers2"][i]);
				if (!text.StartsWith("^^"))
				{
					this.AllFamilyModeHouseAnswers[1].Add(text);
				}
				else
				{
					text = text.Substring(2, text.Length - 2);
				}
				this.AllHouseAnswers[1].Add(text);
			}
			this.AllHouseAnswers[2] = new List<string>();
			this.AllFamilyModeHouseAnswers[2] = new List<string>();
			for (int j = 0; j < jsonData["houseAnswers3"].Count; j++)
			{
				string text2 = Regex.Unescape((string)jsonData["houseAnswers3"][j]);
				if (!text2.StartsWith("^^"))
				{
					this.AllFamilyModeHouseAnswers[2].Add(text2);
				}
				else
				{
					text2 = text2.Substring(2, text2.Length - 2);
				}
				this.AllHouseAnswers[2].Add(text2);
			}
		}
	}
}
