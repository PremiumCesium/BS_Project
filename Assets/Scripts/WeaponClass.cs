using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Weapons", order = 1)]
public class WeaponClass : ScriptableObject
{
    public string gunName;
    public int damage;
    public int maxRounds;
    public int currRounds;
    public int reloadTime;
    public float range;
    

    public GameObject gunModel;

    //Gun logic
    public ParticleSystem impactParticleSystem;
    public TrailRenderer bulletTrail;
    public float fps;
    //bulletspread logic
    public Vector3 bulletVariance;
    public bool bulletSpread;

    //for later add model reference as object too
}
