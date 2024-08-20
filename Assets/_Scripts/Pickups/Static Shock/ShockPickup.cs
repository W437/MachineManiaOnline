using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewShockPickup", menuName = "Pickup/Shock")]
public class ShockPickup : Pickup
{
    [SerializeField]ShockBehavior shockBehavior;
    NetworkObject playerWhoPlacedIt;
    PlayerController player;
    
    public override void Use(PlayerController player)
    {

        if (player.TryGetComponent(out ShockBehavior shock))
        {
            shock.Init();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        
    }
}
