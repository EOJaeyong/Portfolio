using UnityEngine;
using DG.Tweening;
public class ObjectActivation: MonoBehaviour
{
    private bool isAlive = false;
    private float rotateValue = 0f;
    private float time = 0f;
    private bool isRightAngle = true;
    private bool cantMove = false;

    private Vector3 origin;
    private Vector3 move;

    private void OnEnable()
    {
        isAlive = true;
        isRightAngle = true;
        origin = this.transform.position;
        move = origin;
    }

    private void OnDisable()
    {
        isAlive = false;
        origin = Vector3.zero;
        move = Vector3.zero;
    }

    public void ToggleAngle()
    {
        isRightAngle = !isRightAngle;
    }

    public void ToggleCantMove(bool isMove, Transform pos)
    {
        cantMove = isMove;

        origin = pos.position;
        origin.y = .3f;
        move = origin;
    }

    private void Start()
    {
        this.transform.DOMove(move, 2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            time += Time.deltaTime;

            if (time > .75f)
            {
                move.y += (Time.deltaTime * .3f);
            }
            else
            {
                move.y -= (Time.deltaTime * .3f);
            }

            if (time >= 1.5f)
                time = 0f;

            rotateValue += (Time.deltaTime * 10f);
            if (isRightAngle)
            {
                this.transform.eulerAngles = new Vector3(-90f, 0, rotateValue);
            }
            else
            {
                this.transform.eulerAngles = new Vector3(0f, rotateValue, 0f);
            }

            if (cantMove)
            {
                transform.DOPause();
            }
        }
    }
}
