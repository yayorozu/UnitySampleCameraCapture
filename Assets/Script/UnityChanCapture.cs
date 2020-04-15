using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace CameraCapture
{
	public class UnityChanCapture : MonoBehaviour
	{
		[SerializeField]
		private GameObject _gameObjectResource;
		[SerializeField]
		private Camera _camera;
		[SerializeField]
		private DirectionalLight _directionalLight;
		[SerializeField]
		private Transform _objRoot;
		
		[SerializeField, HideInInspector]
		private GameObject _instanceObj;
		[SerializeField, HideInInspector]
		private Animator _animator; 
		
		public Camera Camera => _camera;
		public Animator Animator
		{
			get
			{
				if (_animator == null)
				{
					_animator = _instanceObj.GetComponent<Animator>();
				}
				return _animator;
			}
		}

		internal void SetUp()
		{
			_instanceObj = Instantiate(_gameObjectResource, _objRoot);
			_animator = _instanceObj.GetComponent<Animator>();
		}
	}
}