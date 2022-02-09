using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpStairs : MonoBehaviour
{
    public PlayerController player;
    public bool onLadder;
    public bool UpStair;
    public bool DownStair;
    public float upForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (UpStair == true)
        {
            if (Input.GetAxis("Vertical") == -1 || Input.GetKey(KeyCode.S))
            {
                StepDown();
            }
            else if (Input.GetButtonDown("Jump"))
            {
                UpStep();
                player.PlayerDoubleJump = false;
            }
        }

        if (DownStair && player.player.isGrounded)
        {
            DownStair = false;
            onLadder = false;
            player.downStair = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Step")
        {
            UpStair = true;
            DownStair = false;
            onLadder = true;
            player.upStair = UpStair;
            player.downStair = DownStair;
            player.onLadder = true;
            player.playerJump = false;
            player.PlayerDoubleJump = false;
            Debug.Log("Agarrado a escalon");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Step")
        {
            UpStair = false;
            player.upStair = UpStair;
            Debug.Log("Agarrado a escalon");
        }
    }

    public void UpStep()
    {
        UpStair = false;
        DownStair = false;
        player.upStair = UpStair;
        player.downStair = DownStair;
        player.fallVelocity = upForce;
        player.movePlayer.y = player.fallVelocity;
    }

    public void GrabStep()
    {
        if (UpStair == true)
        {

        }
    }

    public void StepDown()
    {
        UpStair = false;
        DownStair = true;
        player.upStair = UpStair;
        player.downStair = DownStair;
    }
}
