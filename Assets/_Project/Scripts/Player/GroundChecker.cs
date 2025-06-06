using NUnit.Framework;
using UnityEngine;

namespace RpgPractice
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] private float groundDistance = 0.08f;
        [SerializeField] private LayerMask groundLayers;
        
        public bool IsGrounded { get; private set; }


        void Update()
        {
            IsGrounded = Physics.CheckSphere(transform.position, groundDistance, groundLayers);
            //IsGrounded = Physics.SphereCast(transform.position+Vector3.up*0.5f, groundDistance, Vector3.down, out _, groundDistance, groundLayers);
            
        }
        
    }
}