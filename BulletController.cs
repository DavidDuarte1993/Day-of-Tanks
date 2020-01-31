using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class BulletController : NetworkBehaviour {

    [Header("Bullet Configuration")]
    public float damage;

    [Header("Bullet References")]
    [SerializeField]
    private GameObject bulletExplore;
    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer)
        {
            CmdExplore();
            Destroy(gameObject);
        }
    }

    [Command]
    private void CmdExplore()
    {
        ParticleSystem par = Instantiate(bulletExplore, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        NetworkServer.Spawn(par.gameObject);
        par.Play();
        Destroy(par.gameObject, par.main.startLifetimeMultiplier);
    }
}
