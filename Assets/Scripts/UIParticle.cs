using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticle : MonoBehaviour
{
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private bool isTest;
    private GameObject particleWorld;
    private ParticleSystem particleSystemWorld;
    private WaitForSecondsRealtime particleDurationWFS;
    private IEnumerator currentRoutine;


    void Awake() 
    {
        particleWorld = Instantiate(particlePrefab,transform);
        particleSystemWorld = particleWorld.GetComponent<ParticleSystem>();
        particleDurationWFS = new WaitForSecondsRealtime(particleSystemWorld.main.duration);

        particleWorld.transform.position = transform.position + Vector3.back;        
        particleWorld.transform.localScale = new Vector3(1/transform.lossyScale.x, 1/transform.lossyScale.y,1/transform.lossyScale.z);
        
        particleWorld.SetActive(true);

    }

    void Update() 
    {
        if (isTest) 
        {
            if(Input.GetMouseButtonDown(0)) Fire();    
        }

    }
    private IEnumerator FireRoutine() 
    {
        particleSystemWorld.Play();
        yield return particleDurationWFS;
        particleSystemWorld.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        currentRoutine = null;
    }
    
    public void Fire() 
    {   
        if (currentRoutine != null) 
        {
            StopCoroutine(currentRoutine);
            particleSystemWorld.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        currentRoutine = FireRoutine();
        StartCoroutine(currentRoutine);
    }

    public void SafeSetPosition(Vector3 newPos) 
    {
        newPos.z = transform.position.z;
        transform.position = newPos;
    }

    public void SafeSetPosition(Transform posTransform) 
    {
        Vector3 newPos = posTransform.position;
        newPos.z = transform.position.z;
        transform.position = newPos;
    }


}
