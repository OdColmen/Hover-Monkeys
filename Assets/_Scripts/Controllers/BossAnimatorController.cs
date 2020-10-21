using UnityEngine;

public class BossAnimatorController : MonoBehaviour
{
    // Animators
    Animator animator;

    // ---------- AWAKE ----------------------------------------------------+

    private void Awake()
    {
        // Get animator component
        animator = gameObject.GetComponent<Animator>();
    }

    // ---------- DEFAULT ----------------------------------------------------+

    #region START DEFAULT ANIMATION
    public void StartDefaultAnimation()
    {
        // Change animation
        animator.SetBool("isDead", false);
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

    #region PREPARE JAMMING GUN ANIMATION
    public void PrepareJammingGunAnimation()
    {
        // Change animation
        animator.SetBool("isJammingGun", true);
    }
    #endregion

    #region START DEAD ANIMATION
    public void StartDeadAnimation()
    {
        // Change animation
        animator.SetBool("isDead", true);
    }
    #endregion

    // ---------- CHECKS ----------------------------------------------------+

    #region CHECK IF DEAD
    public bool CheckIf_Dead()
    {
        // Return if dead
        return (animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"));
    }
    #endregion

    #region CHECK IF ATTACKING
    public bool CheckIf_Attacking()
    {
        // Return if attacking
        return (animator.GetCurrentAnimatorStateInfo(0).IsName("Attacking"));
    }
    #endregion

    #region CHECK IF DEFAULT
    public bool CheckIf_Default()
    {
        // Return if default
        return (animator.GetCurrentAnimatorStateInfo(0).IsName("Default"));
    }
    #endregion
}
