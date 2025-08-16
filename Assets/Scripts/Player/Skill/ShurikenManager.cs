using UnityEngine;
using System.Collections.Generic;

public class ShurikenManager : MonoBehaviour
{
    public GameObject shurikenPrefab;
    public Transform player;
    public int numberOfShurikens = 3; // Mặc định là 3 phi tiêu
    public float radius = 2f;

    private List<GameObject> shurikens = new List<GameObject>();

    void Start()
    {
        SpawnShurikens();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Nhấn Space để thay đổi số phi tiêu
        {
            numberOfShurikens = (numberOfShurikens % 5) + 1; // Tăng số lượng từ 1 -> 5 rồi quay về 1
            UpdateShurikens();
        }
    }

    void SpawnShurikens()
    {
        ClearShurikens();

        for (int i = 0; i < numberOfShurikens; i++)
        {
            GameObject shuriken = Instantiate(shurikenPrefab, player.position, Quaternion.identity);
            Shuriken orbit = shuriken.GetComponent<Shuriken>();
            orbit.target = player;
            orbit.radius = radius;
            orbit.SetIndex(i, numberOfShurikens);

            shurikens.Add(shuriken);
        }
    }

    void UpdateShurikens()
    {
        SpawnShurikens();
    }

    void ClearShurikens()
    {
        foreach (GameObject shuriken in shurikens)
        {
            Destroy(shuriken);
        }
        shurikens.Clear();
    }
}