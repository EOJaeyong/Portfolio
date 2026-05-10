using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    // 오브젝트 제거 Y축 기준
    public float destroyY = -7f;

    void Update()
    {
        // 화면 아래로 벗어나면 오브젝트 제거
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            GameManager.Instance.OnFail();
            Destroy(gameObject);
        }
    }
}
