using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{   
    [SerializeField] private int layerCount;
    [SerializeField] private List<string> sfxDictWords;
    [SerializeField] private List<AudioClip> sfxDictClips;

    private AudioSource[] sfxSources;
    
    private void Awake() 
    {
        GameObject sfxSourceRef =  new GameObject("sfx source");
        sfxSourceRef.AddComponent<AudioSource>();

        sfxSources = new AudioSource[layerCount];

        for (int i=0; i<layerCount; i++)  
            sfxSources[i] = Instantiate(sfxSourceRef,transform).GetComponent<AudioSource>();

        Destroy(sfxSourceRef);

    }

    private int GetFirstEmptyLayer() 
    {
        for (int i=0; i<layerCount; i++) 
            if (!sfxSources[i].isPlaying) 
                return i;

        return -1;
    }


    public void PlaySFX(string name, bool firstLayer = true) 
    {  
        int layer;

        if (firstLayer) { layer = 0;  sfxSources[layer].Stop();}
        else { layer = GetFirstEmptyLayer();  if (layer == -1) return; }

        if (name == "") return; 
        sfxSources[layer].PlayOneShot(sfxDictClips[sfxDictWords.IndexOf(name)]);
    }

   

}
