using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public BattleTeam battleTeam;
    public HeroData[] allHeroDatas;

    public Hero[] allHeroes;

    void Start()
    {
        battleTeam = new BattleTeam();

        // Dùng ScriptableObject để tạo hero runtime
        allHeroes = new Hero[allHeroDatas.Length];
        for (int i = 0; i < allHeroDatas.Length; i++)
        {
            allHeroes[i] = new Hero(allHeroDatas[i],0);
        }
    }

}