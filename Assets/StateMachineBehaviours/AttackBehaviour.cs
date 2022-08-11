using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    private Character character;
    private bool rangedAttack;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character = animator.GetComponent<Character>();
        rangedAttack = character.IsRangedWeaponActive();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character.FinishAttack(rangedAttack);
    }
}
