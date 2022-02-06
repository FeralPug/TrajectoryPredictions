using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchController : MonoBehaviour
{
    public float launchVelocity = 15f;
    public ProjectileController projectilePrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchProjectile();
        }
    }

    void LaunchProjectile()
    {
        ProjectileController projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        projectile.Rigidbody.velocity = transform.forward * launchVelocity;
    }
}

