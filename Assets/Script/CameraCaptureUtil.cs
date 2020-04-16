using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CameraCapture
{
	[Serializable]
	public class Info
	{
		[SerializeField]
		private Camera _camera;
		[SerializeField]
		private RenderTexture _renderTexture;

		public Info(Camera camera, Vector2Int size)
		{
			_camera = camera;
			CreateRenderTexture(size);
		}

		~Info()
		{
			_camera.targetTexture = null;
			if (_renderTexture != null)
				Object.DestroyImmediate(_renderTexture);
		}

		public void CreateRenderTexture(Vector2Int size)
		{
			if (_renderTexture != null)
			{
				_camera.targetTexture = null;
				Object.DestroyImmediate(_renderTexture);
			}

			_renderTexture = new RenderTexture(size.x, size.y, 1)
			{
				format = RenderTextureFormat.ARGB32
			};

			_renderTexture.Create();
			_camera.targetTexture = _renderTexture;
		}

		public void DrawRenderTexture()
		{
			if (_renderTexture == null)
				return;

			_renderTexture.Release();
			_camera.Render();
			var rect = GUILayoutUtility.GetRect(_renderTexture.width, _renderTexture.height, GUILayout.MaxWidth(_renderTexture.width), GUILayout.MaxHeight(_renderTexture.height));
			EditorGUI.DrawTextureTransparent(rect, _renderTexture);
		}

		public void Capture()
		{
			var savePath = EditorUtility.SaveFilePanel("Select Save Path", "", "", "png");

			if (string.IsNullOrEmpty(savePath) || !savePath.EndsWith(".png"))
				return;

			_camera.Render();
			
			// 別にEditorだったらこの処理いらない
			var cache = RenderTexture.active;

			RenderTexture.active = _renderTexture;
			var texture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
			texture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0, false);
			texture.Apply();

			System.IO.File.WriteAllBytes(savePath, texture.EncodeToPNG());

			Object.DestroyImmediate(texture);
			RenderTexture.active = cache;
		}
	}
}