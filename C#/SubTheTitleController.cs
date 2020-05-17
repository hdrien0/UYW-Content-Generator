using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200078F RID: 1935
public class SubTheTitleController : BasicGameController
{
	// Token: 0x17000B18 RID: 2840
	// (get) Token: 0x0600391F RID: 14623 RVA: 0x000346F1 File Offset: 0x000328F1
	protected override string TaskDescriptionText
	{
		get
		{
			return "Complete the subtitle!";
		}
	}

	// Token: 0x17000B19 RID: 2841
	// (get) Token: 0x06003920 RID: 14624 RVA: 0x000346F8 File Offset: 0x000328F8
	protected override string WaitingMessage
	{
		get
		{
			return "WATCH THE CLIP!";
		}
	}

	// Token: 0x06003921 RID: 14625 RVA: 0x000346FF File Offset: 0x000328FF
	protected override IEnumerator StartState(GameController.State state)
	{
		yield return base.StartState(state);
		switch (state)
		{
		case GameController.State.Introduction:
			RichPresenceManager.SetRichPresence("UYW_RP_SubTheTitle");
			this.InputText.gameObject.SetActive(false);
			this.MovieImage.enabled = false;
			this._ActiveContent = (Singleton<ContentManager>.Instance.FindRandomContent(ContentManager.GameType.SubTheTitle) as ContentManager.SubTheTitleContent);

			if (this._ActiveContent.ID > 999) {
				moviePath = Application.persistentDataPath + "/NewContent/stt" + this._ActiveContent.ID.ToString() +".mp4";
			} else {
				moviePath =  string.Concat(new object[] {
				"Content/SubTheTitle/",
				this._ActiveContent.ID,
				"/clip",
				(Singleton<GameManager>.Instance.CurrentLanguage != GameManager.LanguageSettings.French) ? string.Empty : "_FR"
			});
			}

			if (this._ActiveContent.Dimensions != Vector2.zero)
			{
				this.InputText.rectTransform.sizeDelta = this._ActiveContent.Dimensions;
			}
			this.InputText.transform.localPosition = this._ActiveContent.Position;
			while (Singleton<LoadingScreen>.Instance.IsShowing)
			{
				yield return null;
			}
			if (Singleton<GameManager>.Instance.CurrentMode != GameManager.GameMode.Debug)
			{
				if (Singleton<GameManager>.Instance.IsFirstSubTheTitle)
				{
					Singleton<AudioManager>.Instance.SetMusicVolume(0f);
					yield return this.PlayTutorialVideo(GameController.State.Introduction, "STT_Tut1" + ((Singleton<GameManager>.Instance.CurrentLanguage != GameManager.LanguageSettings.French) ? string.Empty : "_FR"), false);
					Singleton<AudioManager>.Instance.SetMusicVolume(1f);
				}
				else
				{
					yield return this._MoviePlayer.Play(SubTheTitleController.CountdownMoviePath, false, false);
				}
				this._MoviePlayer.Target = this.MovieImage;
				yield return this._MoviePlayer.Play(SubTheTitleController.CountdownMoviePath, false, false);
				this.MovieImage.enabled = true;
				while (this._MoviePlayer.IsPlaying)
				{
					yield return null;
				}
			}
			base.ChangeState(GameController.State.RevealPrompt);
			break;
		case GameController.State.RevealPrompt:
		{
			if (Singleton<GameManager>.Instance.FamilyMode)
			{
				base.SetHouseAnswers(this._ActiveContent.FamilyModeHouseAnswers);
			}
			else
			{
				base.SetHouseAnswers(this._ActiveContent.HouseAnswers);
			}
			Singleton<AudioManager>.Instance.FadeOutMusic(0f, 0f);
			UnityEngine.Debug.Log("Loading Prompt");
			UnityEngine.Debug.Log(this._ActiveContent.ID);
			this.ShadowBackdrop.DOFade(0.58f, 0.5f);
			string insertSubtitle = "[Insert subtitle here.]";
			if (Singleton<GameManager>.Instance.CurrentLanguage == GameManager.LanguageSettings.French)
			{
				insertSubtitle = "[Insérer un sous-titre ici.]";
			}
			yield return this.PlayMovie(GameController.State.RevealPrompt, this._ActiveContent, moviePath, insertSubtitle, false, true);
			base.ChangeState(GameController.State.InputCheck);
			break;
		}
		case GameController.State.WaitForInput:
		{
			string insertSubtitle2 = "[Insert subtitle here.]";
			if (Singleton<GameManager>.Instance.CurrentLanguage == GameManager.LanguageSettings.French)
			{
				insertSubtitle2 = "[Insérer un sous-titre ici.]";
			}
			yield return this.PlayMovie(GameController.State.WaitForInput, this._ActiveContent, moviePath, insertSubtitle2, false, true);
			this.ShadowBackdrop.DOFade(0f, 0.5f);
			Singleton<AudioManager>.Instance.PlayMusic("STT_Theme", 1f, true);
			base.StartCoroutine(this.PlayTutorialVideo(GameController.State.WaitForInput, "STT_Tut2" + ((Singleton<GameManager>.Instance.CurrentLanguage != GameManager.LanguageSettings.French) ? string.Empty : "_FR"), true));
			break;
		}
		case GameController.State.RevealAnswers:
		{
			Singleton<AudioManager>.Instance.StopMusic();
			this.ShadowBackdrop.DOFade(0.58f, 0.5f);
			yield return Singleton<AudioManager>.Instance.PlayVoiceOverAndWait("12.X", Singleton<GameManager>.Instance.GetAnswerRevealClips());
			GameController.Answer[] inputArray = this._UserInput[this._CurrentRound].ToArray();
			foreach (GameController.Answer inputPair in inputArray)
			{
				yield return this.PlayMovie(GameController.State.RevealAnswers, this._ActiveContent, moviePath, inputPair.Value, true, true);
				yield return base.ResolveInput(inputPair.User.ID);
			}
			this.ShadowBackdrop.DOFade(0f, 0.5f);
			base.ChangeState(GameController.State.VotingCheck);
			break;
		}
		case GameController.State.WaitForVoting:
			if (Singleton<GameManager>.Instance.IsFirstSubTheTitle && Singleton<GameManager>.Instance.HouseAnswers)
			{
				Singleton<AudioManager>.Instance.StopMusic();
				yield return this.PlayTutorialVideo(GameController.State.WaitForVoting, "STT_Tut3" + ((Singleton<GameManager>.Instance.CurrentLanguage != GameManager.LanguageSettings.French) ? string.Empty : "_FR"), false);
				this._TutorialPlaying = false;
			}
			Singleton<AudioManager>.Instance.PlayMusic("STT_Theme", 1f, true);
			if (this._CurrentState != GameController.State.WaitForVoting)
			{
				yield break;
			}
			yield return this._MoviePlayer.Play(SubTheTitleController.LobbyMoviePath, true, false);
			this.MovieImage.enabled = true;
			break;
		case GameController.State.RevealResults:
		{
			this.ShadowBackdrop.DOFade(0.58f, 0.5f);
			yield return Singleton<AudioManager>.Instance.PlayVoiceOverAndWait("36.X", Singleton<GameManager>.Instance.GetVoteRevealVO());
			this.StopTutorialVideo();
			List<GameController.Answer> orderedAnswers = base.CalculateVoteOrderedInput();
			foreach (GameController.Answer inputPair2 in orderedAnswers)
			{
				yield return this.ShowResult(this._ActiveContent.ID.ToString(), inputPair2.Value);
				yield return base.ResolveVote(inputPair2.User.ID);
			}
			this.ShadowBackdrop.DOFade(0f, 0.5f);
			base.ChangeState(GameController.State.PostResults);
			break;
		}
		}
		yield break;
	}

