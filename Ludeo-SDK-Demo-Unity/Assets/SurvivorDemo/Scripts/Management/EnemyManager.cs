using LudeoSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    [Header("Asset Referenes")]
    public Enemy p_Enemy;

    [Header("Parameters")]
    public Vector2 _spawnArea = new Vector2(50, 50);

    public int EnemiesLeft => _enemyCountForLevel - _enemiesKilledForLevel;
    public bool AnyEnemyAlive => EnemiesLeft > 0;

    public int EnemyCountForLevel => _enemyCountForLevel;
    private int _enemyCountForLevel;

    public int EnemiesKilledTotal = 0;
    private int _enemiesKilledForLevel = 0;
    List<Enemy> _enemies;
    GameObject _enemiesParent;

    public IEnumerator GenerateEnemies(int level, Transform target)
    {
        if (level == 0)
        {
            EnemiesKilledTotal = 0;
        }

        _enemiesKilledForLevel = 0;
        _enemyCountForLevel = level * Random.Range(10, 20);

        if (_enemiesParent != null)
            Destroy(_enemiesParent);

        _enemiesParent = new GameObject("Enemies");
        _enemiesParent.transform.position = Vector3.zero;
        _enemiesParent.transform.rotation = Quaternion.identity;

        _enemies = new List<Enemy>(_enemyCountForLevel);
        System.Random rand = new System.Random((int)Time.time);

        for (int i = 0; i < _enemyCountForLevel; i++)
        {
            if (target == null)
                break;

            var enemy = Instantiate(p_Enemy, _enemiesParent.transform);
            const float size = 5f;
            var position = target.position + new Vector3(
                rand.Next((int)(-_spawnArea.x - level * size), (int)(_spawnArea.x + level * size)),
                1f,
                rand.Next((int)(-_spawnArea.y - level * size), (int)(_spawnArea.y + level * size)));

            enemy.transform.position = position;
            Debug.DrawLine(enemy.transform.position, target.position, Color.red, 5f);
            enemy.Init(target);
            if (enemy.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                agent.nextPosition = position;
            }
            _enemies.Add(enemy);

            enemy.Damage += level;

            if (level > 4)
            {
                var rand2 = Random.Range(0, 100);
                if (rand2 > 80)
                {
                    var randomAddon = Random.Range(1, (rand2 - 80) / 3f);
                    int hpbuff = (int)(level * randomAddon);
                    enemy.MaxHp += hpbuff;
                    enemy.transform.localScale *= 1 + randomAddon / 6f;
                }
            }

            if (level > 6)
            {
                enemy.MaxHp += level;
            }

            if (level > 8)
            {
                var rand2 = Random.Range(0, 100);
                if (rand2 > 80)
                {
                    var speedbuff = Mathf.Min(level / 2f, 20);
                    enemy.Speed += speedbuff;
                    enemy.transform.localScale -= Vector3.one * (speedbuff / 100f);
                }
            }

            enemy.OnDeath += OnEnemyDeath;
            yield return new WaitForSeconds(Random.Range(0f, 0.3f));
        }
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        EnemiesKilledTotal++;
        _enemiesKilledForLevel++;
        LudeoManager.SetGameplayState(LudeoWrapper.ENEMY_COUNT, EnemiesLeft);
        LudeoManager.SetGameplayState(LudeoWrapper.NORMAL_KILL, EnemiesKilledTotal);
        _enemies.Remove(enemy);
    }

    public Enemy GetClosestEnemyTo(Vector3 position, List<Transform> exclude = null)
    {
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;
        foreach (var enemy in _enemies)
        {
            if (exclude != null && exclude.Contains(enemy.transform))
                continue;

            var distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }
}
