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
		[SerializeField]
		private GameObject _captureObject;
		[SerializeField]
		private Vector2Int _size = new Vector2Int(300, 300);

		private void Release()
		{
			if (_captureData == null)
				return;

			_captureData.Dispose();
			_captureData = null;
		}
		
		private void OnDestroy()
		{
			Release();
		}

		private void OnGUI()
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
					
					if (_captureData == null)
						_captureData = new CaptureData();
					_captureData.SetUp(this, _captureObject, _size);
				}
			}
			
			if (_captureData == null)
				return;
						
			_captureData.Draw();
		}
	}
}