	// Token: 0x06003922 RID: 14626 RVA: 0x00034715 File Offset: 0x00032915
	private IEnumerator PlayMovie(GameController.State curState, ContentManager.SubTheTitleContent subTheTitleContent, string moviePath, string promptText, bool fadeMusic = true, bool holdBleep = true)
	{
		this.MovieImage.gameObject.SetActive(true);
		this.ScreenshotImage.gameObject.SetActive(false);
		if (this._MoviePlayer != null)
		{
			this._MoviePlayer.Stop();
		}
		this._MoviePlayer.Target = this.MovieImage;
		yield return this._MoviePlayer.Play(moviePath, false, false);
		float aspectRatio = this._MoviePlayer.Width / this._MoviePlayer.Height;
		if (this._ActiveContent.ID < 999)
		{
			if (aspectRatio >= 1.7f)
			{
				this.MovieImage.rectTransform.sizeDelta = new Vector2(1282f, 722f);
			}
			else
			{
				this.MovieImage.rectTransform.sizeDelta = new Vector2(962f, 722f);
			}
		}
		if (fadeMusic)
		{
			Singleton<AudioManager>.Instance.SetMusicVolume(0.25f);
		}
		this.MovieImage.enabled = true;
		while (!this._MoviePlayer.IsPlaying)
		{
			yield return null;
		}
		float timeElapsed = 0f;
		this.InputText.text = promptText;
		UnityEngine.Debug.Log("[STT] Start : " + subTheTitleContent.Start);
		while ((double)timeElapsed <= subTheTitleContent.Start)
		{
			timeElapsed += Time.deltaTime;
			if (curState != this._CurrentState)
			{
				this.InputText.gameObject.SetActive(false);
				this._MoviePlayer.Stop();
				yield break;
			}
			yield return null;
		}
		this.InputText.gameObject.SetActive(true);
		while ((double)timeElapsed <= subTheTitleContent.End && this._MoviePlayer.IsPlaying)
		{
			timeElapsed += Time.deltaTime;
			if (curState != this._CurrentState)
			{
				this.InputText.gameObject.SetActive(false);
				this._MoviePlayer.Stop();
				yield break;
			}
			yield return null;
		}
		this.InputText.text = string.Empty;
		this.InputText.gameObject.SetActive(false);
		while (this._MoviePlayer.IsPlaying)
		{
			if (curState != this._CurrentState)
			{
				this.InputText.gameObject.SetActive(false);
				this._MoviePlayer.Stop();
				yield break;
			}
			yield return null;
		}
		this._MoviePlayer.Stop();
		this._MoviePlayer.Unload();
		yield return this._MoviePlayer.Play(SubTheTitleController.BleepMoviePath, false, false);
		while (this._MoviePlayer.IsPlaying)
		{
			if (curState != this._CurrentState)
			{
				this.InputText.gameObject.SetActive(false);
				this._MoviePlayer.Stop();
				yield break;
			}
			yield return null;
		}
		yield return Yielders.Seconds(0.5f);
		this._MoviePlayer.Stop();
		if (!holdBleep)
		{
			this._MoviePlayer.Unload();
			this.MovieImage.enabled = false;
		}
		if (fadeMusic)
		{
			Singleton<AudioManager>.Instance.SetMusicVolume(1f);
		}
		yield break;
	}

