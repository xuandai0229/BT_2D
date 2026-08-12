using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 5f;

    public Transform _groundCheck;
    [SerializeField] float _groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator ani;
    private SpriteRenderer spriteRenderer;
     
    private float moveInput;
    public bool isGrounded;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        

        isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, groundLayer);

        //animator
        ani.SetBool("isRunning", moveInput != 0);
        ani.SetBool("isGrounded", isGrounded);
        ani.SetFloat("yVelocity", rb.linearVelocity.y);

        //jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            ani.SetTrigger("Jump");
        }

        //flip
        if(moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * _speed, rb.linearVelocity.y);
    }
    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }
}
