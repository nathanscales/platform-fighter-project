using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightAbility1 : Ability
{
    public KnightAbility1(Player player) : base(player)
    {
        windup = 0.5f;
        airborneWindup = 0.4f;
        duration = 0.1f;
        cooldown = 1;

        usableWhileAirborne = true;
        preventsMovement = true;
    }

    public override void OnUse(Hitbox hitbox)
    {
        hitbox.damage = 10.0f;
        hitbox.gameObject.transform.position = player.transform.position;

        if (player.airborne)
        {
            hitbox.gameObject.transform.localScale = new Vector3(2.3f,0.7f,1.0f);
            hitbox.gameObject.transform.position += new Vector3(1.5f*direction,1.2f,0.0f);

        }
        else
        {
            hitbox.gameObject.transform.localScale = new Vector3(1.4f,1.4f,1.0f);
            hitbox.gameObject.transform.position += new Vector3(1.2f*direction,0.67f,0.0f);
        }
    }
}
