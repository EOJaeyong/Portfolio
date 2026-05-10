using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SendOffManager : MonoBehaviour
{
    #region Singleton
    private static SendOffManager instance;

    public static SendOffManager Instance
    {
        get
        {
            if (instance == null)
                instance = new SendOffManager();

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public AudioSource audioSource;
    public AudioClip artifactInsertSound;

    private List<GameObject> so_Objs;
    public GameObject effectPrefab;
    public Material effectMtl;

    public GameObject resultUI;

    private void OnEnable()
    {
        so_Objs = new List<GameObject>();
    }

    private void OnDisable()
    {
        so_Objs.Clear();
    }

    public void AddSendObjects(GameObject item)
    {
        so_Objs.Add(item);

        item.transform.localPosition += new Vector3(0f, Random.Range(.5f, 1.5f), 0f);
        item.transform.localScale = new Vector3(1f, 1f, 1f);

        item.AddComponent<ObjectActivation>();
        item.GetComponent<ObjectActivation>().ToggleAngle();

        item.GetComponentInChildren<ObjectEffect>().enabled = false;

        if (audioSource != null && artifactInsertSound != null)
            audioSource.PlayOneShot(artifactInsertSound);
    }

    public void SendAllObjects()
    {
        var total_value = 0;
        foreach (var temp in so_Objs)
        {
            total_value += Mathf.RoundToInt(temp.GetComponent<ItemPickUp>().Item.itemValue);

            // 
            temp.GetComponentInChildren<MeshRenderer>().material = effectMtl;
            temp.GetComponentInChildren<ObjectEffect>().enabled = true;
            // 
            Destroy(temp, 2.1f);
        }

        so_Objs.Clear();

        var effect = Instantiate(effectPrefab, this.gameObject.transform);
        Destroy(effect, 2.5f);
        Debug.Log("제출 전 : " + GameManager.Instance.currentQuota);
        GameManager.Instance.currentQuota += total_value;
        Debug.Log("제출 후 : " + GameManager.Instance.currentQuota);

        resultUI.SetActive(true);
    }

}
