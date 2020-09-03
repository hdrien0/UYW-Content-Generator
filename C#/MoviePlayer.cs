using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(AudioSource))]
public class MoviePlayer : MonoBehaviour
{
	public bool IsPlaying
	{
		get
		{
			return this._VideoPlayer != null && (this._VideoPlayer.isPlaying || this._Paused);
		}
	}

	public float Width
	{
		get
		{
			return 1280f;
		}
	}

	public float Height
	{
		get
		{
			return 720f;
		}
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		this.Unload();
	}

	public IEnumerator Play(string moviePath, bool loop = false, bool renderOnCamera = false)
	{
		this._IsPrepared = false;
		if (this._VideoPlayer == null)
		{
			this._VideoPlayer = base.GetComponent<VideoPlayer>();
			if (this._VideoPlayer == null)
			{
				this._VideoPlayer = base.gameObject.AddComponent<VideoPlayer>();
			}
		}
		this._VideoPlayer.waitForFirstFrame = true;
		this._AudioSource = base.GetComponent<AudioSource>();
		this._VideoPlayer.source = VideoSource.Url;
		this._VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		//BEGIN MODIFICATION
		if (moviePath.Contains("NewContent"))	
		{	
			this._VideoPlayer.url = moviePath;	
			this._VideoPlayer.aspectRatio = VideoAspectRatio.FitInside;	
		}	
		else	
		{	
			this._VideoPlayer.url = Path.Combine(Application.streamingAssetsPath, moviePath + ".mp4");	
		}
		//END MODIFICATION
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
		if (this.Target != null)
		{
			this.Target.color = Color.black;
		}
		this._VideoPlayer.Play();
		this._AudioSource.Play();
		float prepareTimer = 0f;
		while (!this._IsPrepared && prepareTimer < 2f)
		{
			prepareTimer += Time.deltaTime;
			yield return null;
		}
		yield return Yielders.Seconds(0.1f);
		if (!renderOnCamera)
		{
			this.Target.texture = this._VideoPlayer.texture;
		}
		if (this.Target != null)
		{
			this.Target.color = Color.white;
		}
		yield break;
	}

	private void OnPrepareComplete(VideoPlayer source)
	{
		source.prepareCompleted -= this.OnPrepareComplete;
		this._IsPrepared = true;
	}

	public IEnumerator PlayAndWait(string moviePath, bool loop = false)
	{
		yield return this.Play(moviePath, loop, false);
		while (this.IsPlaying)
		{
			yield return null;
		}
		yield break;
	}

	public void Stop()
	{
		if (this._VideoPlayer == null)
		{
			return;
		}
		this._VideoPlayer.Stop();
	}

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
			return;
		}
		this._VideoPlayer.Play();
	}

	public void Unload()
	{
		if (this._VideoPlayer == null)
		{
			return;
		}
		if (this.Target != null)
		{
			this.Target.material.SetTexture("_MainTex", null);
			this.Target.material.SetTexture("_CromaTex", null);
		}
		Resources.UnloadAsset(this._VideoPlayer.clip);
		this._VideoPlayer = null;
	}

	public RawImage Target;

	public Material FMVMaterial;

	private AudioSource _AudioSource;

	private VideoPlayer _VideoPlayer;

	private bool _IsPrepared;

	private bool _Paused;
}
