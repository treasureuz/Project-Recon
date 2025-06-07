using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
	[SerializeField] private CinemachineCamera _followCamera;

	private void Awake()
	{
		SwitchToFollowCamera();
	}

	private void SwitchToFollowCamera()
	{
		this._followCamera.enabled = true;
	}

}
