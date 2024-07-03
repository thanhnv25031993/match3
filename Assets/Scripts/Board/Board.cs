using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Board
{
    public enum eMatchDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }

    private readonly int boardSizeX;

    private readonly int boardSizeY;

    private readonly Cell[,] m_cells;

    private readonly Transform m_root;

    private readonly int m_matchMin;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        boardSizeX = gameSettings.BoardSizeX;
        boardSizeY = gameSettings.BoardSizeY;

        m_cells = new Cell[boardSizeX, boardSizeY];

        CreateBoard();
    }

    private void CreateBoard()
    {
        var origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        // GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        AddressableManager.instance.AdressableLoadObjectByKey(Constants.PREFAB_CELL_BACKGROUND, prefabBG =>
        {
            for (var x = 0; x < boardSizeX; x++)
            for (var y = 0; y < boardSizeY; y++)
            {
                var go = Object.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                var cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }

            //set neighbours
            for (var x = 0; x < boardSizeX; x++)
            for (var y = 0; y < boardSizeY; y++)
            {
                if (y + 1 < boardSizeY) m_cells[x, y].NeighbourUp = m_cells[x, y + 1];
                if (x + 1 < boardSizeX) m_cells[x, y].NeighbourRight = m_cells[x + 1, y];
                if (y > 0) m_cells[x, y].NeighbourBottom = m_cells[x, y - 1];
                if (x > 0) m_cells[x, y].NeighbourLeft = m_cells[x - 1, y];
            }
        });
    }

    internal void Fill()
    {
        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            var cell = m_cells[x, y];
            var item = new NormalItem();

            var types = new List<NormalItem.eNormalType>();

            if (cell.NeighbourBottom != null)
            {
                var nitem = cell.NeighbourBottom.Item as NormalItem;
                if (nitem != null) types.Add(nitem.ItemType);
            }

            if (cell.NeighbourLeft != null)
            {
                var nitem = cell.NeighbourLeft.Item as NormalItem;
                if (nitem != null) types.Add(nitem.ItemType);
            }

            item.SetType(Utils.GetRandomNormalTypeExcept(types.ToArray()));
            item.SetView();
            item.SetViewRoot(m_root);

            cell.Assign(item);
            cell.ApplyItemPosition(false);
        }
    }

    internal void Shuffle()
    {
        var list = new List<Item>();
        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            list.Add(m_cells[x, y].Item);
            m_cells[x, y].Free();
        }

        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            var rnd = Random.Range(0, list.Count);
            m_cells[x, y].Assign(list[rnd]);
            m_cells[x, y].ApplyItemMoveToPosition();

            list.RemoveAt(rnd);
        }
    }

    internal void FillGapsWithNewItems()
    {
        var itemCounts = new Dictionary<NormalItem.eNormalType, int>();

        // Count item occurrences
        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            var cell = m_cells[x, y];
            if (cell.IsEmpty) continue;

            Debug.Log(cell.Item);
            if (cell.Item is BonusItem)
            {
                continue;
                ;
            }

            var itemType = ((NormalItem)cell.Item).ItemType; // Get existing item type
            if (!itemCounts.ContainsKey(itemType)) itemCounts.Add(itemType, 0);

            itemCounts[itemType]++;
        }

        var min = itemCounts.Min(m => m.Value);
        var itemFound = NormalItem.eNormalType.TYPE_ONE;
        foreach (var item in itemCounts.Where(item => item.Value == min)) itemFound = item.Key;

        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            var cell = m_cells[x, y];
            if (!cell.IsEmpty) continue;

            var item = new NormalItem();
            var type = GetValidItemTypes(x, y, itemFound);

            item.SetType(type);
            item.SetView();
            item.SetViewRoot(m_root);

            cell.Assign(item);
            cell.ApplyItemPosition(true);
        }
    }

    private NormalItem.eNormalType GetValidItemTypes(int x, int y, NormalItem.eNormalType minvalue)
    {
        var validTypes = Enum.GetValues(typeof(NormalItem.eNormalType)).Cast<NormalItem.eNormalType>().ToList();
        // Get surrounding cell types
        var leftType = (NormalItem.eNormalType)GetItemType(x - 1, y);
        var rightType = (NormalItem.eNormalType)GetItemType(x + 1, y);
        var topType = (NormalItem.eNormalType)GetItemType(x, y + 1);
        var bottomType = (NormalItem.eNormalType)GetItemType(x, y - 1);

        if ((int)leftType != -1)
            if (validTypes.Contains(leftType))
                validTypes.Remove(leftType);

        if ((int)rightType != -1)
            if (validTypes.Contains(rightType))
                validTypes.Remove(rightType);

        if ((int)topType != -1)
            if (validTypes.Contains(topType))
                validTypes.Remove(topType);

        if ((int)bottomType != -1)
            if (validTypes.Contains(bottomType))
                validTypes.Remove(bottomType);

        var type = Utils.GetRandomNormalTypeCustom(validTypes.ToArray());

        if (validTypes.Contains(minvalue)) type = minvalue;

        return type;
    }

