using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// Token: 0x020007CA RID: 1994
[RequireComponent(typeof(AudioSource))]
public class MoviePlayer : MonoBehaviour
{
	// Token: 0x17000B80 RID: 2944
	// (get) Token: 0x06003A9E RID: 15006 RVA: 0x00139250 File Offset: 0x00137450
	public bool IsPlaying
	{
		get
		{
			return this._VideoPlayer != null && ((this._VideoPlayer.isPlaying && this._VideoPlayer.frame < (long)this._VideoPlayer.frameCount) || this._Paused);
		}
	}

	// Token: 0x17000B81 RID: 2945
	// (get) Token: 0x06003A9F RID: 15007 RVA: 0x0003526B File Offset: 0x0003346B
	public float Width
	{
		get
		{
			return 1280f;
		}
	}

	// Token: 0x17000B82 RID: 2946
	// (get) Token: 0x06003AA0 RID: 15008 RVA: 0x00035272 File Offset: 0x00033472
	public float Height
	{
		get
		{
			return 720f;
		}
	}

	// Token: 0x06003AA1 RID: 15009 RVA: 0x0001A770 File Offset: 0x00018970
	private void Update()
	{
	}

	// Token: 0x06003AA2 RID: 15010 RVA: 0x00035279 File Offset: 0x00033479
	private void OnDestroy()
	{
		this.Unload();
	}

	// Token: 0x06003AA3 RID: 15011 RVA: 0x0001A770 File Offset: 0x00018970
	private void OnPreRender()
	{
	}

	// Token: 0x06003AA4 RID: 15012 RVA: 0x00035281 File Offset: 0x00033481
	public IEnumerator Play(string moviePath, bool loop = false, bool renderOnCamera = false)
	{
		UnityEngine.Debug.Log("PLAY REQUEST: " + moviePath);
		UnityEngine.Debug.Log("CREATING STREAMS");
		UnityEngine.Debug.Log(string.Concat(new object[]
		{
			"SCREEN DIMS: ",
			Screen.width,
			", ",
			Screen.height
		}));
		this._IsPrepared = false;
		if (this._VideoPlayer == null)
		{
			this._VideoPlayer = base.GetComponent<VideoPlayer>();
			if (this._VideoPlayer == null)
			{
				this._VideoPlayer = base.gameObject.AddComponent<VideoPlayer>();
			}
		}
		this._AudioSource = base.GetComponent<AudioSource>();
		this._VideoPlayer.source = VideoSource.Url;
		this._VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		if (moviePath.Contains("NewContent"))
		{
			this._VideoPlayer.url = moviePath;
			this._VideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
		}
		else
		{
			this._VideoPlayer.url = Path.Combine(Application.streamingAssetsPath, moviePath + ".mp4");
		}
		this._VideoPlayer.controlledAudioTrackCount = 1;
		this._VideoPlayer.isLooping = loop;
		if (renderOnCamera)
		{
			this._VideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
			this._VideoPlayer.targetCamera = Camera.main;
		}
		else
		{
			this._VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
		}
		this._VideoPlayer.EnableAudioTrack(0, true);
		this._VideoPlayer.SetTargetAudioSource(0, this._AudioSource);
		this._VideoPlayer.prepareCompleted += this.OnPrepareComplete;
		this._VideoPlayer.Prepare();
		this._VideoPlayer.EnableAudioTrack(0, true);
		this._VideoPlayer.SetTargetAudioSource(0, this._AudioSource);
		float prepareTimer = 0f;
		while (!this._IsPrepared && prepareTimer < 2f)
		{
			prepareTimer += Time.deltaTime;
			yield return null;
		}
		UnityEngine.Debug.Log(this._VideoPlayer.audioTrackCount);
		if (!renderOnCamera)
		{
			this.Target.texture = this._VideoPlayer.texture;
		}
		this._VideoPlayer.Play();
		this._AudioSource.Play();
		yield break;
	}

	// Token: 0x06003AA5 RID: 15013 RVA: 0x000352A5 File Offset: 0x000334A5
	private void OnPrepareComplete(VideoPlayer source)
	{
		this._IsPrepared = true;
	}

	// Token: 0x06003AA6 RID: 15014 RVA: 0x001392A8 File Offset: 0x001374A8
	public IEnumerator PlayAndWait(string moviePath, bool loop = false)
	{
		yield return this.Play(moviePath, loop, false);
		while (this.IsPlaying)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x06003AA7 RID: 15015 RVA: 0x000352AE File Offset: 0x000334AE
	public void Stop()
	{
		if (this._VideoPlayer == null)
		{
			return;
		}
		this._VideoPlayer.Stop();
	}

	// Token: 0x06003AA8 RID: 15016 RVA: 0x000352CD File Offset: 0x000334CD
	public void Pause(bool paused)
	{
		if (this._VideoPlayer == null)
		{
			return;
		}
		this._Paused = paused;
		if (paused)
		{
			this._VideoPlayer.Pause();
		}
		else
		{
			this._VideoPlayer.Play();
		}
	}

	// Token: 0x06003AA9 RID: 15017 RVA: 0x001392D4 File Offset: 0x001374D4
	public void Unload()
	{
		if (this._VideoPlayer == null)
		{
			return;
		}
		if (this.Target != null)
		{
			this.Target.texture = null;
		}
		Resources.UnloadAsset(this._VideoPlayer.clip);
		this._VideoPlayer = null;
	}

	// Token: 0x0400264D RID: 9805
	public RawImage Target;

	// Token: 0x0400264E RID: 9806
	public Material FMVMaterial;

	// Token: 0x0400264F RID: 9807
	private AudioSource _AudioSource;

	// Token: 0x04002650 RID: 9808
	private VideoPlayer _VideoPlayer;

	// Token: 0x04002651 RID: 9809
	private bool _IsPrepared;

	// Token: 0x04002652 RID: 9810
	private bool _Paused;
}
