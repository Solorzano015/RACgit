using UnityEngine;

public class puertajefe : MonoBehaviour
{
    public GameObject puertaG;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider player)
    {
        if (player.tag == "Player")
        {
            puertaG.SetActive(true);
            bossBrain.instance.follow = true;
        }

    }
}
