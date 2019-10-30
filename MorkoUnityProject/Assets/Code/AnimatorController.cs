using UnityEngine;

namespace Morko
{
    public class AnimatorController : MonoBehaviour
    {
        public Animator animator;
        private LocalPlayerController.MovementState previousState;
    
        private void Start()
        {
            animator = GetComponent<Animator>();
            animator.Play("Idle");
        }
    
        public void SetAnimation(LocalPlayerController.MovementState movementState, float multiplier = 1f)
        {
            Debug.Log("PLAYING ANIM: " + movementState + " - " + multiplier);

            if (previousState == movementState)
            {
                animator.speed = multiplier;
                return;
            }
            
            animator.Play(movementState.ToString());
            animator.speed = multiplier;
    
            previousState = movementState;
        }
    }
}