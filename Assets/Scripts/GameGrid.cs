using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class GameGrid : MonoBehaviour {
    public int xSize, ySize;
    public float gemWidth = 1f;
    public float gemHeight = 1f;
    private GameObject[] _gems;
    private GridItem[,] _items;
    private GridItem _currentlySelectedItem;
    public float delayBetweenMatches = 0.2f;
    public static int minItemsForMatch = 3;
    public int pointsPerGemDestroyed = 60;
    public bool canPlay;
    public Canvas targetCanvas;

    public bool swapping
    {
        get;
        private set;
    }

    private void Start()
    {
        canPlay = true;
        GetGems();
        FillGrid();
        ClearGrid();

        //Events
        GridItem.OnMouseOverItemEventHandler += OnMouseOverItem;
        GameController.OnGameOverEventHandler += OnGameOver;
        SwipeControl.OnSwipeEventHandler += OnSwipe;
    }

    private void OnDisable()
    {
        //Unsubscribe Events
        GridItem.OnMouseOverItemEventHandler -= OnMouseOverItem;
        GameController.OnGameOverEventHandler -= OnGameOver;
        SwipeControl.OnSwipeEventHandler -= OnSwipe;
    }

    //Instiate gems in the initial grid
    void FillGrid()
    {   
        _items = new GridItem[xSize, ySize];

        for ( int x = 0; x < xSize; x++ )
        {
            for ( int y = 0; y < ySize; y++ )
            {
                _items[x, y] = InstantiateGem(x, y);
            }
        }
    }

    void FillGridNoMatches()
    {
        int type = 0;
        _items = new GridItem[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                _items[x, y] = InstantiateGem(x, y, type);
                type += 1;
                if (type >= 4)
                    type = 0;
            }
        }
    }

    //Scan grid to clear matches
    void ClearGrid()
    {
        for(int x = 0;x<xSize;x++)
        {
            for(int y = 0;y<ySize;y++)
            {
                MatchInfo matchInfo = GetMatchInformation(_items[x, y]);
                if(matchInfo.validMatch)
                {
                    Destroy(_items[x, y].gameObject);
                    _items[x,y] = InstantiateGem(x, y);
                    y--;
                }
            }
        }
    }

    GridItem InstantiateGem(int x, int y, int type = -1)
    {
        GameObject randomGem;

        //Check gem type
        if (type == -1)        
            randomGem = _gems[Random.Range(0, _gems.Length)];        
        else        
            randomGem = _gems[type];        

        //Instantiate gem in the correct position
        GridItem newGem = ((GameObject)Instantiate(randomGem, new Vector3(x * gemWidth - ((xSize/2 * gemWidth)), y * gemHeight),Quaternion.identity)).GetComponent<GridItem>();
        newGem.gameObject.GetComponent<Renderer>().sortingLayerName = "MiddleLayer";
        newGem.transform.SetParent(targetCanvas.transform);
        newGem.OnItemPositionChanged(x, y);

        return newGem;
    }    

    void OnMouseOverItem(GridItem item)
    {
        if (_currentlySelectedItem == item || !canPlay )
        {
            Debug.Log("Não pode jogar!");
            return;
        }

        if(_currentlySelectedItem == null)
        {
            _currentlySelectedItem = item;
        }
        else
        {
            //Check if items are close to each other and can be swapped
            float xDiff = Mathf.Abs(item.x - _currentlySelectedItem.x);
            float yDiff = Mathf.Abs(item.y - _currentlySelectedItem.y);
            Debug.Log(string.Format("[{0},{1}][{2},{3}] = [{4}]", item.x, item.y, _currentlySelectedItem.x, _currentlySelectedItem.y, xDiff + yDiff));
            if (xDiff + yDiff == 1)            
                StartCoroutine(TryMatch(_currentlySelectedItem, item));              
            else            
                Debug.LogError("Esses itens estão mais de 1 unidade de distancia.");
            
            _currentlySelectedItem = null;
        }
    }

    //Handles Swipe
    void OnSwipe(Vector3 fp, Directions dir)
    {
        Collider2D itemCollider;
        GridItem item1 = null;
        GridItem item2 = null;

        //Check if first point of contact is an item
        Vector3 wp1 = Camera.main.ScreenToWorldPoint(fp);
        itemCollider = Physics2D.OverlapPoint(new Vector2(wp1.x, wp1.y));
        if (itemCollider != null)
        {
            item1 = itemCollider.gameObject.GetComponent<GridItem>();
            OnMouseOverItem(item1);
            switch (dir)
            {
                case Directions.Top:
                    if (item1.y + 1 <= ySize)
                        item2 = _items[item1.x, item1.y + 1];
                    break;
                case Directions.Bottom:
                    if (item1.y - 1 >= 0)
                        item2 = _items[item1.x, item1.y - 1];
                    break;
                case Directions.Left:
                    if (item1.x - 1 >= 0)
                        item2 = _items[item1.x - 1, item1.y];
                    break;
                case Directions.Right:
                    if (item1.x + 1 <= xSize)
                        item2 = _items[item1.x + 1, item1.y];
                    break;
            }

            if (item2 != null)
                OnMouseOverItem(item2);
        }

        /*//Check if last point of contact is an item
        Vector3 wp2 = Camera.main.ScreenToWorldPoint(lp);
        itemCollider = Physics2D.OverlapPoint(new Vector2(wp2.x, wp2.y));
        if (itemCollider != null)
        {
            item2 = itemCollider.gameObject.GetComponent<GridItem>();
            OnMouseOverItem(item2);
        }*/  
    }

    //Handles game over
    void OnGameOver(GameController controller)
    {
        //Set score on game over panel
        controller.SetGameOverText(string.Format("Pontos obtidos: {0}", GetComponent<ScoreBoard>().points));
    }

    void IncreasePointsOnMatch(long newPoints)
    {
        gameObject.GetComponent<ScoreBoard>().points += newPoints;
    }

    IEnumerator TryMatch(GridItem a, GridItem b)
    {
        canPlay = false;
        yield return StartCoroutine(Swap(a, b));
        //Get match information for items on the new positions
        MatchInfo matchA = GetMatchInformation(a);
        MatchInfo matchB = GetMatchInformation(b);
        //Check if there is a valid match
        if(!matchA.validMatch && !matchB.validMatch)
        {
            yield return StartCoroutine(Swap(a, b));
            yield return StartCoroutine(CheckIfMatchesLeft());
        }
        else if (matchA.validMatch)
        {
            ProcessPointsAndBonus(matchA);
            yield return StartCoroutine(DestroyItems(matchA));
            yield return new WaitForSeconds(delayBetweenMatches);
            yield return StartCoroutine(UpdateGridAfterMatch(matchA));            
        }
        else if (matchB.validMatch)
        {
            ProcessPointsAndBonus(matchB);
            yield return StartCoroutine(DestroyItems(matchB));
            yield return new WaitForSeconds(delayBetweenMatches);
            yield return StartCoroutine(UpdateGridAfterMatch(matchB));
        }
        yield return StartCoroutine(CheckIfMatchesLeft());
        canPlay = true;
    }

    private void HandleHorizontalMatch(MatchInfo match)
    {
        //match horizontal
        Debug.Log("Handling horizontal match.");
        //loop each column affected by match
        for (int x = match.matchStartingX; x <= match.matchEndingX; x++)
        {
            //loop each item in column affected by gem
            if (match.matchStartingY != match.matchEndingY)
            {
                if (x != match.intersectionX)
                {
                    for (int y = match.intersectionY; y < ySize - 1; y++)
                    {
                        //swap items accordingly                 
                        GridItem upperIndex = _items[x, y + 1];
                        GridItem current = _items[x, y];
                        _items[x, y] = upperIndex;
                        _items[x, y + 1] = current;
                        _items[x, y].OnItemPositionChanged(_items[x, y].x, _items[x, y].y - 1);
                    }
                    //instantiate a new item to fill holes
                    _items[x, ySize - 1] = InstantiateGem(x, ySize - 1);
                }
            }
            else
            {
                for (int y = match.intersectionY; y < ySize - 1; y++)
                {
                    //swap items accordingly                 
                    GridItem upperIndex = _items[x, y + 1];
                    GridItem current = _items[x, y];
                    _items[x, y] = upperIndex;
                    _items[x, y + 1] = current;
                    _items[x, y].OnItemPositionChanged(_items[x, y].x, _items[x, y].y - 1);
                }
                //instantiate a new item to fill holes
                _items[x, ySize - 1] = InstantiateGem(x, ySize - 1);
            }
        }
    }

    private void HandleVerticalMatch(MatchInfo match)
    {
        //match vertical        
        int matchHeight;
        Debug.Log("Handling vertical");
        //get match vertical height
        matchHeight = 1 + (match.matchEndingY - match.matchStartingY);
        //loop each item on top of match
        for (int y = match.matchStartingY + matchHeight; y <= ySize - 1; y++)
        {
            //swap items on the grid to update holes
            GridItem lowerIndex = _items[match.intersectionX, y - matchHeight];
            GridItem current = _items[match.intersectionX, y];
            _items[match.intersectionX, y - matchHeight] = current;
            _items[match.intersectionX, y] = lowerIndex;
        }

        //start item position change event for items on top of match
        for (int y = 0; y < ySize - matchHeight; y++)
        {
            _items[match.intersectionX, y].OnItemPositionChanged(match.intersectionX, y);
        }

        //instantiate new items on the count of items that were destroyed
        for (int i = 0; i < match.vmatch.Count; i++)
        {
            _items[match.intersectionX, (ySize - 1) - i] = InstantiateGem(match.intersectionX, (ySize - 1) - i);
        }
    }

    IEnumerator UpdateGridAfterMatch(MatchInfo match)
    {
        yield return new WaitWhile(() => IsMoving());

        if (match.matchStartingY == match.matchEndingY)
        {
            HandleHorizontalMatch(match);
        }
        else if(match.matchStartingX == match.matchEndingX)
        {
            HandleVerticalMatch(match);
        }
        else
        {
            HandleVerticalMatch(match);
            HandleHorizontalMatch(match);            
        }        

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                MatchInfo matchInfo = GetMatchInformation(_items[x, y]);
                if (matchInfo.validMatch)
                {                    
                    yield return StartCoroutine(DestroyItems(matchInfo));
                    ProcessPointsAndBonus(matchInfo);
                    yield return new WaitForSeconds(delayBetweenMatches);
                    yield return StartCoroutine(UpdateGridAfterMatch(matchInfo));
                }
            }
        }        
    }

    bool IsMoving(GridItem i = null)
    {
        if(i != null)
        {
            if (i.GetComponent<Rigidbody2D>().velocity.y < -0.3f)
                return true;
            else
                return false;
        }
        else
        {
            foreach(GridItem it in _items)
            {
                if (it == null)
                    continue;
                if (it.GetComponent<Rigidbody2D>().velocity.y < -0.3f)
                    return true;                    
            }

            return false;
        }
    }

    void ProcessPointsAndBonus(MatchInfo match)
    {
        //process points and bonus horizontal
        foreach (GridItem i in match.hmatch)
        {
            IncreasePointsOnMatch(pointsPerGemDestroyed);
            gameObject.GetComponent<Timer>().gameDuration += i.timeBonus;
        }

        //process points and bonus vertical
        foreach (GridItem i in match.vmatch)
        {
            IncreasePointsOnMatch(pointsPerGemDestroyed);
            gameObject.GetComponent<Timer>().gameDuration += i.timeBonus;
        }
    }

    IEnumerator DestroyItems(MatchInfo match)
    {
        yield return new WaitWhile(() => IsMoving());        

        //send destroy events to gems
        if (OnGemDestroyedEventHandler != null)
        {
            OnGemDestroyedEventHandler(match.hmatch);
            OnGemDestroyedEventHandler(match.vmatch);
        }

        //Wait for gems to be destroyed
        foreach (GridItem i in match.hmatch)        
            yield return new WaitUntil(() => i == null);        

        foreach (GridItem i in match.vmatch)        
            yield return new WaitUntil(() => i == null);        
    }

    IEnumerator ResetGrid()
    {
        Debug.Log("No possible matches. Resetting grid...");
        MatchInfo m = new MatchInfo();
        foreach (GridItem i in _items)
            m.hmatch.Add(i);
        yield return StartCoroutine(DestroyItems(m));
        FillGrid();
        ClearGrid();
    }

    IEnumerator Swap(GridItem a, GridItem b)
    {
        yield return new WaitWhile(() => swapping);
        swapping = true;
        ChangeRigidBodyStatus(false); //desativar corpos rigidos
        Vector3 aPosition = a.transform.position;
        float delay = 0.1f;
        StartCoroutine(a.transform.Move(b.transform.position, delay));
        StartCoroutine(b.transform.Move(aPosition, delay));
        yield return new WaitWhile(() => a.isMoving || b.isMoving);
        SwapIndices(a, b);
        ChangeRigidBodyStatus(true); //ativar corpos rigidos
        swapping = false;
    }

    void SwapIndices(GridItem a, GridItem b)
    {
        GridItem tempA = _items[a.x, a.y];
        _items[a.x, a.y] = b;
        _items[b.x, b.y] = tempA;
        int bOldX = b.x; int bOldY = b.y;
        b.OnItemPositionChanged(a.x, a.y);
        a.OnItemPositionChanged(bOldX, bOldY);
    }

    List<GridItem> SearchHorizontally(GridItem item)
    {
        List<GridItem> hItem = new List<GridItem> {item};
        int left = item.x - 1;
        int right = item.x + 1;
        while(left >= 0 && _items[left, item.y] != null && _items[left, item.y].id == item.id)
        {
            hItem.Add(_items[left, item.y]);
            left--;
        }
        while (right < xSize && _items[right, item.y] != null && _items[right, item.y].id == item.id)
        {
            hItem.Add(_items[right, item.y]);
            right++;
            Debug.Log(right);
        }

        return hItem;
    }

    List<GridItem> SearchVertically(GridItem item)
    {
        List<GridItem> vItem = new List<GridItem> { item };
        int up = item.y + 1;
        int down = item.y - 1;
        while (down >= 0 && _items[item.x, down] != null && _items[item.x, down].id == item.id)
        {
            vItem.Add(_items[item.x, down]);
            down--;
        }
        while (up < ySize && _items[item.x, up] != null && _items[item.x, up].id == item.id)
        {
            vItem.Add(_items[item.x, up]);
            up++;
        }

        return vItem;
    }

    IEnumerator CheckIfMatchesLeft()
    {
        bool u = false, d = false, l = false, r = false;

        foreach(GridItem i in _items)
        {
            if (i.y - 1 >= 0)            
                u = HasMatches(i, _items[i.x, i.y - 1]);                       

            if (i.y + 1 < ySize)            
                d = HasMatches(i, _items[i.x, i.y + 1]);                       

            if (i.x - 1 >= 0)            
                l = HasMatches(i, _items[i.x - 1, i.y]);                        

            if (i.x + 1 < xSize)            
                r = HasMatches(i, _items[i.x + 1, i.y]);                        

            //Check if possible matches
            if(u || d || l || r)            
                yield break;                        
        }

        //No possible matches
        yield return StartCoroutine(ResetGrid());

    }

    bool HasMatches(GridItem a, GridItem b)
    {
        SwapIndices(a, b);
        MatchInfo ma = GetMatchInformation(a);
        MatchInfo mb = GetMatchInformation(b);
        SwapIndices(b, a);

        if(ma.validMatch || mb.validMatch)        
            return true;        
        else     
            return false;        
    }

    MatchInfo GetMatchInformation(GridItem item)
    {
        MatchInfo m = new MatchInfo();
        List<GridItem> hMatch = SearchHorizontally(item);
        List<GridItem> vMatch = SearchVertically(item);
        if(hMatch.Count >= minItemsForMatch)
        {
            //definir infos para match horizontal
            m.matchStartingX = GetMinimumX(hMatch);
            m.matchEndingX = GetMaximumX(hMatch);
            if(m.matchStartingY == 0 && m.matchEndingY == 0)
                m.matchStartingY = m.matchEndingY = hMatch[0].y;
            m.hmatch.InsertRange(0,hMatch);
        }

        if (vMatch.Count >= minItemsForMatch)
        {
            //definit infos para match vertical
            m.matchStartingY = GetMinimumY(vMatch);
            m.matchEndingY = GetMaximumY(vMatch);
            if (m.matchStartingX == 0 && m.matchEndingX == 0)
                m.matchStartingX = m.matchEndingX = vMatch[0].x;
            m.vmatch.InsertRange(0, vMatch);
        }

        //set intersection between horizontal and vertical as the moved item
        m.intersectionX = item.x;
        m.intersectionY = item.y;

        return m;
    }

    int GetMinimumX(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0;i<indices.Length;i++)
        {
            indices[i] = items[i].x;
        }
        return (int)Mathf.Min(indices);
    }

    int GetMaximumX(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = items[i].x;
        }
        return (int)Mathf.Max(indices);
    }

    int GetMinimumY(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = items[i].y;
        }
        return (int)Mathf.Min(indices);
    }

    int GetMaximumY(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = items[i].y;
        }
        return (int)Mathf.Max(indices);
    }

    void GetGems()
    {
        _gems = Resources.LoadAll<GameObject>("Prefabs");

        for(int i = 0; i<_gems.Length; i++)
        {
            _gems[i].GetComponent<GridItem>().id = i;
        }
    }

    void ChangeRigidBodyStatus(bool status)
    {
        foreach(GridItem g in _items)
        {
            g.GetComponent<Rigidbody2D>().isKinematic = !status;
        }
        Debug.Log("Kinematic changed to " + !status);
    }

    public delegate void OnGemDestroyed(List<GridItem> items);
    public static event OnGemDestroyed OnGemDestroyedEventHandler;
}
