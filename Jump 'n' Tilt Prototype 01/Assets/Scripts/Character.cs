﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : PhysicsObject
{

    protected float moveDirection;              // Gets value between -1 and 1 for direction left or right if Input.GetAxisRaw is used

    // Values can be adjusted in inspector
    public float moveSpeed;                // Movement Speed
    public float jumpHeight;               //Gravity Modifier in inspector is set to 4 to get a non floaty feeling
    public float jumpHeightReducer;      // Reduces jump height, if button is pressed shortly
    public float moveWhileJumping;         // Movement value while jumping
    public float airResistance;              //by Marvin Winkler, 1 no resistance, 0 no movement possible (x-axis dampening wile in the air)
    protected float wallJumpTime;             //by Marvin Winkler, used so that midAir movement does not overreide the wall jump
    public float wallJumpTimeSpeed;           //by Marvin Winkler, used to adjust the time less midair movement is possible after a wall jump

    public bool onWall;                 //by Marvin Winkler, used to fix wall climbing while level is tilted

    public float slideSpeed;
    public float slideReducer;

    public float slideBackwardsMaxSpeed;    //by Marvin Winkler, max speed while pressing against the tilt

    protected Vector2 slideDirection;

    public bool isFacingRight;
    protected bool isSliding;

    // for Attack method
    public Transform attackPos;                 // is set in Unity window
    public float attackRadius;
    public LayerMask whatIsEnemy;

    // taking Damage, getting knocked back
    public int health;
    public float knockback;             //by Marvin Winkler, backwards velocity given to character when hit
    public float knockup;               //by Marvin Winkler, upwards velocity given to character when hit
    protected bool isDead;              //by Marvin Winkler, true when character is dead

    // inherited from PhysicsObject.cs
    protected override void OnEnable()
    {
        base.OnEnable();
        wallJumpTime = 0;
        isDead = false;
    }

    // Author: Michelle Limbach, Nicole Mynarek, Marvin Winkler
    //gets called onece per update
    protected override void ComputeVelocity()
    {
        // Player only slides when there is no input
        if (moveDirection != 0)
        {
            // character looks in the direction he is moving
            CharacterFacingDirection(moveDirection);
        }
        else
        {
            Slide();
        }
        moveDirection = 0;

        // death of character
        //if (health <= 0)
        //{
        //    Destroy(gameObject);
        //}

        if(wallJumpTime < 0)
        {
            wallJumpTime = 0;
        }
        else if(wallJumpTime > 0)
        {
            wallJumpTime -= (1 - wallJumpTime * 0.99f) * timeController.getSpeedAdjustedDeltaTime() * wallJumpTimeSpeed;
        }

        velocity.x -= velocity.x * airResistance;
    }

    // Author: Nicole Mynarek, Michelle Limbach, Marvin Winkler
    // Method for basic horizontal movement 
    protected virtual void Movement(float direction)
    {
        moveDirection = direction;

        if (onWall)
        {
            return; // this disables manual movement if the player is on a wall while the level is tilted, thus disabling wall climbing
        }

        if (grounded)
        {
            if (isSliding)
            {
                // if slideDirection and moveDirection are both negativ or positiv, then the player moves faster
                if ((slideDirection.x < 0 && moveDirection < 0 || slideDirection.x > 0 && moveDirection > 0) && velocity.magnitude < maxSpeed)
                {
                    velocity += moveDirection * slideSpeed * Vector2.right; //Because the velocity is changed and not replaced Speed changes don't happen instantly but have an excelleration time
                }
                // if slideDirection and moveDirection have unequal signs (e. g. one is positive and the other one is negative), then the player moves slower
                else if ((slideDirection.x < 0 && moveDirection > 0 || slideDirection.x > 0 && moveDirection < 0) && velocity.magnitude < slideBackwardsMaxSpeed)
                {
                    velocity += moveDirection * slideSpeed * Vector2.right;
                }
            }
            else
            {
                velocity = new Vector2(moveDirection * moveSpeed, velocity.y);  //Here velocity gets a new vector, therefore the speed/direction change happens instantly, there is no excelleration time
            }
        }
        // if player is in the air and gives input, the player can move left or right
        else if (!grounded && moveDirection != 0)
        {
            velocity += (moveDirection * moveWhileJumping) * Vector2.right * (1 - wallJumpTime) * (1 / ((0.1f + Mathf.Abs(velocity.x) * 0.5f))); //velocity = new Vector2((velocity.x + (moveWhileJumping * moveDirection)) * Mathf.Pow(airResistance, velocity.magnitude) * (1 - wallJumpTime), velocity.y);
        }
    }

    // Author: Nicole Mynarek, Marvin Winkler
    // Method for basic jump
    protected virtual void Jump()
    {
        if (jumpable)
        {
            // Gravity Modifier of PhysicsObject.class needs to be adjusted according to jumpHeight for good game feeling
            //velocity += jumpHeight * Vector2.up;
            velocity = new Vector2(velocity.x, jumpHeight);
        }
    }

    // Author: Nicole Mynarek, Rewritten for debugging by Marvin Winkler
    // Method for flipping character sprites according to moving direction
    protected void CharacterFacingDirection(float direction)
    {
        if(direction < 0)
        {
            isFacingRight = false;
            gameObject.GetComponent<SpriteRenderer>().flipX = true;
        }
        else if(direction > 0)
        {
            isFacingRight = true;
            gameObject.GetComponent<SpriteRenderer>().flipX = false;
        }

    }

    // Author: Nicole Mynarek, Michelle Limbach, Marvin Winkler
    // Method for sliding. Player always looks in direction of the tilt. 
    // Variable 'minGroundNormalY' from PhysicsObject.class can be adjusted for different results
    // Sliding speed still has to be adjusted
    protected virtual void Slide()
    {
        Vector2 normal;

        if (grounded)
        {
            //Debug.Log(groundNormal);
            if (groundNormal.y < 1) //!= new Vector2(0f, 1f))
            {
                isSliding = true;

                for (int i = 0; i < hitBufferList.Count; i++)
                {
                    normal = hitBufferList[i].normal;
                   
                    // left tilt direction
                    if (normal.x < 0)
                    {
                        //Debug.Log("Normal x < 0: " + normal.x);
                        slideDirection = Vector2.Perpendicular(normal);

                        slideDirection.x = -1;
                        CharacterFacingDirection(slideDirection.x);
                    }
                    // right tilt direction
                    else
                    {
                        //Debug.Log("Normal x > 0: " + normal.x);
                        slideDirection = Vector2.Perpendicular(-normal);

                        slideDirection.x = 1;
                        CharacterFacingDirection(slideDirection.x);
                    }
                }
                if(velocity.x <= maxSpeed && velocity.x >= -maxSpeed)
                {
                    velocity += slideDirection;
                } 
            }
            else
            {
                isSliding = false;
                slideDirection = new Vector2(0f, 0f);
            }
        }
    }

    //Author: Nicole Mynarek, Marvin Winkler
    protected virtual void Attack()
    {
        //  Debug.Log("Nicole ---------- ATTACK!!!!!!!");
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPos.position, attackRadius, whatIsEnemy);

        for (int i = 0; i < enemies.Length; i++)
        {
            Vector3 dmgDirection = gameObject.transform.localPosition - enemies[i].GetComponent<Transform>().localPosition;
            Vector2 dmgDirection2D = new Vector2(dmgDirection.x, dmgDirection.y);
            dmgDirection2D.Normalize();
            enemies[i].GetComponent<Enemy>().TakeDamage(1, dmgDirection2D);
        }
    }

    // Author: Nicole Mynarek, Marvin Winkler
    // changed protected to public to access it in Onryo script
    public virtual void TakeDamage(int damage, Vector2 direction)
    {
        Debug.Log("player damage");
        health -= damage;
        velocity = new Vector2(-direction.x * knockback, knockup);
        CharacterFacingDirection(-velocity.x);
        if(health <= 0)
        {
            isDead = true;
        }
    }
}