	// Token: 0x06003923 RID: 14627 RVA: 0x0013216C File Offset: 0x0013036C
	private IEnumerator PlayTutorialVideo(GameController.State curState, string tutorialName, bool loop)
	{
		this._TutorialPlaying = true;
		if (curState == GameController.State.Introduction)
		{
			this.ShowSkipButton();
		}
		this._MoviePlayer.Target = this.MovieImage;
		if (tutorialName.Contains("STT_Tut1"))
		{
			yield return this._MoviePlayer.Play("Videos/Tutorial/" + tutorialName, loop, false);
		}
		yield return this._MoviePlayer.Play("Videos/Tutorial/" + tutorialName, loop, false);
		float aspectRatio = this._MoviePlayer.Width / this._MoviePlayer.Height;
		if (aspectRatio >= 1.7f)
		{
			this.MovieImage.rectTransform.sizeDelta = new Vector2(1282f, 722f);
		}
		else
		{
			this.MovieImage.rectTransform.sizeDelta = new Vector2(962f, 722f);
		}
		this.MovieImage.enabled = true;
		if (!loop)
		{
			while (this._MoviePlayer.IsPlaying)
			{
				if (this._CurrentState != curState || (curState == GameController.State.Introduction && this._PlayerController.GetButtonDown("UISubmit")))
				{
					this._MoviePlayer.Stop();
					this._MoviePlayer.Unload();
					this.HideSkipButton();
					yield break;
				}
				yield return null;
			}
			this.HideSkipButton();
			this.MovieImage.enabled = false;
			this._TutorialPlaying = false;
			this._MoviePlayer.Stop();
			this._MoviePlayer.Unload();
		}
		yield break;
	}

