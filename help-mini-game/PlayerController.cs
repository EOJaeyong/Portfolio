using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 이동 속도
    public float moveSpeed = 5f;

    // 이동 제한 범위
    public float limitX = 4.5f;

    private bool canMove = true;

    void Update()
    {
        if (!canMove) return;

        float input = Input.GetAxisRaw("Horizontal");

        Vector3 pos = transform.position;
        pos.x += input * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        transform.position = pos;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
