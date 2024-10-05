using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightAbility2 : Ability
{
    public KnightAbility2(Player player) : base(player)
    {
        windup = 0.2f;
        duration = 1.2f;
        cooldown = 15;
    }

    public override void OnUse(Hitbox hitbox)
    {
        hitbox.damage = 20.0f;
        hitbox.followsSource = true;
        hitbox.offsetFromSource = new Vector3(0.0f,0.8f,0.0f);
        hitbox.gameObject.transform.localScale = new Vector3(3.9f,1.6f,1.0f);
    }
}
