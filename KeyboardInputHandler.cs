using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputHandler : InputHandler
{
    float jumpTime = 2.5f;
    bool isGrounded;

    public KeyboardInputHandler(Player player) : base(player) {}
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // add acceleration to movement

        if(Input.GetKey(KeyCode.A))
        {
            player.transform.position += Vector3.left * player.speed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.D))
        {
            player.transform.position += Vector3.right * player.speed * Time.deltaTime;
        }

        // prevent spam jump acting like flying
        if(Input.GetKey(KeyCode.W)) 
        {
            isGrounded = false;
            player.transform.position += Vector3.up * player.jumpHeight * Time.deltaTime;
        }

        if(!isGrounded) {
            jumpTime -= Time.deltaTime;
        }
    }

    void FixedUpdate() 
    {
        if(!isGrounded && jumpTime > 0) {
            player.rb.AddForce(transform.up * player.jumpHeight);
        }
    }
}
