using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchInfo
{
    public List<GridItem> hmatch;
    public List<GridItem> vmatch;
    public int matchStartingX;
    public int matchEndingX;
    public int matchStartingY;
    public int matchEndingY;
    public int intersectionX;
    public int intersectionY;

    public MatchInfo()
    {
        hmatch = new List<GridItem>();
        vmatch = new List<GridItem>();
    }

    public bool validMatch
    {
        get { return (hmatch != null && hmatch.Count > 0) || (vmatch != null && vmatch.Count > 0); }
    }
}
