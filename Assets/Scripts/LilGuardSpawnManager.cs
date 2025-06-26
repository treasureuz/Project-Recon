using UnityEngine;

public class LilGuardSpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _copEnemy;

	[Header("Spawn Points")]
	[SerializeField] private Transform[] _spawnPoints = new Transform[2];

	private void Awake()
	{
		this._copEnemy = GameObject.Find("CopEnemy").transform; 
		this._spawnPoints = GetComponentsInChildren<Transform>();
	}

	private void Start()
	{
		this.transform.position = this._copEnemy.position;
	}

	private void Update()
	{
		if (this._copEnemy != null)
		{
			RedirectSpawnPoints();
		}
	}

	private void RedirectSpawnPoints()
	{
		this.transform.position = new Vector3(this._copEnemy.position.x, this._copEnemy.position.y, this.transform.position.z);
		this.transform.rotation = this._copEnemy.rotation;
	}

	#region Getters and Setters
	public Vector3 GetTopSpawnPoint()
	{
		return this._spawnPoints[0].position;
	}

	public Vector3 GetBottomSpawnPoint()
	{
		return this._spawnPoints[1].position;
	}
	#endregion
}
