
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public enum AminState
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Fall = 3,
    }

    [SerializeField] private float _speed = 5f;


    private Rigidbody2D _rb;
    private Animator _ani;

    private AminState _stase = AminState.Idle;

    private bool _horizontalInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ani = GetComponent<Animator>();


    }

    private void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");

    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_horizontalInput * _speed, _rb.linearVelocity.y);
    }




}
