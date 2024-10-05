using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueAbility1 : Ability
{
    public RogueAbility1(Player player) : base(player)
    {
        windup = 0.2f;
        duration = 0.2f;
        cooldown = 0;
    }

    public override void OnUse(Hitbox hitbox)
    {
        hitbox.damage = 15.0f;
        hitbox.gameObject.transform.localScale = new Vector3(1.1f,0.5f,1.0f);

        hitbox.followsSource = true;
        hitbox.offsetFromSource = new Vector3(0.8f*direction,0.8f,0.0f);;
    }
}
