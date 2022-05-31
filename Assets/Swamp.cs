using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swamp : MonoBehaviour
{
    private List<GameObject> Slimes;

    private void Start()
    {
        Slimes = new List<GameObject>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Neutrality" && col.name == "Slime")
        {
            Slimes.Add(col.gameObject);
            Debug.Log("�������߰���");
            Debug.Log("���� " + Slimes.Count + "����");
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if(col.tag == "Neutrality" && col.name == "Slime")
        {
            Slimes.Remove(col.gameObject);
            Debug.Log("�����ӳ���");
            Debug.Log("���� " + Slimes.Count + "����");
        }
    }
}
