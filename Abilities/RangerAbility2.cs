using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerAbility2 : Ability
{
    public RangerAbility2(Player player) : base(player)
    {
        windup = 0.0f;
        duration = 0.5f;
        cooldown = 5;
    }

    public override void OnUse(Hitbox hitbox)
    {
        player.rb.AddForce(new Vector2(4.0f*direction, 0.0f), ForceMode2D.Impulse);
    }
}
