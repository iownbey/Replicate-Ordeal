using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Single {get; private set;}

    void Awake() {
        if(Game.Single != null) { Destroy(gameObject); }        
        else { Game.Single = this; }
    }
}
