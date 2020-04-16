using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CameraCapture
{
	[Serializable]
	public class CaptureData
	{
		[SerializeField]
		private GameObject _captureObject;
		[SerializeField]
		private Vector2Int _size = new Vector2Int(300, 300);
		[SerializeField]
		private Info _info;
		[SerializeField]
		private EditorWindow _window;
		[SerializeField]
		private Animator _animator;
		[SerializeField]
		private GameObject _createObject;
		[SerializeField]
		private GameObject _objectCaptureObject;
		private ObjectCapture _objectCapture;
		private Color _backgroundColor;
		private Vector3 _cameraPosition;
		private Vector3 _cameraRotation;
		private int _animationIndex;
		private float _maxLength;
		private float _length;

		public CaptureData(EditorWindow window)
		{
			_window = window;
		}

		public void SetUp(GameObject resource, Vector2Int size)
		{
			if (_objectCaptureObject == null)
			{
				var instance = EditorGUIUtility.Load("ObjectCapture/ObjectCapture.prefab");
				_objectCaptureObject = (GameObject) Object.Instantiate(instance);
				_objectCaptureObject.name = instance.name;
				_objectCaptureObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
			}

			_objectCapture = _objectCaptureObject.GetComponent<ObjectCapture>();

			if (_createObject == null)
				_createObject = Object.Instantiate(resource, _objectCapture.ObjRoot);

			_animator = _createObject.GetComponent<Animator>();
			if (_animator.runtimeAnimatorController.animationClips.Length > 0)
				_maxLength = _animator.runtimeAnimatorController.animationClips[_animationIndex].length;

			_backgroundColor = _objectCapture.Camera.backgroundColor;
			_cameraPosition = _objectCapture.Camera.transform.localPosition;
			_cameraRotation = _objectCapture.Camera.transform.localEulerAngles;
			if (_info == null)
				_info = new Info(_objectCapture.Camera, size);
			else
				_info.CreateRenderTexture(size);
		}

		public void Dispose()
		{
			_animator = null;
			Object.DestroyImmediate(_createObject);
			Object.DestroyImmediate(_objectCaptureObject);
			EditorApplication.update -= UpdateAnimation;
		}

		public void PlayAnimation(string name, float normalizedTime = -1f)
		{
			if (normalizedTime < 0)
				_animator.speed = 1f;
			else
				_animator.speed = 0f;

			_animator.Play(name, 0, normalizedTime);

			EditorApplication.update -= UpdateAnimation;
			if (normalizedTime >= 0f)
			{
				_animator.Update(Time.deltaTime);
				return;
			}

			EditorApplication.update += UpdateAnimation;
		}

		public void StopAnimation()
		{
			EditorApplication.update -= UpdateAnimation;
		}

		private void UpdateAnimation()
		{
			_window.Repaint();
			_animator.Update(Time.deltaTime);
		}

		private bool SetParam()
		{
			if (_objectCaptureObject == null)
				_objectCaptureObject = GameObject.Find("ObjectCapture");
			if (_objectCaptureObject == null)
				return false;

			if (_objectCapture == null)
				_objectCapture = _objectCaptureObject.GetComponent<ObjectCapture>();
			if (_objectCapture == null)
				return false;

			if (_animator == null)
				_animator = _createObject.GetComponent<Animator>();
			if (_animator == null)
				return false;

			return true;
		}

		public void OnGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUILayout.VerticalScope(GUILayout.Width(300)))
				{
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						_captureObject = (GameObject) EditorGUILayout.ObjectField("Capture Object", _captureObject, typeof(GameObject));
						_size = EditorGUILayout.Vector2IntField("Size", _size);
				
						if (check.changed)
						{
							if (_captureObject == null)
								return;
					
							if (_size.x <= 0 && _size.y <= 0)
								return;
					
							var anim = _captureObject.GetComponent<Animator>();
							if (anim == null)
							{
								Debug.LogWarning("Require Animator");
								return;
							}
					
							SetUp(_captureObject, _size);
						}
					}

					if (!SetParam())
						return;
						
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						_backgroundColor = EditorGUILayout.ColorField("BackgroundColor", _backgroundColor);
						_cameraPosition = EditorGUILayout.Vector3Field("CameraPosition", _cameraPosition);
						_cameraRotation = EditorGUILayout.Vector3Field("CameraRotation", _cameraRotation);
						if (check.changed)
						{
							_objectCapture.Camera.backgroundColor = _backgroundColor;
							_objectCapture.Camera.transform.localPosition = _cameraPosition;
							_objectCapture.Camera.transform.localEulerAngles = _cameraRotation;
						}
					}

					var clips = _animator.runtimeAnimatorController.animationClips.Select(ac => ac.name).ToArray();
					if (_animationIndex >= clips.Length)
						_animationIndex = 0;

					using (var check = new EditorGUI.ChangeCheckScope())
					{
						_animationIndex = EditorGUILayout.Popup("Animation", _animationIndex, clips);
						if (check.changed)
							_maxLength = _animator.runtimeAnimatorController.animationClips[_animationIndex].length;
					}

					using (var check = new EditorGUI.ChangeCheckScope())
					{
						_length = EditorGUILayout.Slider("NormarizedTime", _length, 0f, _maxLength);
						if (check.changed)
							PlayAnimation(clips[_animationIndex], _length);
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						if (GUILayout.Button("Play"))
							PlayAnimation(clips[_animationIndex]);
						if (GUILayout.Button("Stop"))
							StopAnimation();
					}
					if (GUILayout.Button("Capture"))
						_info?.Capture();
				}

				_info?.DrawRenderTexture();
			}
		}
	}
}