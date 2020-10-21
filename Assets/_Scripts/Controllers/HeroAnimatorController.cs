using UnityEngine;

public class HeroAnimatorController : MonoBehaviour
{
    // Animators
    private Animator animator;

    // Animator controllers
    [SerializeField] private RuntimeAnimatorController normalAnimatorController = null;
    [SerializeField] private RuntimeAnimatorController turningAnimatorController = null;

    // ---------- AWAKE ----------------------------------------------------+

    #region AWAKE
    private void Awake()
    {
        // Get animator component
        animator = gameObject.GetComponent<Animator>();
    }
    #endregion

    // ---------- DEFAULT ----------------------------------------------------+
    
    #region START DEFAULT ANIMATION
    public void StartDefaultAnimation()
    {
        // Change hero animation
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", false);
        animator.SetBool("isDodging", false);
        animator.SetBool("isJumping", false);
    }
    #endregion

    // ---------- BASIC MOVEMENT ----------------------------------------------------+

    #region START JUMPING ANIMATION
    public void StartJumpingAnimation()
    {
        // Change hero animation
        animator.SetBool("isDodging", false);
        animator.SetBool("isJumping", true);
    }
    #endregion

    #region STOP JUMPING ANIMATION
    public void StopJumpingAnimation()
    {
        // Change hero animation
        animator.SetBool("isJumping", false);
    }
    #endregion

    #region START DODGING ANIMATION
    public void StartDodgingAnimation()
    {
        // Change hero animation
        animator.SetBool("isJumping", false);
        animator.SetBool("isDodging", true);
    }
    #endregion

    #region STOP DODGING ANIMATION
    public void StopDodgingAnimation()
    {
        // Change hero animation
        animator.SetBool("isDodging", false);
    }
    #endregion

    // ---------- ATTACK & DEATH ----------------------------------------------------+

    #region START ATTACKING ANIMATION
    public void StartAttackingAnimation()
    {
        // Change animation
        animator.SetBool("isAttacking", true);
    }
    #endregion

    #region START DEAD ANIMATION
    public void StartDeadAnimation()
    {
        // Change animation
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDodging", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isDead", true);
    }
    #endregion

    #region STOP DEAD ANIMATION
    public void StopDeadAnimation()
    {
        // Change animation
        animator.SetBool("isDead", false);
    }
    #endregion

    // ---------- ANIMATOR CONTROLLER ----------------------------------------------------+

    #region SET ANIMATOR CONTROLLER
    public void SetAnimatorController(bool _isTurning)
    {
        if (_isTurning)
        {
            animator.runtimeAnimatorController = turningAnimatorController;
        }

        else
        {
            animator.runtimeAnimatorController = normalAnimatorController;
        }        
    }
    #endregion

    // ---------- CHECKS ----------------------------------------------------+

    #region CHECK IF DEAD
    public bool CheckIf_Dead()
    {
        // Return if hero is dying 
        return (animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"));
    }
    #endregion

    #region CHECK IF JUMPING
    public bool CheckIf_Jumping()
    {
        // Return if hero is jumping
        return (animator.GetCurrentAnimatorStateInfo(0).IsName("Jumping"));
    }
    #endregion
}
