using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranqPistol : MonoBehaviour
{
    [SerializeField]
    public Rigidbody projectile;
    public float projectileSpeed = 200;

    public static int maxAmmo = 1;
    public int currentAmmo = 1;

    // Start is called before the first frame update

    public void Shoot(Rigidbody projectile)
    {
        Rigidbody bullet = Instantiate(projectile, transform.GetChild(0).transform.position, transform.GetChild(0).transform.rotation);
        bullet.velocity = -transform.up * projectileSpeed;
        if (currentAmmo != 0) currentAmmo--;
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
