using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererAbility1 : Ability
{
    private RuntimeAnimatorController animatorController;

    public SorcererAbility1(Player player, RuntimeAnimatorController animatorController) : base(player)
    {
        this.animatorController = animatorController;

        windup = 0.3f;
        duration = 0.5f;
        cooldown = 1;

        hitboxPersists = true;
    }

    public override void OnUse(Hitbox hitbox)
    {
        hitbox.damage = 10.0f;
        hitbox.damageIncreasePerSecond = 5.0f;

        hitbox.animator.runtimeAnimatorController = animatorController;
        hitbox.projectileType = "frostbolt";
        hitbox.spriteRenderer.flipX = player.spriteRenderer.flipX;

        hitbox.boxCollider.size = new Vector2(0.24f, 0.14f);
        hitbox.boxCollider.offset = new Vector2(0.02f, 0.0f);

        hitbox.gameObject.transform.localScale = new Vector3(1.75f, 1.75f, 1.0f);
        hitbox.gameObject.transform.position = player.transform.position + new Vector3(0.5f*direction,0.725f,0.0f);

        hitbox.movement = new Vector3(0.3f*direction,0.0f,0.0f);
    }
}