	// Token: 0x06003924 RID: 14628 RVA: 0x00034751 File Offset: 0x00032951
	private IEnumerator ShowResult(string contentID, string promptText)
	{
		this.MovieImage.gameObject.SetActive(false);
		this.ScreenshotImage.gameObject.SetActive(true);
		this.ScreenshotImage.DOFade(0f, 0.125f);
		yield return Yielders.Seconds(0.125f);
		if (this._ActiveContent.ID > 999)
		{
			byte[] data = File.ReadAllBytes(Application.persistentDataPath + "/NewContent/sttImg" + this._ActiveContent.ID);
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
			this.ScreenshotImage.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), 100f);
			this.ScreenshotImage.preserveAspect = true;
		}
		else
		{
			ResourceRequest resourceRequest = Resources.LoadAsync<Sprite>("SubTheTitleScreens/" + contentID);
			yield return resourceRequest;
			this.ScreenshotImage.sprite = (resourceRequest.asset as Sprite);
			this.ScreenshotImage.SetNativeSize();
			if (this.ScreenshotImage.sprite.bounds.size.x / this.ScreenshotImage.sprite.bounds.size.y >= 1.7f)
			{
				this.ScreenshotImage.rectTransform.sizeDelta = new Vector2(1282f, 722f);
			}
			else
			{
				this.ScreenshotImage.rectTransform.sizeDelta = new Vector2(962f, 722f);
			}
			resourceRequest = null;
		}
		this.ScreenshotImage.DOFade(1f, 0.125f);
		this.InputText.text = promptText;
		this.InputText.gameObject.SetActive(true);
		yield return Yielders.Seconds(1.75f);
		yield break;
	}

	// Token: 0x06003925 RID: 14629 RVA: 0x0003476E File Offset: 0x0003296E
	private void StopTutorialVideo()
	{
		base.StopCoroutine("PlayTutorialVideo");
		this._TutorialPlaying = false;
		this._MoviePlayer.Stop();
		this._MoviePlayer.Unload();
	}

	// Token: 0x06003926 RID: 14630 RVA: 0x0013219C File Offset: 0x0013039C
	private void ShowSkipButton()
	{
		Controller controller = this._PlayerController.controllers.GetLastActiveController();
		if (controller == null)
		{
			if (this._PlayerController.controllers.joystickCount > 0)
			{
				controller = this._PlayerController.controllers.Joysticks[0];
			}
			else
			{
				controller = this._PlayerController.controllers.Keyboard;
			}
		}
		ControllerType type = controller.type;
		if (type != ControllerType.Keyboard)
		{
			if (type == ControllerType.Joystick)
			{
				Joystick joystick = (Joystick)controller;
				string text = joystick.hardwareName.ToLower();
				if (text.Contains("xbox") || text.Contains("xinput"))
				{
					this.Xbox360Group.SetActive(true);
				}
				else if (text.Contains("playstation") || text.Contains("ps4"))
				{
					this.PS4Group.SetActive(true);
				}
				else
				{
					this.Xbox360Group.SetActive(true);
				}
			}
		}
		else
		{
			this.KeyboardGroup.SetActive(true);
		}
	}

	// Token: 0x06003927 RID: 14631 RVA: 0x001322B4 File Offset: 0x001304B4
	private void HideSkipButton()
	{
		this.XboxOneGroup.SetActive(false);
		this.Xbox360Group.SetActive(false);
		this.KeyboardGroup.SetActive(false);
		this.PS4Group.SetActive(false);
		this.WiiUGroup.SetActive(false);
		this.SwitchGroup.SetActive(false);
	}

	// Token: 0x0400247F RID: 9343
	public static string BleepMoviePath = "Videos/Bumper/STTBoop";

	// Token: 0x04002480 RID: 9344
	public static string CountdownMoviePath = "Videos/Bumper/STTCountdown";

	// Token: 0x04002481 RID: 9345
	public static string LobbyMoviePath = "Videos/Bumper/letsgotolobby";

	// Token: 0x04002482 RID: 9346
	public RawImage MovieImage;

	// Token: 0x04002483 RID: 9347
	public Image ScreenshotImage;

	// Token: 0x04002484 RID: 9348
	public TextMeshProUGUI InputText;

	// Token: 0x04002485 RID: 9349
	public Image ShadowBackdrop;

	// Token: 0x04002486 RID: 9350
	[Header("Skip Containers")]
	public GameObject KeyboardGroup;

	// Token: 0x04002487 RID: 9351
	public GameObject XboxOneGroup;

	// Token: 0x04002488 RID: 9352
	public GameObject PS4Group;

	// Token: 0x04002489 RID: 9353
	public GameObject Xbox360Group;

	// Token: 0x0400248A RID: 9354
	public GameObject WiiUGroup;

	// Token: 0x0400248B RID: 9355
	public GameObject SwitchGroup;

	// Token: 0x0400248C RID: 9356
	[SerializeField]
	private MoviePlayer _MoviePlayer;

	// Token: 0x0400248D RID: 9357
	private ContentManager.SubTheTitleContent _ActiveContent;

	// Token: 0x0400248E RID: 9358
	private bool _TutorialPlaying;

	// Token: 0x0400248F RID: 9359
	private string moviePath;
}

