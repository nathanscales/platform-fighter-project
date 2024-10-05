using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability
{
    public float cooldown, remainingCooldown;
    public float windup; // time between user pressing ability button and ability happening
    public float airborneWindup; // same as windup but for when player is airborne
    public float duration; // time between ability happening and user input being re-enabled
    public float modSpeed; // amount that the ability changes the character's speed
    public bool hitboxPersists; // causes the hitbox to remain active after the ability's duration is over.
    public bool preventsSpriteFlip, preventsMovement, usableWhileAirborne, setImmune;

    // determines player direction using its sprite direction
    public int direction { get { return ((player.spriteRenderer.flipX) ? -1 : 1);}}

    protected Player player;

    public Ability(Player player)
    {
        this.player = player;
    }

    public abstract void OnUse(Hitbox hitbox);
}
