using UnityEngine;

/// <summary>
/// Entity animation related stuff...
/// </summary>
public class EntityAnimator
{
    public Animator Animator { get; private set; }
    private readonly string entityName;

    public EntityAnimator(Entity entity)
    {
        Animator = entity.GetComponent<Animator>();
        entityName = entity.name;

        if(!Animator)
        {
            Debug.LogWarning($"[{entityName}] does not have an animator component!");
        }
    }

    public void Play(string animStateName, float normalizedTime = 0, int layer = 0)
    {
        // Grab current animator state info and state hash...
        AnimatorStateInfo stateInfo = GetCurrentStateInfo(layer);
        int stateHash = Animator.StringToHash(animStateName);

        // Already playing? Fuck off!
        if(stateInfo.fullPathHash == stateHash) { return; }

        // No state found?
        if(!Animator.HasState(layer, stateHash))
        {
            Debug.Log($"Animation state: [{animStateName}] does not exist for: [{entityName}]!");
            return;
        }

        // Play the animation with specified layer and normalized time...
        Animator.Play(stateHash, layer, normalizedTime);
    }

    public bool IsFinished(int layer = 0)
    {
        AnimatorStateInfo animStateInfo = GetCurrentStateInfo(layer);
        return animStateInfo.normalizedTime >= 1f && !Animator.IsInTransition(layer) && !animStateInfo.loop;
    }

    public float GetCurrentDuration(int layer = 0)
    {
        AnimatorStateInfo animStateInfo = GetCurrentStateInfo(layer);
        return animStateInfo.length;
    }

    public float GetRemainingTime(int layer = 0)
    {
        AnimatorStateInfo animStateInfo = GetCurrentStateInfo(layer);
        float normalizedTime = animStateInfo.normalizedTime % 1f;
        float totalDuration = animStateInfo.length;
        float currentTime = normalizedTime * totalDuration;
        float remainingTime = totalDuration - currentTime;
        return remainingTime;
    }

    public bool IsPlaying(string animStateName, int layer = 0)
    {
        AnimatorStateInfo animStateInfo = GetCurrentStateInfo(layer);
        return animStateInfo.IsName(animStateName);
    }

    public AnimatorStateInfo GetCurrentStateInfo(int layer = 0)
    {
        return Animator.GetCurrentAnimatorStateInfo(layer);
    }

    public void SetFloat(string parameterName, float value) => Animator.SetFloat(parameterName, value);
    public void SetBool(string parameterName, bool value) => Animator.SetBool(parameterName, value);
    public void SetPlaybackSpeed(float speed) => Animator.speed = speed;
}
