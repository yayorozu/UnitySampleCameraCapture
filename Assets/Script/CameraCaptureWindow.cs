using System;
using UnityEditor;
using UnityEngine;

namespace CameraCapture
{
	internal class CameraCaptureWindow : EditorWindow
	{
		[MenuItem("Tools/Capture")]
		private static void ShowWindow()
		{
			var window = GetWindow<CameraCaptureWindow>();
			window.titleContent = new GUIContent("Capture");
			window.Show();
		}

		[SerializeField]
		private CaptureParam _unityChanCapture;

		private void OnEnable()
		{
		}
		
		private void OnDisable()
		{
			if (_unityChanCapture != null)
			{
				_unityChanCapture.Dispose();
				_unityChanCapture = null;
			}
		}

		private void OnGUI()
		{
			if (_unityChanCapture == null)
			{
				_unityChanCapture = new CaptureParam();
				_unityChanCapture.SetUp(this);
			}
			
			_unityChanCapture.Draw();
		}
	}

	[Serializable]
	public class CaptureParam
	{
		[SerializeField]
		private UnityChanCapture _unityChanCapture;
		[SerializeField]
		private Info _info;
		[SerializeField]
		private EditorWindow _window;
		
		public void SetUp(EditorWindow window)
		{
			if (_unityChanCapture != null)
				return;

			_window = window;
			
			var obj = EditorGUIUtility.Load("UnityChanCapture.prefab");
			_unityChanCapture = ((GameObject) GameObject.Instantiate(obj)).GetComponent<UnityChanCapture>();
			_unityChanCapture.name = obj.name;
			_unityChanCapture.SetUp();
			
			_info = new Info(_unityChanCapture.Camera, new Vector2Int(500, 500));
		}

		public void Dispose()
		{
			GameObject.DestroyImmediate(_unityChanCapture);
			EditorApplication.update -= UpdateAnimation;
		}

		public void PlayAnimation(string name)
		{
			_unityChanCapture.Animator.speed = 1f;
			_unityChanCapture.Animator.Play(name);
			
			EditorApplication.update -= UpdateAnimation;
			EditorApplication.update += UpdateAnimation;
		}

		public void StopAnimation()
		{
			EditorApplication.update -= UpdateAnimation;
		}

		private void UpdateAnimation()
		{
			_window.Repaint();
			_unityChanCapture.Animator.Update(Time.deltaTime);
		}
		
		public void Draw()
		{
			if (_info != null)
			{
				// var pos = _camera.transform.localPosition;
				// pos.z = EditorGUILayout.FloatField("Z", pos.z);
				// _camera.transform.localPosition = pos;
			}
			_info?.DrawRenderTexture();

			if (GUILayout.Button("Play"))
			{
				PlayAnimation("RUN00_F");
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