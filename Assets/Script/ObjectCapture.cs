using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace CameraCapture
{
	public class ObjectCapture : MonoBehaviour
	{
		[SerializeField]
		private Camera _camera;
		[SerializeField]
		private DirectionalLight _directionalLight;
		[SerializeField]
		private Transform _objRoot;
		
		public Transform ObjRoot => _objRoot;
		public Camera Camera => _camera;
		public DirectionalLight Light => _directionalLight;
	}
}