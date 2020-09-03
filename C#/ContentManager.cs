using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine;

public class ContentManager : Singleton<ContentManager>
{
	public List<ContentManager.Package> Packages
	{
		get
		{
			return new List<ContentManager.Package>(this._Packages);
		}
	}

	public List<ContentManager.Content> AvailableContent
	{
		get
		{
			return this._AvailableContent;
		}
	}

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

	private void Start()
	{
		Singleton<DLCManager>.Instance.OnDLCPurchased += this.HandleDLCPurchased;
		UnityEngine.Random.InitState(DateTime.Now.Millisecond);
		this.ReloadContent();
	}

	protected override void OnWillBeDestroyed()
	{
		base.OnWillBeDestroyed();
		Singleton<DLCManager>.Instance.OnDLCPurchased -= this.HandleDLCPurchased;
	}

	public void ReloadContent()
	{
		//BEGIN MODIFICATION
        string newContentJsonPath = Application.persistentDataPath + "/NewContent/nc.json";	
		bool newContent = File.Exists(newContentJsonPath);	
		JsonData newJsonData = null;	
		if (newContent){	
			newJsonData = JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/NewContent/nc.json"));	
		}
		//END MODIFICATION

		JsonData jsonData = JsonMapper.ToObject(Resources.Load<TextAsset>("Content/Manifest/Manifest").text)["packages"];
		this._AvailableContent = new List<ContentManager.Content>();
		this._Packages = new List<ContentManager.Package>();
		this._UsedContent = new List<ContentManager.Content>();
		if (Singleton<GameManager>.Instance.CurrentLanguage == GameManager.LanguageSettings.English)
		{
			for (int i = 0; i < jsonData.Count; i++)
			{
                //BEGIN MODIFICATION
                if (newContent){	
					for (int z = 0; z < newJsonData["remove"].Count; z++)	
					{	
						jsonData[i][(string)newJsonData["remove"][z]].Clear();	
					}	
				}
                //END MODIFICATION

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
            //BEGIN MODIFICATION
            if (newContent){	
				for (int k = 0; k < newJsonData["remove"].Count; k++)	
				{	
					jsonData[j][(string)newJsonData["remove"][k]].Clear();	
				}	
			}
            //END MODIFICATION

			ContentManager.Package package2 = new ContentManager.Package();
			package2.LoadFromJSON(jsonData[j], true);
			this._Packages.Add(package2);
			this._AvailableContent.AddRange(package2.Content);
		}

		//BEGIN MODIFICATION
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
		//END MODIFICATION

		base.StartCoroutine(this.LoadUsedContent());
	}

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

	private IEnumerator LoadUsedContent()
	{
		if (File.Exists(Application.persistentDataPath + "/UsedContent.json"))
		{
			this.LoadUsedContentFromJSON(JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/UsedContent.json")));
		}
		yield break;
	}

	private IEnumerator SaveUsedContent()
	{
		StringBuilder stringBuilder = new StringBuilder();
		JsonWriter jsonWriter = new JsonWriter(stringBuilder);
		jsonWriter.PrettyPrint = true;
		jsonWriter.WriteObjectStart();
		ContentManager.GameType type;
		ContentManager.GameType type2;
		for (type = ContentManager.GameType.SubTheTitle; type < ContentManager.GameType.Max; type = type2 + 1)
		{
			jsonWriter.WritePropertyName(type.ToString());
			jsonWriter.WriteArrayStart();
			List<ContentManager.Content> list = this._UsedContent.FindAll((ContentManager.Content x) => x.GameType == type);
			for (int i = 0; i < list.Count; i++)
			{
				jsonWriter.Write(list[i].ID);
			}
			jsonWriter.WriteArrayEnd();
			type2 = type;
		}
		jsonWriter.WriteObjectEnd();
		File.WriteAllText(Application.persistentDataPath + "/UsedContent.json", stringBuilder.ToString());
		yield break;
	}

	private void LoadUsedContentFromJSON(JsonData jsonData)
	{
		ContentManager.GameType type;
		ContentManager.GameType type2;
		for (type = ContentManager.GameType.SubTheTitle; type < ContentManager.GameType.Max; type = type2 + 1)
		{
			JsonData usedGameTypeContentData = jsonData[type.ToString()];
			if (usedGameTypeContentData != null && usedGameTypeContentData.IsArray)
			{
				int j;
				int i;
				for (i = 0; i < usedGameTypeContentData.Count; i = j + 1)
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
					j = i;
				}
			}
			type2 = type;
		}
	}

	private void ExcludeContent(ContentManager.GameType contentType, int contentID)
	{
		ContentManager.Content content = this._AvailableContent.Find((ContentManager.Content x) => x.GameType == contentType && x.ID == contentID);
		if (content != null)
		{
			this._AvailableContent.Remove(content);
			return;
		}
		content = this._UsedContent.Find((ContentManager.Content x) => x.GameType == contentType && x.ID == contentID);
		if (content != null)
		{
			this._UsedContent.Remove(content);
		}
	}

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

	public ContentManager()
	{
	}

	private List<ContentManager.Package> _Packages = new List<ContentManager.Package>();

	private List<ContentManager.Content> _AvailableContent = new List<ContentManager.Content>();

	private List<ContentManager.Content> _UsedContent = new List<ContentManager.Content>();

	public enum GameType
	{
		SubTheTitle,
		ExtraExtra,
		BlankoMatic,
		BookIt,
		SurveySays,
		Max
	}

	public class Package
	{
		public string ID { get; private set; }

		public List<ContentManager.Content> Content
		{
			get
			{
				if (Singleton<GameManager>.Instance.CurrentLanguage != GameManager.LanguageSettings.French)
				{
					return this._Content;
				}
				return this._ContentFR;
			}
		}

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

		public Package()
		{
		}

		private List<ContentManager.Content> _Content = new List<ContentManager.Content>();

		private List<ContentManager.Content> _ContentFR = new List<ContentManager.Content>();
	}

	public abstract class Content
	{
		public int ID { get; private set; }

		public List<string> HouseAnswers { get; private set; }

		public bool FamilyMode { get; private set; }

		public List<string> FamilyModeHouseAnswers { get; private set; }

		public abstract ContentManager.GameType GameType { get; }

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

		protected Content()
		{
		}
	}

	public class SubTheTitleContent : ContentManager.Content
	{
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.SubTheTitle;
			}
		}

