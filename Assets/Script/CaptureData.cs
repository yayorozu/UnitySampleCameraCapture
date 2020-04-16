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

		public void SetUp(EditorWindow window, GameObject resource, Vector2Int size)
		{
			_window = window;
			
			if (_objectCaptureObject == null)
			{
				var instance = EditorGUIUtility.Load("ObjectCapture/ObjectCapture.prefab");
				_objectCaptureObject = (GameObject) GameObject.Instantiate(instance);
				_objectCaptureObject.name = instance.name;
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
		
		public void Draw()
		{
			if (_objectCaptureObject == null)
				_objectCaptureObject = GameObject.Find("ObjectCapture");
			if (_objectCaptureObject == null)
				return;
			
			if (_objectCapture == null)
				_objectCapture = _objectCaptureObject.GetComponent<ObjectCapture>();
			if (_objectCapture == null)
				return;
			
			if (_animator == null)
				_animator = _createObject.GetComponent<Animator>();
			if (_animator == null)
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
			_info?.DrawRenderTexture();
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				_animationIndex = EditorGUILayout.Popup("Animation", _animationIndex, clips);
				if (check.changed)
				{
					_maxLength = _animator.runtimeAnimatorController.animationClips[_animationIndex].length;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				_length = EditorGUILayout.Slider("NormarizedTime", _length, 0f, _maxLength);
				if (check.changed)
				{
					PlayAnimation(clips[_animationIndex], _length);
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Play"))
				{
					PlayAnimation(clips[_animationIndex]);
				}
				if (GUILayout.Button("Stop"))
				{
					StopAnimation();
				}
				if (GUILayout.Button("Capture"))
				{
					_info?.Capture();
				}
			}
		}
	}
}