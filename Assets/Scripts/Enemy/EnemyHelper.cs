using System.Collections.Generic;
using UnityEngine;

public class EnemyHelper : MonoBehaviour
{
	private List<Enemy> _enemyList = new List<Enemy>();

	public void AddEnemyToList(Enemy newEnemy)
	{
		if (!this._enemyList.Contains(newEnemy))
		{
			this._enemyList.Add(newEnemy);
		}
	}
	public void RemoveEnemyFromList(Enemy enemyToRemove)
	{
		if (this._enemyList.Contains(enemyToRemove))
		{
			this._enemyList.Remove(enemyToRemove);
		}
	}
	public List<Enemy> GetEnemyList()
	{
		return this._enemyList;
	}
}
