using System;
using UnityEditor;
using UnityEngine;

namespace CameraCapture
{
	public class CameraCaptureWindow : EditorWindow
	{
		[MenuItem("Tools/Capture")]
		private static void ShowWindow()
		{
			var window = GetWindow<CameraCaptureWindow>();
			window.titleContent = new GUIContent("Capture");
			window.Show();
		}

		[SerializeField]
		private CaptureData _captureData;
		
		private void Release()
		{
			if (_captureData == null)
				return;

			_captureData.Dispose();
			_captureData = null;
		}

		private void OnEnable()
		{
			if (_captureData == null)
				_captureData = new CaptureData(this);
		}

		private void OnDestroy()
		{
			Release();
		}

		private void OnGUI()
		{
			_captureData.OnGUI();
		}
	}
}