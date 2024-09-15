using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScrewdriver : MonoBehaviour
{

    private void Start()
    {
        ItemSpawner[] screwdriverSpawners = GetComponentsInChildren<ItemSpawner>();
        screwdriverSpawners[Randomize.Index(screwdriverSpawners.Length)].Spawn();
    }

}
