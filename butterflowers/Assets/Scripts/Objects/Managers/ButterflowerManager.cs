using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using Settings;
using UnityEngine.Events;
using uwu.Gameplay;

public class ButterflowerManager : Spawner, IReactToSunCycle
{
    public static ButterflowerManager Instance = null;

    public UnityEvent onAllDead;

    [SerializeField] 
    WorldPreset preset = null;

    #region External

    [SerializeField] new Camera camera;

    #endregion

    #region Attributes

    bool respawn = true;

    [SerializeField] float offsetDistance = 0f;
    [SerializeField] int alive = 0, dead = 0;

    [SerializeField] int player_kills = 0, wizard_kills = 0, shared_kills = 0, misc_kills = 0;

    public float player_hatred => (dead > 0) ? (float)(player_kills + shared_kills/2F) / dead: 0F;
    public float wiz_hatred => (dead > 0)? (float)(wizard_kills + shared_kills/2F) / dead: 0F;
    public float shared_hatred => (dead > 0)? (float)shared_kills / dead: 0F;
    public float enviro_hated => (dead > 0) ? (float)misc_kills / dead : 0F;

    public float minKillDistance => offsetDistance/2f;

    #endregion

    #region Collections

    List<Butterfly> butterflies = new List<Butterfly>();

	#endregion

	#region Monobehaviour callbacks

	protected override void Awake()
    {
        Instance = this;

        if(preset != null)
            amount = preset.amountOfButterflies;

        base.Awake();
    }

    // Start is called before the first frame updaste
    protected override void Start()
    {
        Butterfly.OnRegister += AddButterfly;
        Butterfly.OnUnregister += RemoveButterfly;

        Butterfly.Dying += ButterflyDying;
        Butterfly.Died += ResetButterfly;

        base.Start(); 
    }

    protected override void Update(){
        base.Update();

        if(preset != null)
            amount = preset.amountOfButterflies;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        Butterfly.OnRegister -= AddButterfly;
        Butterfly.OnUnregister -= RemoveButterfly;

        Butterfly.Dying -= ButterflyDying;
        Butterfly.Died -= ResetButterfly;
    }

    #endregion
    
    #region Cycle

    public void Cycle(bool refresh)
    {
        KillButterflies();
    }
    
    #endregion

    #region Spawner overrides

    public override void DecidePosition(ref Vector3 pos)
    {
        /*if (camera != null) {
            Vector3 offset = Vector3.zero, position = Vector3.zero;

            Collider col = root.GetComponent<Collider>();
            var distance = Random.Range(0f,40f);

            offset = new Vector3(Random.Range(0f, 1f),
                                 Random.Range(0f, 1f),
                                 distance);

            print(distance);

            pos = camera.ViewportToWorldPoint(offset);
        }*/
        
        base.DecidePosition(ref pos);
    }

    void ResetButterfly(Butterfly butterfly)
    {
        if (alive == 0)
            onAllDead.Invoke();

        if (!respawn) return;

        InstantiatePrefab(butterfly.gameObject);
        butterfly.Reset();
    }

    protected override void SetPrefabAttributes(GameObject instance, Vector3 position, Quaternion rotation)
    {
        Butterfly butterfly = instance.GetComponent<Butterfly>();
        butterfly.position = position;

        butterfly.transform.localScale = Vector3.zero;
        butterfly.transform.rotation = rotation;
    }

    protected override void onInstantiatePrefab(GameObject obj, bool refresh)
    {
        base.onInstantiatePrefab(obj, refresh);

        alive++;
        if (refresh) {
            dead--;

            var b = obj.GetComponent<Butterfly>();
            SubDying(b);
        }
    }

    #endregion

    #region Butterfly operations

    public void KillButterflies(bool infinite = true)
    {
        respawn = infinite;
        foreach (Butterfly butterfly in butterflies)
            butterfly.Kill();
    }

    public void KillButterfliesInDirection(Vector3 direction, float strength = 1f)
    {
        Vector3 dir = Vector3.zero;
        
        foreach (Butterfly butterfly in butterflies) 
        {
            dir = (butterfly.transform.position - camera.transform.position).normalized;
            dir += direction;
            
            butterfly.Velocity = dir * strength;
            butterfly.Kill();
        }
    }

    public void ReleaseButterflies()
    {
        foreach (Butterfly butterfly in butterflies)
            butterfly.Release();
    }

    #endregion

    #region Butterfly callbacks

    void AddButterfly(Butterfly butterfly)
    {
        butterflies.Add(butterfly);
    }

    void RemoveButterfly(Butterfly butterfly)
    {
        butterflies.Remove(butterfly);

        if (butterfly.state == Butterfly.State.Dying) {
            dead--;
            SubDying(butterfly);
        }
        else
            alive--;
    }

    void ButterflyDying(Butterfly butterfly)
    {
        ++dead;
        AddDying(butterfly);

        --alive;
    }

    #endregion

    #region Health operations

    void AddDying(Butterfly butterfly)
    {
        if (butterfly.agent == AGENT.User)
            ++player_kills;
        else if (butterfly.agent == AGENT.Wizard)
            ++wizard_kills;
        else if (butterfly.agent == AGENT.Inhabitants)
            ++shared_kills;
        else
            ++misc_kills;
    }

    void SubDying(Butterfly butterfly)
    {
        if (butterfly.agent == AGENT.User)
            --player_kills;
        else if (butterfly.agent == AGENT.Wizard)
            --wizard_kills;
        else if (butterfly.agent == AGENT.Inhabitants)
            --shared_kills;
        else
            --misc_kills;
    }

    public float GetHealth()
    {
        int total = (alive + dead);
        if (total == 0) return 1f;

        return ((float)alive / total);
    }

	#endregion
}
