using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class GridItem : MonoBehaviour {

    public int x
    {
        get;
        private set;
    }

    public int y
    {
        get;
        private set;
    }

    public float timeBonus
    {
        get;
        private set;
    }

    public bool isMoving
    {
        get;
        set;
    }

    public Colors color;
    public string anim_name;

    [HideInInspector]
    public int id;
    public Animator anim;

    private void Start()
    {
        timeBonus = CalculateTimeBonus();        
        anim = GetComponent<Animator>();
        anim.SetFloat("Time_Bonus", timeBonus);
        anim.SetInteger("Color", (int)color);
        GameGrid.OnGemDestroyedEventHandler += OnGemDestroyed;
    }

    private void OnDisable()
    {
        GameGrid.OnGemDestroyedEventHandler -= OnGemDestroyed;
    }

    public void OnGemDestroyed(List<GridItem> items)
    {
        foreach(GridItem i in items)
        {
            if(i.x == x && i.y == y)
            {
                anim.SetTrigger("Destroyed");
            }
        }
    }

    public IEnumerator DestroyAnimation()
    {
        /*yield return new WaitUntil(() => anim.isInitialized);
        anim.SetBool(anim_name,true);
        Debug.Log("Animation started");
        yield return new WaitWhile(() =>    anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
        yield return new WaitWhile(() =>    anim.GetCurrentAnimatorStateInfo(0).IsName(anim_name));*/
        Destroy(gameObject);
        yield return null;
    }

    public void OnItemPositionChanged(int newX, int newY)
    {
        x = newX;
        y = newY;
        gameObject.name = string.Format("Sprite [{0}][{1}]", x, y);
    }

    float CalculateTimeBonus()
    {
        int chance = Random.Range(0, 10);

        if (chance == 4)
            return Random.Range(1f, 5f);
        else
            return 0f;
    }

    private void OnMouseDown()
    {
        if (OnMouseOverItemEventHandler != null)
        {
            OnMouseOverItemEventHandler(this);
        }
    }

    public delegate void OnMouseOverItem(GridItem item);
    public static event OnMouseOverItem OnMouseOverItemEventHandler;
}
