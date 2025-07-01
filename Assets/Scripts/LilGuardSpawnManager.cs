using UnityEngine;

public class LilGuardSpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy _copEnemy;

	[Header("Spawn Points")]
	[SerializeField] private Transform[] _spawnPoints = new Transform[2];

	private void Awake()
	{
		this._copEnemy = GameObject.Find("CopEnemy").GetComponent<Enemy>(); 
		this._spawnPoints = GetComponentsInChildren<Transform>();
	}

	private void Start()
	{
		this.transform.position = this._copEnemy.transform.position;
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
		this.transform.position = new Vector2(this._copEnemy.transform.position.x, this._copEnemy.transform.position.y);
		this.transform.rotation = this._copEnemy.transform.rotation;
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
