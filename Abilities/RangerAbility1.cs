using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerAbility1 : Ability
{
    private Sprite arrow;

    public RangerAbility1(Player player, Sprite arrow) : base(player)
    {
        this.arrow = arrow;

        windup = 0.7f;
        duration = 0.6f;
        cooldown = 0;

        preventsMovement = true;
        hitboxPersists = true;
    }

    public override void OnUse(Hitbox hitbox)
    {
        hitbox.spriteRenderer.flipX = player.spriteRenderer.flipX;
        
        // set hitbox sprite
        hitbox.spriteRenderer.sprite = arrow;
        hitbox.projectileType = "arrow";

        // set box collider size
        Vector2 size = hitbox.spriteRenderer.sprite.bounds.size;
        hitbox.boxCollider.size = size; 
        hitbox.boxCollider.offset = new Vector2(0.0f, 0.65f);

        // set scale and position
        hitbox.gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 1.0f);
        hitbox.gameObject.transform.position = player.transform.position + new Vector3(0.6f*direction,-0.59f,0.0f);

        // set hitbox attributes
        hitbox.damage = 10.0f;
        hitbox.piercing = true;
        hitbox.movement = new Vector3(0.3f*direction,0.0f,0.0f);
    }
}