// Helper function to get item type at a specific cell (replace with actual logic)
    private int GetItemType(int x, int y)
    {
        if (x < 0 || x >= boardSizeX || y < 0 || y >= boardSizeY) return -1; // Empty cell

        var cell = m_cells[x, y];
        if (cell.IsEmpty) return -1; // Empty cell

        return (int)((NormalItem)cell.Item).ItemType; // Get item type from cell
    }

    internal void ExplodeAllItems()
    {
        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            var cell = m_cells[x, y];
            cell.ExplodeItem();
        }
    }

    public void Swap(Cell cell1, Cell cell2, Action callback)
    {
        var item = cell1.Item;
        cell1.Free();
        var item2 = cell2.Item;
        cell1.Assign(item2);
        cell2.Free();
        cell2.Assign(item);

        item.View.DOMove(cell2.transform.position, 0.3f);
        item2.View.DOMove(cell1.transform.position, 0.3f)
            .OnComplete(() =>
            {
                if (callback != null) callback();
            });
    }

    public List<Cell> GetHorizontalMatches(Cell cell)
    {
        var list = new List<Cell>();
        list.Add(cell);

        //check horizontal match
        var newcell = cell;
        while (true)
        {
            var neib = newcell.NeighbourRight;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else
            {
                break;
            }
        }

        newcell = cell;
        while (true)
        {
            var neib = newcell.NeighbourLeft;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else
            {
                break;
            }
        }

        return list;
    }

    public List<Cell> GetVerticalMatches(Cell cell)
    {
        var list = new List<Cell>();
        list.Add(cell);

        var newcell = cell;
        while (true)
        {
            var neib = newcell.NeighbourUp;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else
            {
                break;
            }
        }

        newcell = cell;
        while (true)
        {
            var neib = newcell.NeighbourBottom;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else
            {
                break;
            }
        }

        return list;
    }

    internal void ConvertNormalToBonus(List<Cell> matches, Cell cellToConvert)
    {
        var dir = GetMatchDirection(matches);

        var item = new BonusItem();
        switch (dir)
        {
            case eMatchDirection.ALL:
                item.SetType(BonusItem.eBonusType.ALL);
                break;
            case eMatchDirection.HORIZONTAL:
                item.SetType(BonusItem.eBonusType.HORIZONTAL);
                break;
            case eMatchDirection.VERTICAL:
                item.SetType(BonusItem.eBonusType.VERTICAL);
                break;
        }

        if (item != null)
        {
            if (cellToConvert == null)
            {
                var rnd = Random.Range(0, matches.Count);
                cellToConvert = matches[rnd];
            }

            item.SetView();
            item.SetViewRoot(m_root);

            cellToConvert.Free();
            cellToConvert.Assign(item);
            cellToConvert.ApplyItemPosition(true);
        }
    }

    internal eMatchDirection GetMatchDirection(List<Cell> matches)
    {
        if (matches == null || matches.Count < m_matchMin) return eMatchDirection.NONE;

        var listH = matches.Where(x => x.BoardX == matches[0].BoardX).ToList();
        if (listH.Count == matches.Count) return eMatchDirection.VERTICAL;

        var listV = matches.Where(x => x.BoardY == matches[0].BoardY).ToList();
        if (listV.Count == matches.Count) return eMatchDirection.HORIZONTAL;

        if (matches.Count > 5) return eMatchDirection.ALL;

        return eMatchDirection.NONE;
    }

    internal List<Cell> FindFirstMatch()
    {
        var list = new List<Cell>();

        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            var cell = m_cells[x, y];

            var listhor = GetHorizontalMatches(cell);
            if (listhor.Count >= m_matchMin)
            {
                list = listhor;
                break;
            }

            var listvert = GetVerticalMatches(cell);
            if (listvert.Count >= m_matchMin)
            {
                list = listvert;
                break;
            }
        }

        return list;
    }

    public List<Cell> CheckBonusIfCompatible(List<Cell> matches)
    {
        var dir = GetMatchDirection(matches);

        var bonus = matches.Where(x => x.Item is BonusItem).FirstOrDefault();
        if (bonus == null) return matches;

        var result = new List<Cell>();
        switch (dir)
        {
            case eMatchDirection.HORIZONTAL:
                foreach (var cell in matches)
                {
                    var item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.HORIZONTAL) result.Add(cell);
                }

                break;
            case eMatchDirection.VERTICAL:
                foreach (var cell in matches)
                {
                    var item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.VERTICAL) result.Add(cell);
                }

                break;
            case eMatchDirection.ALL:
                foreach (var cell in matches)
                {
                    var item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.ALL) result.Add(cell);
                }

                break;
        }

        return result;
    }

    internal List<Cell> GetPotentialMatches()
    {
        var result = new List<Cell>();
        for (var x = 0; x < boardSizeX; x++)
        {
            for (var y = 0; y < boardSizeY; y++)
            {
                var cell = m_cells[x, y];

                //check right
                /* example *\
                  * * * * *
                  * * * * *
                  * * * ? *
                  * & & * ?
                  * * * ? *
                \* example  */

                if (cell.NeighbourRight != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourRight, cell.NeighbourRight.NeighbourRight);
                    if (result.Count > 0) break;
                }

                //check up
                /* example *\
                  * ? * * *
                  ? * ? * *
                  * & * * *
                  * & * * *
                  * * * * *
                \* example  */
                if (cell.NeighbourUp != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourUp, cell.NeighbourUp.NeighbourUp);
                    if (result.Count > 0) break;
                }

                //check bottom
                /* example *\
                  * * * * *
                  * & * * *
                  * & * * *
                  ? * ? * *
                  * ? * * *
                \* example  */
                if (cell.NeighbourBottom != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourBottom, cell.NeighbourBottom.NeighbourBottom);
                    if (result.Count > 0) break;
                }

                //check left
                /* example *\
                  * * * * *
                  * * * * *
                  * ? * * *
                  ? * & & *
                  * ? * * *
                \* example  */
                if (cell.NeighbourLeft != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourLeft, cell.NeighbourLeft.NeighbourLeft);
                    if (result.Count > 0) break;
                }

                /* example *\
                  * * * * *
                  * * * * *
                  * * ? * *
                  * & * & *
                  * * ? * *
                \* example  */
                var neib = cell.NeighbourRight;
                if (neib != null && neib.NeighbourRight != null && neib.NeighbourRight.IsSameType(cell))
                {
                    var second = LookForTheSecondCellVertical(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourRight);
                        result.Add(second);
                        break;
                    }
                }

                /* example *\
                  * * * * *
                  * & * * *
                  ? * ? * *
                  * & * * *
                  * * * * *
                \* example  */
                neib = null;
                neib = cell.NeighbourUp;
                if (neib != null && neib.NeighbourUp != null && neib.NeighbourUp.IsSameType(cell))
                {
                    var second = LookForTheSecondCellHorizontal(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourUp);
                        result.Add(second);
                        break;
                    }
                }
            }

            if (result.Count > 0) break;
        }

        return result;
    }

    private List<Cell> GetPotentialMatch(Cell cell, Cell neighbour, Cell target)
    {
        var result = new List<Cell>();

        if (neighbour != null && neighbour.IsSameType(cell))
        {
            var third = LookForTheThirdCell(target, neighbour);
            if (third != null)
            {
                result.Add(cell);
                result.Add(neighbour);
                result.Add(third);
            }
        }

        return result;
    }

    private Cell LookForTheSecondCellHorizontal(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look right
        Cell second = null;
        second = target.NeighbourRight;
        if (second != null && second.IsSameType(main)) return second;

        //look left
        second = null;
        second = target.NeighbourLeft;
        if (second != null && second.IsSameType(main)) return second;

        return null;
    }

    private Cell LookForTheSecondCellVertical(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up        
        var second = target.NeighbourUp;
        if (second != null && second.IsSameType(main)) return second;

        //look bottom
        second = null;
        second = target.NeighbourBottom;
        if (second != null && second.IsSameType(main)) return second;

        return null;
    }

    private Cell LookForTheThirdCell(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up
        var third = CheckThirdCell(target.NeighbourUp, main);
        if (third != null) return third;

        //look right
        third = null;
        third = CheckThirdCell(target.NeighbourRight, main);
        if (third != null) return third;

        //look bottom
        third = null;
        third = CheckThirdCell(target.NeighbourBottom, main);
        if (third != null) return third;

        //look left
        third = null;
        third = CheckThirdCell(target.NeighbourLeft, main);
        ;
        if (third != null) return third;

        return null;
    }

    private Cell CheckThirdCell(Cell target, Cell main)
    {
        if (target != null && target != main && target.IsSameType(main)) return target;

        return null;
    }

    internal void ShiftDownItems()
    {
        for (var x = 0; x < boardSizeX; x++)
        {
            var shifts = 0;
            for (var y = 0; y < boardSizeY; y++)
            {
                var cell = m_cells[x, y];
                if (cell.IsEmpty)
                {
                    shifts++;
                    continue;
                }

                if (shifts == 0) continue;

                var holder = m_cells[x, y - shifts];

                var item = cell.Item;
                cell.Free();

                holder.Assign(item);
                item.View.DOMove(holder.transform.position, 0.3f);
            }
        }
    }

    public void Clear()
    {
        for (var x = 0; x < boardSizeX; x++)
        for (var y = 0; y < boardSizeY; y++)
        {
            var cell = m_cells[x, y];
            cell.Clear();

            Object.Destroy(cell.gameObject);
            m_cells[x, y] = null;
        }
    }
}