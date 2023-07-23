namespace FWGPUE.Gameplay;

class Weapon
{
    /// <summary>
    /// Whether the hitbox of the weapon is currently active.
    /// </summary>
    public bool Attacking;
    /// <summary>
    /// Whether the weapon can restart an attack during an attack.
    /// </summary>
    public bool CanCancelIntoNewAttack;

    /// <summary>
    /// The current hitbox of the weapon is stored as a set of polygons.
    /// </summary>
    public Hitbox Hitbox => Hitboxes[AttackStageCounter];
    /// <summary>
    /// Different movements of the weapon are represented as different hitboxes.
    /// </summary>
    public Hitbox[] Hitboxes;
    /// <summary>
    /// The current index into the hitbox array.
    /// </summary>
    public int AttackStageCounter = 0;
    /// <summary> 
    /// Speed at which the attack stage advances. 
    /// </summary>
    public float AttackSpeed;
    /// <summary> 
    /// Attack stage accumulator. 
    /// </summary>
    public float TimeInCurrentStage;

    public virtual void StopAttack()
    {
        Attacking = false;
        AttackStageCounter = 0;
        TimeInCurrentStage = 0;
    }
    public void ForceAttack()
    {
        StopAttack();
        Attacking = true;
    }

    public virtual void Attack()
    {
        // if the weapon can start attacking regardless of attack state, just do that
        if (CanCancelIntoNewAttack)
        {
            ForceAttack();
            return;
        }

        // otherwise, don't attack if the weapon is already attacking
        if (Attacking)
        {
            return;
        }

        ForceAttack();
    }

    public virtual void TickHitbox()
    {
        if (Attacking)
        {
            TimeInCurrentStage += TickTime;

            if (TimeInCurrentStage >= AttackSpeed)
            {
                AttackStageCounter++;
                TimeInCurrentStage = 0;

                if (AttackStageCounter >= Hitboxes.Length)
                {
                    // if there are no more hitboxes, the weapon is done attacking
                    StopAttack();
                }
            }

            // TODO: find overlaps and resolve damage etc.
        }
    }
}