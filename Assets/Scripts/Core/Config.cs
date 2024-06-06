using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Config/Config", order = 51)]
public class Config : ScriptableObject
{
    public float PlayerDefaultAttackRadius = 2.5f;
    public float PlayerDefaultAttackForce = 1.0f;
    public float PlayerDefaultMoveSpeed = 1.0f;
    public float AttackUpdateTime = 0.1f;
    public int AttackedEnemyMaxCount = 2;
    public float EenemyDefaultHp = 4f;
    public int MaxEnemySpawnCount = 25;
    public float EnemySpawnInterval = 5f;

    private static Config _instance;
    public static Config Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<Config>("Config/Config");
            }

            return _instance;
        }
    }
}
