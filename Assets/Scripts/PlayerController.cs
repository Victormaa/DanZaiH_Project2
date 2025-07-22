using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 
    /// 1. AI 算法；  开发 流程 架构 设计模式
    /// 善用 AI Github；
    /// 
    /// 
    /// 
    /// </summary>
    public float runSpeed;
    private Rigidbody2D myRigidbody;
    private float moveX, moveY;
    private Vector2 moveDirection;
    private Animator myAnim;
    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnim = transform.Find("player").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        inputSection();
        Flip();
    }

    private void FixedUpdate()
    {
        playerMove();
    }

    void Flip()
    {
        bool plyerHasXAxisSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        if (plyerHasXAxisSpeed)
        {
            if (myRigidbody.velocity.x > 0.1f)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            }

            if (myRigidbody.velocity.x < -0.1f)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private void inputSection()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;
        if (moveDirection != Vector2.zero)
        {
            myAnim.SetBool("Walk", true);
        }
        else
        {
            myAnim.SetBool("Walk", false);
        }
    }

    private void playerMove()
    {
        myRigidbody.velocity = new Vector2(moveDirection.x * runSpeed, moveDirection.y * runSpeed);
    }
}
