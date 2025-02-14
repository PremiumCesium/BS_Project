using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Weapons", order = 1)]
public class WeaponClass : ScriptableObject
{
    public string gunName;
    public int damage;
    public int maxRounds;
    public int currRounds;
    public int frps;
    public int reloadTime;
    public float range;

    public GameObject gunModel;

    //for later add model reference as object too
}
