using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCharacteristics : VitalCharacteristics
{
    [Space]
    public GameObject flyingDamagePrefab;
    public Vector3 offset;

    private Rigidbody2D r2d;

    [Header("Balance data")]
    private SaveDataToPlainTextFile balanceManager;
    private float totalDamage;
    private int totalStrokes;

    [Header("health")]
    public PlayerUILifeController playerUILifeController;
    public SceneManagerNPCState sceneManagerNPCState;

    [Header("Damage evasion")]
    public float dodge�hance = 0f;

    [Header("PlayerEnemiesSpeedIncreaseSpecification")]
    public int countRoomNoDamage = 0;
    public float percent;
    private PlayerEnemiesSpeedIncreaseSpecification playerEnemiesSpeedIncreaseSpecification;
    private PlayerMovement playerMovement;
    private RoomSpawnEnemies currentRoomsSpawnEnemies;

    [Header("Audio")]
    public PlayerSoundtrack playerSoundtrack;


    private void Start()
    {
        percent = 0f;

        // UI Change Cell Life
        //fact hp
        var sceneManager = GameObject.FindGameObjectWithTag("SceneManager");
        if (sceneManager != null)
        {
            sceneManagerNPCState = sceneManager.GetComponent<SceneManagerNPCState>();
            if (sceneManagerNPCState != null)
            {
                healthMax = sceneManagerNPCState.healthGGStart + (sceneManagerNPCState.healthGGEnd * (sceneManagerNPCState.floorNumber - 1));
            }
        }
        else
        {
            healthMax = 7;
        }
        health = healthMax;
        //ui
        if (sceneManager != null)
        {
            playerUILifeController = sceneManager.GetComponent<PlayerUILifeController>();
            if (playerUILifeController != null)
            {
                playerUILifeController.ChangeCountCell(this);
            }
        }    
     
        r2d = GetComponent<Rigidbody2D>();

        //balance
        var balanceManagerSource = GameObject.FindGameObjectWithTag("BalanceManager");
        if (balanceManagerSource != null)
        {
            balanceManager = balanceManagerSource.GetComponent<SaveDataToPlainTextFile>();
            totalDamage = 0;
            totalStrokes = 0;
        }
        else
        {
            Debug.LogWarning("Balance manager = null");
        }
    }

    private void Update()
    {
        if (r2d != null)
            r2d.WakeUp();   // ���������� ������� ���� �������� (��� �������� � StayTrigger)
    }

    private void OnDestroy()
    {
        SceneManager.LoadScene(9);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("CenterRoom"))
        {
            if (playerEnemiesSpeedIncreaseSpecification == null)
                playerEnemiesSpeedIncreaseSpecification = GetComponent<PlayerEnemiesSpeedIncreaseSpecification>();

            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();

            currentRoomsSpawnEnemies = collision.GetComponent<RoomSpawnEnemies>();

            if (currentRoomsSpawnEnemies.isSpawned)
            {
                countRoomNoDamage++;
                Debug.Log($"������� ����� � ������, �� �����: {countRoomNoDamage}");
                if (countRoomNoDamage > 2)
                {
                    percent += 3;
                    ChangeSpeed();
                }
            }
        }
    }

    private void ChangeSpeed()
    {
        if (percent > 0)
            StartCoroutine(playerEnemiesSpeedIncreaseSpecification.IncreaseSpeed(sceneManagerNPCState, currentRoomsSpawnEnemies, playerMovement, percent));
    }


    public override void DealDamage(float damage)
    {
        if (!IsDodgeChance())   // ���� �� ����������, �� �������� ����
        {
            // ����� ����� �������� �������� ������
            Debug.Log("����� ������ ������");
            countRoomNoDamage = 0;
            percent -= 3;
            if (percent < 0)
            {
                percent = 0;
            }
            ChangeSpeed();
            
            //
            health -= damage;
            CheckDeath();
            CallFlyingDamage(damage);

            //audio
            if (playerSoundtrack != null)
                playerSoundtrack.PlaySound(false);

            // UI Change Cell Life
            if (playerUILifeController != null)
            {
                playerUILifeController.ChangeCountCell(this);
            }
        }
        else // ���� ����������, �� ���� �� ������
        {
            CallFlyingDamage("evasion");
            damage = 0; // for balance
        }

        //balance
        if (balanceManager != null)
        {
            totalDamage += damage;
            totalStrokes++;
        }
    }

    /// <summary>
    /// ���������
    /// </summary>
    /// <param name="value">��������� ��������� ������� ��������</param>
    public void Treatment(float value)
    {
        health += value;
        if (health > healthMax)
            health = healthMax;

        // UI Change Cell Life
        if (playerUILifeController != null)
        {
            playerUILifeController.ChangeCountCell(this);
        }
    }

    private void CheckDeath()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void CallFlyingDamage(float damage)
    {
        var inst = Instantiate(flyingDamagePrefab);
        inst.transform.parent = gameObject.transform;
        inst.transform.position = transform.position + offset;
        inst.GetComponent<FlyingDamage>().damage = damage;
    }

    private void CallFlyingDamage(string damageText)
    {
        var inst = Instantiate(flyingDamagePrefab);
        inst.transform.parent = gameObject.transform;
        inst.transform.position = transform.position + offset;
        inst.GetComponent<FlyingDamage>().damage = 0;
        inst.GetComponent<FlyingDamage>().damageText = damageText;
    }

    public void IncreaseMaxHealth(int magnificationAmount)
    {
        healthMax += magnificationAmount;
        if (playerUILifeController != null)
        {
            playerUILifeController.ChangeCountCell(this);
        }
    }

    private bool IsDodgeChance()
    {
        return Random.Range(0, 100) <= dodge�hance;
    }

    //balance
    public void ResetToatalDamageAndStrokes()
    {
        totalDamage = 0;
        totalStrokes = 0;
    }

    //balance
    public float GetTotalDamage()
    {
        return totalDamage;
    }

    //balance
    public int GetTotalStrokes()
    {
        return totalStrokes;
    }
}
