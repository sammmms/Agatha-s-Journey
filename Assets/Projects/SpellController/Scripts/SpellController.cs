using UnityEngine;

public enum Spell{
    None,
    Lightweight,
    Barrier,
    Enhance,
    Bolt,
    Flare,
    Arrow,
}

public class SpellController : MonoBehaviour
{

    public List<Spell> selectedSpell = new List<Spell>();
    
    [SerializeField] private GameObject barrier;
    [SerializeField] private GameObject bolt;
    [SerializeField] private GameObject flare;
    [SerializeField] private GameObject arrow;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
