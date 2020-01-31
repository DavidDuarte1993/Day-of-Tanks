using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {

    [Header("Tank Control Configuration")]
    [SerializeField] private float tankMovingSpeed = 10f;
    [SerializeField] private float chassisRotatingSpeed = 70f;
    [SerializeField] private float turretRotatingSpeed = 20f;
    [SerializeField] private float shootingDelay = 0.1f;
    [SerializeField] private float bulletSpeed = 70f;
    [SerializeField] private int reviveDelay = 5;
    [SerializeField] private float maxHealth = 100f;

    [Header("Multiplayer References")]
    [SerializeField] private Material localPlayerMaterial;


    [Header("Tank References")]
    [SerializeField] private Transform turretTrans;
    [SerializeField] private Transform chassisTrans;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private GameObject tankExplore;
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private Image healthBar;


    private Rigidbody rigid;
    private GetMousePosition getMousePosition;
    private Vector3 movingDirection = Vector3.zero;
    private Vector3 turretRotatingDir = Vector3.zero;

    [SyncVar(hook = "UpdateHealthBar")]
    private float currentHealth = 100f;

    [HideInInspector] [SyncVar] public bool isDead = false;

    private void Start () {

        rigid = GetComponent<Rigidbody>();
        getMousePosition = FindObjectOfType<GetMousePosition>();
        movingDirection = chassisTrans.TransformDirection(Vector3.forward);

        if (isLocalPlayer)
        {
            ResetPlayer();
            StartCoroutine(Shooting());
        }       
    }

	private void FixedUpdate () {

        if (isLocalPlayer)
        { 
            if (Input.GetKey(KeyCode.W))
            {
                rigid.velocity = movingDirection.normalized * tankMovingSpeed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                rigid.velocity = -movingDirection.normalized * tankMovingSpeed;
            }


            if (Input.GetKey(KeyCode.D))
            {
                chassisTrans.eulerAngles += new Vector3(0, chassisRotatingSpeed * Time.deltaTime, 0);
                movingDirection = chassisTrans.TransformDirection(Vector3.forward);
            }

            if (Input.GetKey(KeyCode.A))
            {
                chassisTrans.eulerAngles -= new Vector3(0, chassisRotatingSpeed * Time.deltaTime, 0);
                movingDirection = chassisTrans.TransformDirection(Vector3.forward);
            }

            Vector3 mouseWorldPos = new Vector3(getMousePosition.X, turretTrans.position.y, getMousePosition.Z);
            turretRotatingDir = mouseWorldPos - turretTrans.transform.position;
            Quaternion designRot = Quaternion.LookRotation(turretRotatingDir);
            turretTrans.rotation = Quaternion.Slerp(turretTrans.rotation, designRot, turretRotatingSpeed * Time.deltaTime);
        }     
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Finish"))
        {
            TakeDamage(collision.gameObject.GetComponent<BulletController>().damage);
        }
    }

    [Command]
    private void CmdFire()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletPoint.position, Quaternion.identity) as GameObject;
        bullet.transform.eulerAngles = turretTrans.eulerAngles;
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
        NetworkServer.Spawn(bullet);
    }

    private void UpdateHealthBar(float value)
    {
        healthBar.fillAmount = value / maxHealth;
    }

    private void TakeDamage(float damage)
    {
        if (isServer)
        {
            currentHealth -= damage;
            if (currentHealth <= 0 && !isDead) //Player die
            {
                isDead = true;
                OnefallGames.SoundManager.Instance.PlaySound(OnefallGames.SoundManager.Instance.explode);
                RpcDie();
            }
        }     
    }

    [ClientRpc]
    private void RpcDie()
    {
        SetActiveState(false);
        CmdTankExplore();
        StartCoroutine(Respawn());
    }

    [Command]
    private void CmdTankExplore()
    {
        ParticleSystem explore = Instantiate(tankExplore, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0))).GetComponent<ParticleSystem>();
        explore.Play();
        NetworkServer.Spawn(explore.gameObject);
        Destroy(explore, explore.main.startLifetimeMultiplier);
    }

    private void SetActiveState(bool active)
    {
        turretTrans.gameObject.SetActive(active);
        chassisTrans.gameObject.SetActive(active);
        healthCanvas.SetActive(active);
    }

    private void ResetPlayer()
    {
        currentHealth = maxHealth;
        healthBar.fillAmount = 1;
        SetActiveState(true);
        isDead = false;
    }

    private IEnumerator Shooting()
    {
        bool isReload = false;

        while (!isDead)
        {
            if (!isReload)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isReload = true;
                    OnefallGames.SoundManager.Instance.PlaySound(OnefallGames.SoundManager.Instance.fire);
                    CmdFire();
                }
            }
            else
            {
                yield return new WaitForSeconds(shootingDelay);
                isReload = false;
            }
            yield return null;
        }
    }

    private IEnumerator Respawn()
    {
        NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        rigid.velocity = Vector3.zero;
        if (isLocalPlayer)
            UIManager.Instance.StartCountDown(reviveDelay);
        yield return new WaitForSeconds(reviveDelay);
        ResetPlayer();
    }
    //Networking
    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        turretTrans.GetComponent<MeshRenderer>().material = localPlayerMaterial;
        chassisTrans.GetComponent<MeshRenderer>().material = localPlayerMaterial;
    }


}
