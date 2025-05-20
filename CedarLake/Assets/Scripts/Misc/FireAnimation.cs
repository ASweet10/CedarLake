using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAnimation : MonoBehaviour
{
    [SerializeField] ParticleSystem lighterFluidParticles;
    [SerializeField] ParticleSystem zippoFire;
    [SerializeField] AudioSource fireSlosh;
    [SerializeField] AudioSource fireLighter;
    [SerializeField] AudioSource firePour;

    public void PourLighterFluid() {
        lighterFluidParticles.Play();
    }

    public void StartZippoFire() {
        zippoFire.Play();
    }
    public void PlayFireSlosh() {
        fireSlosh.Play();
    }
    public void PlayFireLighter() {
        fireLighter.Play();
    }
    public void PlayFirePour() {
        firePour.Play();
    }
}
