using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemController : MonoBehaviour
{
    public ParticleSystemSettings systemSettings;

    [System.Serializable]
    public struct ParticleSystemSettings
    {
        public int maxParticles;
        public float particleSize;
    }

    ParticleSystem _particleSystem;

    ParticleSystem.Particle[] particles;
    ParticleData[] particleData;

    int lastSetParticleDataIndex = -1;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        ResetParticleSystem();
    }

    void ResetParticleSystem()
    {
        particles = new ParticleSystem.Particle[systemSettings.maxParticles];
        particleData = new ParticleData[systemSettings.maxParticles];

        for (int i = 0; i < particleData.Length; i++)
        {
            particleData[i] = new ParticleData();
        }

        _particleSystem.Clear();
    }

    public void SetParticle(RaycastHit collisionData)
    {
        int index = lastSetParticleDataIndex + 1;

        if(index < systemSettings.maxParticles)
        {
            ParticleData particle = new ParticleData();

            particle.position = collisionData.point + collisionData.normal * 0.01f;
            Vector3 normal = collisionData.normal;
            normal.z *= -1;
            particle.rotation = Quaternion.LookRotation(normal).eulerAngles;
            particleData[index] = particle;

            lastSetParticleDataIndex = index;
        }
    }

    public void ClearParticles()
    {
        for (int i = 0; i <= lastSetParticleDataIndex; i++)
        {
            particles[i].startColor = Color.clear;
        }

        lastSetParticleDataIndex = -1;
    }

    public void DisplayParticles()
    {
        for (int i = 0; i <= lastSetParticleDataIndex; i++)
        {
            particles[i].position = particleData[i].position;
            particles[i].rotation3D = particleData[i].rotation;
            particles[i].startSize = systemSettings.particleSize;
            particles[i].startColor = Color.white;
        }

        _particleSystem.SetParticles(particles, particles.Length);
    }
}
