using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Plays reactive layered audio corresponding to the player's closeness to the target. 
	/// </summary>
	public class TargetSFX : MonoBehaviour
	{

		/// <summary>
		/// The audio layer to apply when closest to the target.
		/// </summary>
		[SerializeField]
		protected AudioSource _audioClose;

		/// <summary>
		/// The audio layer to apply when approaching the target.
		/// </summary>
		[SerializeField]
		protected AudioSource _audioMiddle;

		/// <summary>
		/// The audio layer to apply when far from the target.
		/// </summary>
		[SerializeField]
		protected AudioSource _audioFar;

		/// <summary>
		/// Threshold in [0, 1] at which point <see cref="TargetSFX._audioMiddle"/> begins fading in. 
		/// </summary>
		[SerializeField]
		protected float _middleThreshold;

		/// <summary>
		/// Threshold in [0, 1] at which point <see cref="TargetSFX._audioClose"/> begins fading in. 
		/// </summary>
		[SerializeField]
		protected float _closeThreshold;

		/// <summary>
		/// The closeness to the target, from 0 to 1. Typically set by the <see cref="TargetController"/>.
		/// </summary>
		public float Closeness
		{
			set { _closeness = value; }
			get { return _closeness; }
		}

		private AudioHighPassFilter _audioCloseFilter;
		private AudioHighPassFilter _audioMiddleFilter;
		private AudioLowPassFilter _audioFarFilter;

		private float _closeness;

		private void Start()
		{
			_audioFarFilter = _audioFar.GetComponent<AudioLowPassFilter>();
			_audioMiddleFilter = _audioMiddle.GetComponent<AudioHighPassFilter>();
			_audioCloseFilter = _audioClose.GetComponent<AudioHighPassFilter>();

			// Schedule the audio clips to play slightly in the future so they have time to sync.
			double startTime = AudioSettings.dspTime + 0.01;
			_audioClose.PlayScheduled(startTime);
			_audioMiddle.PlayScheduled(startTime);
			_audioFar.PlayScheduled(startTime);
		}


		private void Update()
		{
			// Each factor ranges from 0 to 1 and controls the relative volume of each layer
			float farFactor = _closeness;
			float middleFactor = Mathf.Clamp01((_closeness - _middleThreshold) / (1.0f - _middleThreshold));
			float closeFactor = Mathf.Clamp01((_closeness - _closeThreshold) / (1.0f - _closeThreshold));

			// Far layer low pass maps exponentially from ~150 Hz to ~3000 Hz
			_audioFarFilter.cutoffFrequency = Mathf.Exp(farFactor * 3.0f + 5.0f);
			
			// Middle layer high pass maps exponentially from ~22000 Hz to ~55 Hz
			_audioMiddleFilter.cutoffFrequency = Mathf.Exp(10.0f - middleFactor * 6.0f);
			
			// High layer high pass maps exponentially from ~22000 Hz to ~55 Hz
			_audioCloseFilter.cutoffFrequency = Mathf.Exp(10.0f - closeFactor * 6.0f);


			// Keep sync between each layer using _audioClose as the master
			if (_audioClose.timeSamples != _audioMiddle.timeSamples)
			{
				_audioMiddle.timeSamples = _audioClose.timeSamples;
			}

			if (_audioClose.timeSamples != _audioFar.timeSamples)
			{
				_audioFar.timeSamples = _audioClose.timeSamples;
			}

		}
	}
}
