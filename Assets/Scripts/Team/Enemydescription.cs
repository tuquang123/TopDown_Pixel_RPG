using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyInfoData
{
    public string enemyId;
    public string enemyName;
    public string description;
}

[CreateAssetMenu(
    menuName = "GameData/Enemy Info Database",
    fileName = "EnemyInfoDatabase"
)]
public class EnemyInfoDataBase: ScriptableObject
{
    public List<EnemyInfoData> info = new ();
    
    //to do Void Get Des by ID
    public void GetDescriptionById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        for (int i = 0; i < info.Count; i++)
        {
            if (info[i].enemyId == id)
            {
                Debug.Log(info[i].description);
                return;
            }
        }

        Debug.LogWarning($"Không tìm thấy Enemy với ID: {id}");
    }

}