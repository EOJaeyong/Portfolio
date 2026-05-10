using System.Collections;
using UnityEngine;

/*
 * State에 따른 애니메이션 변화
 * Idle : 기본
 * Move : 이동
 * Attack : 공격
 * Dead : 사망
 */
public enum AnimState
{
    Idle = 0,
    Move,
    Attack,
    Dead
}

public class Enemy: MonoBehaviour
{
    #region Stats
    private int health;
    private int attack;
    private bool isDead = false;
    #endregion
    #region Anim
    private Animator anim;
    #endregion
    #region State
    private ActionState actionState;
    private AnimState animState;
    #endregion

    private void Awake()
    {
        anim = GetComponent<Animator>();
        ChangeState(ActionState.Patrol);
    }
    private void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            ChangeState(ActionState.Patrol);
        }
        else if (Input.GetKeyDown("2"))
        {
            ChangeState(ActionState.Battle);
        }
        else if (Input.GetKeyDown("3"))
        {
            ChangeState(ActionState.Dead);
        }
    }

    private void ChangeState(ActionState state)
    {
        StopCoroutine(state.ToString());
        actionState = state;
        StartCoroutine(state.ToString());
    }

    private IEnumerator Patrol()
    {
        Debug.Log("ON PATROL");
        anim.SetTrigger("Idle");

        while (true)
        {
            Debug.Log("PATROL");
            yield return null;
        }
    }
    private IEnumerator Battle()
    {
        Debug.Log("ON BATTLE");
        anim.SetTrigger("Attack");

        while (true)
        {
            Debug.Log("BATTLE");
            yield return null;
        }
    }
    private IEnumerator Dead()
    {
        Debug.Log("ON DEAD");

        while (true)
        {
            Debug.Log("DEAD");
            yield return null;
        }
    }
}