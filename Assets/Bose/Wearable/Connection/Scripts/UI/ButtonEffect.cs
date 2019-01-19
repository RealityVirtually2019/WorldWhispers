using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bose.Wearable
{
	public class ButtonEffect : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
	{
		[Header("UX Refs")]
		[SerializeField]
		private RectTransform _buttonRectTransform;

		[Header("Animation"), Space(5)]
		[SerializeField]
		private AudioClip _sfxClick;

		[Header("Animation"), Space(5)]
		[SerializeField]
		[Range(0, float.MaxValue)]
		private float _scaleSpeed = 15f;

		[SerializeField]
		private Vector3 _scaleDown = new Vector3(1.2f, 1.2f, 1.2f);

		private AudioControl _audioControl;
		private Coroutine _scaleCoroutine;

		private void Start()
		{
			_audioControl = AudioControl.Instance;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			// scale up
			if (_scaleCoroutine != null)
			{
				StopCoroutine(_scaleCoroutine);
				_scaleCoroutine = null;
			}

			_scaleCoroutine = StartCoroutine(ScaleRectTransform(_buttonRectTransform, Vector3.one));
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			// scale down
			if (_scaleCoroutine != null)
			{
				StopCoroutine(_scaleCoroutine);
				_scaleCoroutine = null;
			}

			_scaleCoroutine = StartCoroutine(ScaleRectTransform(_buttonRectTransform, _scaleDown));
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			PlayClickSting();
		}

		private IEnumerator ScaleRectTransform(RectTransform rectTransform, Vector3 targetScale)
		{
			var waitForEndOfFrame = new WaitForEndOfFrame();
			while (true)
			{
				rectTransform.localScale = Vector3.Lerp(
					rectTransform.localScale,
					targetScale,
					_scaleSpeed * Time.unscaledDeltaTime);
				yield return waitForEndOfFrame;
			}
		}

		private void PlayClickSting()
		{
			if (_sfxClick != null)
			{
				_audioControl.PlayOneShot(_sfxClick);
			}
		}
	}
}
