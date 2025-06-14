using UnityEngine;

public class AsteroidBehavior : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;

	[Header("Asteroid Settings")]
	[SerializeField] private float _asteroidForce = 2f;
	[SerializeField] private float _maxHealth = 30f;
	[SerializeField] private float _asteroidDamage = 50f; // Damage dealt to player on collision
	[SerializeField] private float _asteroidRotationSpeed = 70f; // Speed of asteroid rotation

	private float _currentHealth;

	private void Start()
	{
		LaunchAsteroid();
		this._currentHealth = this._maxHealth; // Initialize health
	}

	private void Update()
	{
		this.transform.Rotate(0, 0, this._asteroidRotationSpeed * _asteroidForce * Time.deltaTime);
	}

	private void LaunchAsteroid()
	{
		// Both do the same thing
		//this._rb2d.linearVelocity = transform.right * this._asteroidForce;
		this._rb2d.AddForce(Vector2.left * this._asteroidForce, ForceMode2D.Impulse);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		RectTransform rectTransform = collision.GetComponent<RectTransform>();
		PBulletManager col = collision.GetComponent<PBulletManager>();

		IDamageable iDamageable = this.GetComponent<IDamageable>();
		
		if (collision.gameObject.layer == LayerMask.NameToLayer("Player") || collision.CompareTag("Backdrop"))
		{
			Destroy(gameObject);
		}
		else if (collision.gameObject.CompareTag("PlayerBullet"))
		{
			iDamageable.OnDamaged(col.GetPlayerBulletDamage());

			rectTransform.position = new Vector3
				(rectTransform.position.x + .35f, rectTransform.position.y - 0.1f, rectTransform.position.z);
			UIManager.instance.SpawnDamageText(rectTransform.position, (int) col.GetPlayerBulletDamage());
		}
	}

	#region Getters 
	public float GetAsteroidDamage()
	{
		return this._asteroidDamage;
	}

	#endregion

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		this._currentHealth -= damageAmount;
		if (this._currentHealth <= 0)
		{
			Destroy(gameObject); // Destroy asteroid when health reaches zero
		}
	}

	#endregion

	
}
