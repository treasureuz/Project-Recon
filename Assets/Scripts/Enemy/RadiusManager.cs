using System.Runtime.CompilerServices;
using UnityEngine;

public class RadiusManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private LayerMask _playerLayerMask;
	[SerializeField] private CircleCollider2D _radius;

	private void Awake()
	{ 
		this._radius = GetComponent<CircleCollider2D>();
	}

	public bool IsWithinRadius()
	{
		return Physics2D.OverlapCircle(this._radius.transform.position, this._radius.bounds.extents.x, this._playerLayerMask);
	}
}
