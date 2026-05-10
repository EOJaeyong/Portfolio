using UnityEngine;

public class ObjectEmission : MonoBehaviour
{
    private bool isAlive = false;
    private Material mtl;

    private float emissionValue;
    private Color setColor;

    private void OnEnable()
    {
        isAlive = true;
        mtl = this.GetComponent<MeshRenderer>().material;
    }

    private void OnDisable()
    {
        isAlive = false;
    }

    private void Update()
    {
        if (isAlive)
        {
            emissionValue = Mathf.PingPong(Time.time, 1f);
            setColor = Color.white * emissionValue;
            mtl.SetColor("_EmissionColor", setColor);
        }
    }
}
