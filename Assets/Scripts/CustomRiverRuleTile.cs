using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RiverRuleTile", menuName = "2D/Tiles/RiverRuleTile")]
public class NewCustomRuleTile : RuleTile<NewCustomRuleTile.Neighbor> {
    public TileBase[] tilesToConnect;
    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Any = 3;
        public const int MyGroup = 4;
        public const int NotMyGroup = 5;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) 
    {
        switch (neighbor) 
        {
            case Neighbor.This:
                return tile == this || IsInMyGroup(tile);
            case Neighbor.NotThis:
                return tile != this && !IsInMyGroup(tile);
            case Neighbor.Any:
                return tile != null;
            case Neighbor.MyGroup:
                return IsInMyGroup(tile);
            case Neighbor.NotMyGroup:
                return tile != null && !IsInMyGroup(tile);
        }
        return base.RuleMatch(neighbor, tile);
    }

    private bool IsInMyGroup(TileBase tile)
    {
        if (tilesToConnect == null) return false;

        foreach (var t in tilesToConnect)
        {
            if (t == tile) return true;
        }
        return false;
    }
}