		public double Start { get; private set; }

		public double End { get; private set; }

		public Vector2 Position { get; private set; }

		public Vector2 Dimensions { get; private set; }

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
			string[] array = ((string)jsonData["position"]).Split(new char[]
			{
				','
			});
			this.Position = new Vector2(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture));
			if (array.Length > 2)
			{
				this.Dimensions = new Vector2(float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
				return;
			}
			this.Dimensions = Vector2.zero;
		}

		public SubTheTitleContent()
		{
		}
	}

	public class ExtraExtraContent : ContentManager.Content
	{
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.ExtraExtra;
			}
		}

		public ExtraExtraContent()
		{
		}
	}

	public class BlankoMaticContent : ContentManager.Content
	{
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.BlankoMatic;
			}
		}

		public string Message { get; private set; }

		public override void LoadFromJSON(JsonData jsonData)
		{
			base.LoadFromJSON(jsonData);
			this.Message = Regex.Unescape((string)jsonData["prompt"]);
			this.Message = this.Message.Replace("%answer%", "_______");
		}

		public BlankoMaticContent()
		{
		}
	}

	public class BookItContent : ContentManager.Content
	{
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.BookIt;
			}
		}

		public BookItContent()
		{
		}
	}

	public class SurveySaysContent : ContentManager.Content
	{
		public override ContentManager.GameType GameType
		{
			get
			{
				return ContentManager.GameType.SurveySays;
			}
		}

		public string[] Prompts { get; private set; }

		public List<string>[] AllHouseAnswers { get; private set; }

		public List<string>[] AllFamilyModeHouseAnswers { get; private set; }

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

		public SurveySaysContent()
		{
		}
	}
}
