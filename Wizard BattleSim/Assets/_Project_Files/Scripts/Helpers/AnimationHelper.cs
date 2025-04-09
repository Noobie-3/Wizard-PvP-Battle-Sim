using UnityEngine;
public static class AnimationHelper
{
    public static void SetMovementBlend(Animator animator, float inputMagnitude, float smoothing)
    {
        animator.SetFloat("MoveSpeed", inputMagnitude, smoothing, Time.deltaTime);
    }

    public static void SetBool(Animator animator, string param, bool value)
    {
        animator.SetBool(param, value);
    }

    public static void Trigger(Animator animator, string param)
    {
        animator.SetTrigger(param);
    }
}
