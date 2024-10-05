using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueAbility2 : Ability
{
    public RogueAbility2(Player player) : base(player)
    {
        windup = 0.0f;
        duration = 1.2f;
        cooldown = 10;

        preventsMovement = true;
        preventsSpriteFlip = true;
    }   

    public override void OnUse(Hitbox hitbox)
    {
        player.rb.AddForce(new Vector2(8.0f*-direction, 4.0f), ForceMode2D.Impulse);
    }
}
