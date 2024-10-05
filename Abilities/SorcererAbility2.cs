using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererAbility2 : Ability
{
    public SorcererAbility2(Player player) : base(player)
    {
        windup = 0.0f;
        duration = 5.0f;
        cooldown = 15;
        modSpeed = 2.0f;

        setImmune = true;
    }

    public override void OnUse(Hitbox hitbox)
    {
        hitbox.damage = 10.0f;
        hitbox.followsSource = true;
        hitbox.offsetFromSource = new Vector3(0.0f,0.2f,0.0f);
        hitbox.gameObject.transform.localScale = new Vector3(2.5f,0.3f,0.0f);
    }
}
