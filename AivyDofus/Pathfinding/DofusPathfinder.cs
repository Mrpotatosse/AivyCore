using AivyDofus.DofusMap.Map;
using AivyDofus.Pathfinding.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AivyDofus.Pathfinding
{
    // com.ankamagames.dofus.types.entities
    // com.ankamagames.atouin.utils.DataMapProvider
    public class DofusPathfinder : IDisposable
    {
        static readonly uint HV_COST = 10;
        static readonly uint DIAG_COST = 15;
        static readonly uint HEURISTIC_COST = 10;
        static readonly int INFINITE_COST = 99999999;

        private int[] _parentOfCell;
        private double[] _costOfCell;
        private double[] _openListWeights;
        private bool[] _isCellClosed;
        private bool[] _isEntityOnCell;

        private List<int> _openList { get; set; } 

        private readonly GameMapData MapData;
        private readonly Map Map;

        private readonly bool _allowTroughEntity;
        private readonly bool _allowDiagonal;
        private readonly bool _avoidObstacle;

        private readonly bool _isInFight;

        public void Dispose()
        {
            Map.Clear();
            _parentOfCell = null;
            _costOfCell = null;
            _openListWeights = null;
            _isCellClosed = null;
            _isEntityOnCell = null;
            _openList.Clear();
            _openList = null;   
        }

        public DofusPathfinder(GameMapData map_data, bool inFight = false, bool troughEntity = true, bool diagonal = true, bool obstacle = true)
        {
            MapData = map_data;
            Map = map_data.CurrentMap;

            _isInFight = inFight;
            _allowTroughEntity = troughEntity;
            _allowDiagonal = diagonal;
            _avoidObstacle = obstacle;

            int cellsCount = Map.CellsCount;
            _parentOfCell = new int[cellsCount];
            _costOfCell = new double[cellsCount];
            _openListWeights = new double[cellsCount];
            _isCellClosed = new bool[cellsCount];
            _isEntityOnCell = new bool[cellsCount];

            for(int i = 0;i < cellsCount; i++)
            {
                _parentOfCell[i] = -1;
                _costOfCell[i] = -1;//
                _openListWeights[i] = -1;//
                _isCellClosed[i] = false;
                _isEntityOnCell[i] = MapData.Actors.ContainsKey(i);// to do check with map_data
            }

            _openList = new List<int>(40);
        }

        public MovementPath FindPath(int start, int end)
        {
            MapPoint start_point = new MapPoint(start);
            MapPoint end_point = new MapPoint(end);

            return FindPath(start_point, end_point);
        }

        public MovementPath FindPath(MapPoint start, MapPoint end)
        {
            int i;
            int y;
            int x;
            
            double minimum;
            int smallestCostIndex;

            double cost;

            int parentId;
            MapPoint parent;

            MapPoint grandParent;
            MapPoint grandGrandParent;

            MapPoint cell;

            double pointWeight;
            int speed;
            double movementCost;

            bool cellOnEndColumn;
            bool cellOnStartColumn;
            bool cellOnEndLine;
            bool cellOnStartLine;

            int distanceTmpToEnd;
            int distanceToEnd = start.DistanceToCell(end);

            int endCellAuxId = start.CellId;
            double heuristic;

            MapPoint k;
            MapPoint next;
            MapPoint inter;

            _costOfCell[start.CellId] = 0;
            _openList.Add(start.CellId);

            while(_openList.Count > 0 && !_isCellClosed[end.CellId])
            {
                minimum = INFINITE_COST;
                smallestCostIndex = 0;
                for(i = 0; i < _openList.Count; i++)
                {
                    cost = _openListWeights[_openList[i]];
                    if(cost <= minimum)
                    {
                        minimum = cost;
                        smallestCostIndex = i;
                    }
                }

                parentId = _openList[smallestCostIndex];
                parent = new MapPoint(parentId);
                _openList.RemoveAt(smallestCostIndex);
                _isCellClosed[parentId] = true;

                for(y = parent.Y - 1; y <= parent.Y + 1; y++)
                {
                    for(x = parent.X - 1; x <= parent.X + 1; x++)
                    {
                        cell = new MapPoint(x, y);

                        if(cell.IsInMap() && !_isCellClosed[cell.CellId] && cell.CellId != parentId 
                        && _pointMov(new MapPoint(x,y), _allowTroughEntity, parentId, end.CellId, _avoidObstacle)
                        && (y == parent.Y || x == parent.X || _allowDiagonal && 
                            (_pointMov(new MapPoint(parent.X, y), _allowTroughEntity, parentId, end.CellId, _avoidObstacle) 
                           || _pointMov(new MapPoint(x, parent.Y), _allowTroughEntity, parentId, end.CellId, _avoidObstacle))))
                        {
                            pointWeight = 0;
                            if(cell.CellId != end.CellId)
                            {
                                pointWeight = 1;
                            }
                            else
                            {
                                speed = Map.Cells[cell.CellId].speed;
                                if (_allowTroughEntity)
                                {
                                    if (_isEntityOnCell[cell.CellId])
                                    {
                                        pointWeight = 20;
                                    }
                                    else if(speed >= 0)
                                    {
                                        pointWeight = 6 - speed;
                                    }
                                    else
                                    {
                                        pointWeight = 12 + Math.Abs(speed);
                                    }
                                }
                                else
                                {
                                    pointWeight = 1;
                                    if (_isEntityOnCell[cell.CellId])
                                    {
                                        pointWeight += 0.3;
                                    }
                                    if(new MapPoint(x + 1, y) is MapPoint r && r.IsInMap() && _isEntityOnCell[r.CellId])
                                    {
                                        pointWeight += 0.3;
                                    }
                                    if (new MapPoint(x, y + 1) is MapPoint b && b.IsInMap() && _isEntityOnCell[b.CellId])
                                    {
                                        pointWeight += 0.3;
                                    }
                                    if (new MapPoint(x - 1, y) is MapPoint l && l.IsInMap() && _isEntityOnCell[l.CellId])
                                    {
                                        pointWeight += 0.3;
                                    }
                                    if (new MapPoint(x, y - 1) is MapPoint t && t.IsInMap() && _isEntityOnCell[t.CellId])
                                    {
                                        pointWeight += 0.3;
                                    }
                                    // to do special effect
                                    //if((specialEffect(cell.CellId) & 2) == 2)
                                    //{
                                    //  pointWeight += 0.2;
                                    //}
                                }
                            }
                            movementCost = _costOfCell[parentId] + (y == parent.Y || x == parent.X ? HV_COST : DIAG_COST) * pointWeight;
                            if (_allowTroughEntity)
                            {
                                cellOnEndColumn = x + y == end.X + end.Y;
                                cellOnStartColumn = x + y == start.X + start.Y;
                                cellOnEndLine = x - y == end.X - end.Y;
                                cellOnStartLine = x - y == start.X - start.Y;

                                if (!cellOnEndColumn && !cellOnEndLine || !cellOnStartColumn && !cellOnStartLine)
                                {
                                    movementCost += cell.DistanceToCell(end); //MapTools.getDistance(cellId, endCellId);
                                    movementCost += cell.DistanceToCell(start); //movementCost += MapTools.getDistance(cellId, startCellId);
                                }
                                if (x == end.X || y == end.Y)
                                {
                                    movementCost -= 3;
                                }
                                if (cellOnEndColumn || cellOnEndLine || x + y == parent.X + parent.Y || x - y == parent.X - parent.Y)
                                {
                                    movementCost -= 2;
                                }
                                if (i == start.X || y == start.Y)
                                {
                                    movementCost -= 3;
                                }
                                if (cellOnStartColumn || cellOnStartLine)
                                {
                                    movementCost -= 2;
                                }
                                distanceTmpToEnd = cell.DistanceToCell(end);
                                if(distanceTmpToEnd < distanceToEnd)
                                {
                                    endCellAuxId = cell.CellId;
                                    distanceToEnd = distanceTmpToEnd;
                                }
                            }
                            if(_parentOfCell[cell.CellId] == -1 || movementCost < _costOfCell[cell.CellId])
                            {
                                _parentOfCell[cell.CellId] = parentId;
                                _costOfCell[cell.CellId] = movementCost;
                                heuristic = HEURISTIC_COST * Math.Sqrt(((end.Y - y) * (end.Y - y)) + ((end.X - x) * (end.X - x)));
                                _openListWeights[cell.CellId] = heuristic + movementCost;
                            
                                if(_openList.IndexOf(cell.CellId) == -1)
                                {
                                    _openList.Add(cell.CellId);
                                }
                            }
                        }
                    }
                }
            }

            MovementPath path = new MovementPath
            {
                CellStart = start,
                Cells = new List<PathElement>(),
                CellEnd = _parentOfCell[end.CellId] == -1 ? new MapPoint(endCellAuxId) : end
            };

            for(int cursor = path.CellEnd.CellId; cursor != start.CellId; cursor = _parentOfCell[cursor])
            {
                if (_allowDiagonal)
                {
                    parent = new MapPoint(_parentOfCell[cursor]);
                    grandParent = parent.CellId == -1 ? new MapPoint(-1) : new MapPoint(_parentOfCell[parent.CellId]);
                    grandGrandParent = grandParent.CellId == -1 ? new MapPoint(-1) : new MapPoint(_parentOfCell[grandParent.CellId]);

                    k = new MapPoint(cursor);
                    if(grandParent.CellId != -1 && k.DistanceToCell(grandParent) == 1)
                    {
                        if(_pointMov(k, _allowTroughEntity, grandParent.CellId, path.CellEnd.CellId, true))
                        {
                            _parentOfCell[cursor] = grandParent.CellId;
                        }
                    }
                    else if (grandGrandParent.CellId != -1 && k.DistanceToCell(grandGrandParent) == 2)
                    {
                        next = new MapPoint(grandGrandParent.CellId);
                        inter = new MapPoint(k.X + ((next.X - k.X) / 2), k.Y + ((next.Y - k.Y) / 2));
                        if(_pointMov(inter, _allowTroughEntity, cursor, path.CellEnd.CellId, true)
                        && _pointWeight(inter) < 2)
                        {
                            _parentOfCell[cursor] = inter.CellId;
                        }
                    }
                    else if (grandParent.CellId != -1 && k.DistanceToCell(grandParent) == 2)
                    {
                        next = new MapPoint(grandParent.CellId);
                        inter = new MapPoint(parent.CellId);

                        if (k.X + k.Y == next.X + next.Y 
                         && k.X - k.Y != inter.X - inter.Y 
                         && !_isChangeZone(k, inter) //!map.isChangeZone(MapTools.getCellIdByCoord(kX, kY), MapTools.getCellIdByCoord(interX, interY)) 
                         && !_isChangeZone(inter, next))//!map.isChangeZone(MapTools.getCellIdByCoord(interX, interY), MapTools.getCellIdByCoord(nextX, nextY)))
                        {
                            _parentOfCell[cursor] = grandParent.CellId;
                        }
                        else if (k.X - k.Y == next.X - next.Y 
                              && k.X - k.Y != inter.X - inter.Y 
                              && !_isChangeZone(k, inter)//!map.isChangeZone(MapTools.getCellIdByCoord(kX, kY), MapTools.getCellIdByCoord(interX, interY))
                              && !_isChangeZone(inter, next))//!map.isChangeZone(MapTools.getCellIdByCoord(interX, interY), MapTools.getCellIdByCoord(nextX, nextY)))
                        {
                            _parentOfCell[cursor] = grandParent.CellId;
                        }
                        else if (k.X == next.X 
                              && k.X != inter.X
                              && _pointWeight(new MapPoint(k.X, inter.Y)) < 2 && _pointMov(new MapPoint(k.X, inter.Y), _allowTroughEntity, cursor, path.CellEnd.CellId, true))// kX, interY, bAllowTroughEntity, cursor, endCellId))
                        {
                            _parentOfCell[cursor] = new MapPoint(k.X, inter.Y).CellId;//MapTools.getCellIdByCoord(kX, interY);
                        }
                        else if (k.Y == next.Y 
                              && k.Y != inter.Y
                              && _pointWeight(new MapPoint(inter.X, k.Y)) < 2 && _pointMov(new MapPoint(inter.X, k.Y), _allowTroughEntity, cursor, path.CellEnd.CellId, true))//interX, kY, bAllowTroughEntity, cursor, endCellId))
                        {
                            _parentOfCell[cursor] = new MapPoint(inter.X, k.Y).CellId;//MapTools.getCellIdByCoord(interX, kY);
                        }
                    }
                }
                MapPoint final = new MapPoint(_parentOfCell[cursor]);
                path.Cells.Add(new PathElement() { Cell = final, Orientation = final.OrientationTo(new MapPoint(cursor))});
            }

            path.Cells.Reverse();

            return path;
        }

        // to do
        private bool _pointMov(MapPoint point, bool troughEntity, int parentId, int endCellId, bool avoidObstacle)
        {
            bool useNewSystem;
            int cellId;
            CellData cell;
            bool mov;
            CellData parent;
            int dif;

            if (point.IsInMap())
            {
                useNewSystem = Map.IsUsingNewMovementSystem;
                cellId = point.CellId;
                cell = Map.Cells[cellId];
                mov = cell.mov && (!_isInFight || !cell.nonWalkableDuringFight);

                // check updatedCells
                if(mov && useNewSystem && parentId != -1 && parentId != cellId)
                {
                    parent = Map.Cells[parentId];
                    dif = Math.Abs(Math.Abs(cell.floor) - Math.Abs(parent.floor));
                    if (parent.moveZone != cell.moveZone && dif > 0 || parent.moveZone == cell.moveZone && cell.moveZone == 0 && dif > 11)
                    {
                        mov = false;
                    }
                }

                if (!troughEntity)
                {
                    if (_isEntityOnCell[cellId] && mov && endCellId != cellId && parentId != -1 && parentId != cellId)
                    {
                        int dir = new MapPoint(parentId).OrientationTo(point);
                                                                           
                        /*int dir = new MapPoint(parentId).OrientationTo(point);
                        if(dir % 2 == 0) // diag
                        {
                            // +1/+3 - +5/+7
                            MapPoint l1 = point.GetNearestCellInDirection((dir + 1) % 8);
                            MapPoint l2 = point.GetNearestCellInDirection((dir + 3) % 8);

                            MapPoint r1 = point.GetNearestCellInDirection((dir + 5) % 8);
                            MapPoint r2 = point.GetNearestCellInDirection((dir + 7) % 8);

                            if (_pointMov(l1, true, parentId, endCellId, avoidObstacle) && !_isEntityOnCell[l1.CellId] &&
                               _pointMov(l2, true, parentId, endCellId, avoidObstacle) && !_isEntityOnCell[l2.CellId])
                                mov = false;

                            if (mov && _pointMov(r1, true, parentId, endCellId, avoidObstacle) && !_isEntityOnCell[r1.CellId] &&
                               _pointMov(r2, true, parentId, endCellId, avoidObstacle) && !_isEntityOnCell[r2.CellId])
                                mov = false;
                        }
                        else
                        {
                            MapPoint l = point.GetNearestCellInDirection((dir + 2) % 8);
                            MapPoint r = point.GetNearestCellInDirection((dir + 6) % 8);

                            if ((_pointMov(l, true, parentId, endCellId, avoidObstacle) && !_isEntityOnCell[l.CellId]) ||
                               (_pointMov(r, true, parentId, endCellId, avoidObstacle) && !_isEntityOnCell[r.CellId]))
                                mov = false;
                        }*/
                    }
                    // to do
                    /*if (MapData.Actors.ContainsKey(cellId) 
                     && cellId != endCellId
                     && )
                    {

                    }*/
                }
            }
            else
            {
                mov = false;
            }

            return mov;
        }

        private double _pointWeight(MapPoint point, bool troughEntity = true)
        {
            int speed = Map.Cells[point.CellId].speed;
            double weight = 1;
            int cellId = point.CellId;

            if (troughEntity)
            {
                if(speed >= 0)
                {
                    weight += 5 - speed;
                }
                else
                {
                    weight += 11 + Math.Abs(speed);
                }

                if (MapData.Actors.ContainsKey(point.CellId))
                {
                    weight = 20;
                }
            }
            else
            {
                if (_isEntityOnCell[cellId])//MapData.Actors.ContainsKey(cellId))//if (!MapData.NoEntitiesOnCell(cellId))
                    weight += 0.3;

                MapPoint adjCell = new MapPoint(point.X + 1, point.Y);
                if (adjCell != null)// && !MapData.NoEntitiesOnCell(adjCell.CellId))
                    if (_isEntityOnCell[adjCell.CellId])//MapData.Actors.ContainsKey(adjCell.CellId))
                        weight += 0.3;

                adjCell = new MapPoint(point.X, point.Y + 1);
                if (adjCell != null)// && !MapData.NoEntitiesOnCell(adjCell.CellId))
                    if (_isEntityOnCell[adjCell.CellId])//MapData.Actors.ContainsKey(adjCell.CellId))
                        weight += 0.3;

                adjCell = new MapPoint(point.X - 1, point.Y);
                if (adjCell != null)// && !MapData.NoEntitiesOnCell(adjCell.CellId))
                    if (_isEntityOnCell[adjCell.CellId]) //MapData.Actors.ContainsKey(adjCell.CellId))
                        weight += 0.3;

                adjCell = new MapPoint(point.X, point.Y - 1);
                if (adjCell != null)// && !MapData.NoEntitiesOnCell(adjCell.CellId))
                    if (_isEntityOnCell[adjCell.CellId]) //MapData.Actors.ContainsKey(adjCell.CellId))
                        weight += 0.3;

                // special effect
            }

            return weight;
        }

        private bool _isChangeZone(MapPoint fCell, MapPoint sCell)
        {
            CellData f = Map.Cells[fCell.CellId];
            CellData s = Map.Cells[sCell.CellId];

            return f.moveZone != s.moveZone &&
                  Math.Abs(f.floor) == Math.Abs(s.floor);
        }
    }
}
