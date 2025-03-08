using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.Player.Movement
{
    public class Player_Movement : MonoBehaviour
    {
        [SerializeField] private Vector2 inputDirection;

        [Header("Movement")]
        public Rigidbody2D rb;
        public float moveSpeed = 10;
        public float acceleration = 7;
        public float decceleration = 7;
        public float velPower = 0.9f;

        public float frictionAmount = 0.2f;

        [Header("Jump")]
        public float jumpForce = 15;


        public void InputHandler_Movement(InputAction.CallbackContext context)
        {
            Debug.Log("Receiving input");
            inputDirection = context.ReadValue<Vector2>();
            //Debug.Log("Direction: " + inputDirection);

            if (context.started)
            {
                Debug.Log("Started");
            }
            else if (context.performed)
            {
                Debug.Log("Performed");
            }
            else if (context.canceled)
            {

                Debug.Log("Canceled");
            }
        }

        private void FixedUpdate()
        {
            Debug.Log("Moving");
            float targetSpeed = inputDirection.x * moveSpeed;
            float speedDif = targetSpeed - rb.linearVelocity.x;

            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

            rb.AddForce(movement * Vector2.right);
        }
    }
}
