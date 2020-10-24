using AivyData.Enums;
using AivyDofus.DofusMap.Map;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AivyDofus.Pathfinding
{
    public class DofusPathCell
    {
        public short CellId { get; set; }
        public Point Position => PathingUtils.CellIdToCoord(CellId);

        public bool IsPair => (CellId / 14) % 2 == 0;

        public DofusPathCell EAST => new DofusPathCell() { CellId = (short)(CellId + 1) };
        public DofusPathCell SOUTH => new DofusPathCell() { CellId = (short)(CellId + 28) };
        public DofusPathCell WEST => new DofusPathCell() { CellId = (short)(CellId - 1) };
        public DofusPathCell NORTH => new DofusPathCell() { CellId = (short)(CellId - 28) };

        public DofusPathCell NORTH_EAST => IsPair ? new DofusPathCell() { CellId = (short)(CellId - 14) } : new DofusPathCell() { CellId = (short)(CellId - 13) };
        public DofusPathCell SOUTH_WEST => IsPair ? new DofusPathCell() { CellId = (short)(CellId + 13) } : new DofusPathCell() { CellId = (short)(CellId + 14) };

        public Dictionary<DirectionsEnum, DofusPathCell> Adjacents
        {
            get
            {
                Dictionary<DirectionsEnum, DofusPathCell> result = new Dictionary<DirectionsEnum, DofusPathCell>()
                {
                    [DirectionsEnum.DIRECTION_EAST] = EAST,
                    [DirectionsEnum.DIRECTION_SOUTH] = SOUTH,
                    [DirectionsEnum.DIRECTION_WEST] = WEST,
                    [DirectionsEnum.DIRECTION_NORTH] = NORTH,
                    [DirectionsEnum.DIRECTION_NORTH_EAST] = NORTH_EAST,
                    [DirectionsEnum.DIRECTION_SOUTH_WEST] = SOUTH_WEST,
                    [DirectionsEnum.DIRECTION_NORTH_WEST] = WEST.NORTH_EAST,
                    [DirectionsEnum.DIRECTION_SOUTH_EAST] = EAST.SOUTH_WEST
                };

                return result;
            }
        }
        public Dictionary<DirectionsEnum, DofusPathCell> ValidAdjacents => Adjacents.Where(x => x.Value.IsValidCell).ToDictionary(k => k.Key, v => v.Value);
        public long[] AdjacentsIds => Adjacents.Select(x => (long)x.Value.CellId).ToArray();
        public bool IsValidCell => CellId >= 0 && CellId < 560 && Position.X >= 0 && Position.Y >= 0 && Position.X < 14 && Position.Y < 20;
    }

    public class DofusPathCellList : List<DofusPathCell>
    {
        public DofusPathCell First { get; set; }
        public DofusPathCell Last { get; set; }
    }

    public class DofusPathfinder
    {
        static readonly double HEURISTIC_WEIGHT = 10.0d;
        static readonly double DIAG_WEIGHT = 15.0d;

        static readonly double INFINITE_COST = 999999999.0d;

        public DofusPathfinder()
        {

        }

        public DofusPathCellList FindPath(GameMapInformations mapInformations, short start_cell, short end_cell)
        {
            Map map = mapInformations.CurrentMap;
            IEnumerable<int> occupied = mapInformations.Actors.Select(x => x.Key);

            FastInArray _openList = new FastInArray(40);
            bool[] _closed = new bool[map.CellsCount];
            double[] _weight = new double[map.CellsCount];

            _openList.clear();
            for(int i = 0; i < map.CellsCount;i++)
            {
                _closed[i] = false;
                _weight[i] = INFINITE_COST;
            }
            
            _closed[start_cell] = true;
            _weight[start_cell] = 1;
            _openList.push(start_cell);

            DofusPathCellList result = new DofusPathCellList()
            {
                First = new DofusPathCell() { CellId = start_cell },
                Last = new DofusPathCell() { CellId = end_cell }
            };

            DofusPathCell cursor = result.First;
            while(_openList.length > 0 && !_closed[end_cell])
            {
                foreach(long adj in cursor.AdjacentsIds)
                {
                    if (!_closed[start_cell])
                    {
                        _closed[adj] = true;
                        _openList.push((int)adj);
                    }
                }
            }

            return result;
        }

        public long[] Test(short start_cell, short end_cell)
        {
            return new DofusPathCell() { CellId = start_cell }.AdjacentsIds;
        }
    }

    public class FastInArray
    {
        public int[] data { get; set; }
        public int length { get; set; }

        public FastInArray(int size = 0)
        {
            data = new int[size];
            length = 0;
        }

        public int this[uint index]
        {
            get
            {
                return data[index];
            }
        }

        public void push(int i)
        {
            if(data.Count() == length)
            {
                data = (data as IEnumerable<int>).Append(i).ToArray();
            }
            else
            {
                data[length] = i;
            }
            length++;
        }

        public int removeAt(uint index)
        {
            int previous = this[index];
            data[index] = data[length - 1];
            length--;
            return previous;
        }

        public int indexOf(int val)
        {
            for (uint i = 0; i < length; i++)
            {
                if (this[i] == val) return (int)i;
            }
            return -1;
        }

        public void clear()
        {
            length = 0;
        }
    }

    /*public class DofusPathfinder
    {
        private readonly int HV_COST = 10;
        private readonly int DIAG_COST = 15;
        private readonly int HEURISTIC_COST = 10;
        private readonly int INFINITE_COST = 99999999;
        private bool _isInit { get; set; } = false;
        private int[] _parentOfCell { get; set; }
        private double[] _costOfCell { get; set; }
        private double[] _openListWeights { get; set; }
        private bool[] _isCellClosed { get; set; }
        private bool[] _isEntityOnCell { get; set; }
        private FastInArray _openList { get; set; }

        public DofusPathfinder()
        {

        }

        public CellList FindPath(GameMapInformations mapInfo, short start, short end, bool diag = true, bool througEntity = true, bool avoidObstacle = true)
        {
            Map map = mapInfo.CurrentMap;
            IEnumerable<int> actors = mapInfo.Actors.Select(actor => actor.Key);

            uint i = 0;
            double minimum;
            uint smallestCostIndex = 0;
            int parentId = 0;
            int parentX = 0;
            int parentY = 0;
            int y = 0;
            double cost;
            int x = 0;
            int cellId = 0;
            double pointWeight;
            int movementCost = 0;
            int speed = 0;
            bool cellOnEndColumn = false;
            bool cellOnStartColumn = false;
            bool cellOnEndLine = false;
            bool cellOnStartLine = false;
            uint distanceTmpToEnd = 0;
            double heuristic;
            int parent = 0;
            int grandParent = 0;
            int grandGrandParent = 0;
            int kX = 0;
            int kY = 0;
            int nextX = 0;
            int nextY = 0;
            int interX = 0;
            int interY = 0;
            int endCellId = start;
            int startCellId = end;
            int endX = 0;
            int endY = 0;
            int endCellAuxId = startCellId;
            uint distanceToEnd = PathingUtils.DistanceToPoint(PathingUtils.CellIdToCoord((short)startCellId),
                                                             PathingUtils.CellIdToCoord((short)endCellId));

            if (!_isInit)
            {
                _costOfCell = new double[map.CellsCount];
                _openListWeights = new double[map.CellsCount];
                _parentOfCell = new int[map.CellsCount];
                _isCellClosed = new bool[map.CellsCount];
                _isEntityOnCell = new bool[map.CellsCount];
                _openList = new FastInArray(40);
                _isInit = true;
            }

            for(i = 0; i < map.CellsCount; i++)
            {
                _parentOfCell[i] = -1;
                _isCellClosed[i] = false;
                _isEntityOnCell[i] = false;
            }

            _openList.clear();
            foreach(var info in mapInfo.Actors)
            {
                _isEntityOnCell[info.Key] = info.Value.Count > 0;
            }
            _costOfCell[startCellId] = 0;
            _openList.push(startCellId);

            while(_openList.length > 0 && _isCellClosed[endCellId] == false)
            {
                minimum = INFINITE_COST;
                smallestCostIndex = 0;
                for(i = 0; i < _openList.length; i++)
                {
                    cost = _openListWeights[_openList[i]];
                    if(cost <= minimum)
                    {
                        minimum = cost;
                        smallestCostIndex = i;
                    }
                }
                parentId = _openList[smallestCostIndex];
                var point = PathingUtils.CellIdToCoord((short)parentId);
                parentX = (int)point.X;
                parentY = (int)point.Y;
                _openList.removeAt(smallestCostIndex);
                _isCellClosed[parentId] = true;
                for(y = parentY - 1; y <= parentY + 1; y++)
                {
                    for(x = parentX - 1; x <= parentX + 1; x++)
                    {
                        cellId = PathingUtils.CoordToCellId(x, y);
                        bool not_invalid = cellId != -1;
                        bool not_closed = !_isCellClosed[cellId];
                        bool not_parentId = cellId != parentId;
                        bool is_free = pointMov(map, x, y, througEntity, parentId, endCellId, avoidObstacle, _isEntityOnCell);
                        bool is_free_x_y = y == parentY 
                                        || x == parentX
                                        || (diag && (pointMov(map, parentX, y, througEntity, parentId, endCellId, avoidObstacle, _isEntityOnCell) 
                                                  || pointMov(map, parentX, y, througEntity, parentId, endCellId, avoidObstacle, _isEntityOnCell)));
                        
                        if(not_invalid && not_closed && not_parentId && is_free && is_free_x_y)
                        {
                            pointWeight = 0;
                            if(cellId == endCellId)
                            {
                                pointWeight = 1;
                            }
                            else
                            {
                                speed = map.Cells[cellId].speed;
                                if (througEntity)
                                {
                                    if (_isEntityOnCell[cellId])
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
                                    if (_isEntityOnCell[cellId])
                                    {
                                        pointWeight = pointWeight + 0.3d;
                                    }
                                    if(isValidCoord(map, x + 1, y) && _isEntityOnCell[PathingUtils.CoordToCellId(x + 1, y)])
                                    {
                                        pointWeight = pointWeight + 0.3d;
                                    }
                                    if (isValidCoord(map, x, y + 1) && _isEntityOnCell[PathingUtils.CoordToCellId(x, y + 1)])
                                    {
                                        pointWeight = pointWeight + 0.3d;
                                    }
                                    if (isValidCoord(map, x - 1, y) && _isEntityOnCell[PathingUtils.CoordToCellId(x - 1, y)])
                                    {
                                        pointWeight = pointWeight + 0.3d;
                                    }
                                    if (isValidCoord(map, x, y - 1) && _isEntityOnCell[PathingUtils.CoordToCellId(x, y - 1)])
                                    {
                                        pointWeight = pointWeight + 0.3d;
                                    }
                                    if((pointSpecialEffects(map, x, y) & 2) == 2)
                                    {
                                        pointWeight = pointWeight + 0.2d;
                                    }
                                }
                            }

                            movementCost = (int)(_costOfCell[parentId] + (y == parentY || x == parentX ? HV_COST : DIAG_COST) * pointWeight);
                            if (througEntity)
                            {
                                cellOnEndColumn = x + y == endX + endY;
                                cellOnStartColumn = PathingUtils.CellIdToCoord((short)startCellId) is Point _c && x + y == _c.X + _c.Y;
                                cellOnEndLine = x - y == endX - endY;
                                cellOnStartLine = PathingUtils.CellIdToCoord((short)endCellId) is Point _c2 && x - y == _c2.X - _c2.Y;

                                if(!cellOnEndColumn && !cellOnEndLine || !cellOnStartColumn && !cellOnStartLine)
                                {
                                    movementCost = movementCost + (int)PathingUtils.DistanceToPoint(PathingUtils.CellIdToCoord((short)cellId), PathingUtils.CellIdToCoord((short)endCellId));
                                    movementCost = movementCost + (int)PathingUtils.DistanceToPoint(PathingUtils.CellIdToCoord((short)cellId), PathingUtils.CellIdToCoord((short)startCellId));
                                }

                                if(x == endX || y == endY)
                                {
                                    movementCost = movementCost - 3;
                                }
                                if(cellOnEndColumn || cellOnEndLine || x + y == parentX + parentY || x - y == parentX - parentY)
                                {
                                    movementCost = movementCost - 2;
                                }

                                if(PathingUtils.CellIdToCoord((short)startCellId) is Point _c3 && (i == _c3.X || y == _c3.Y))
                                {
                                    movementCost = movementCost - 3;
                                }

                                if(cellOnStartColumn || cellOnStartLine)
                                {
                                    movementCost = movementCost - 2;
                                }

                                distanceTmpToEnd = PathingUtils.DistanceToPoint(PathingUtils.CellIdToCoord((short)cellId), PathingUtils.CellIdToCoord((short)endCellId));
                                if(distanceTmpToEnd < distanceToEnd)
                                {
                                    endCellAuxId = cellId;
                                    distanceToEnd = distanceTmpToEnd;
                                }
                            }
                            if(_parentOfCell[cellId] == -1 || movementCost < _costOfCell[cellId])
                            {
                                _parentOfCell[cellId] = parentId;
                                _costOfCell[cellId] = movementCost;
                                heuristic = HEURISTIC_COST * Math.Sqrt((endY - y) * (endY - y) + (endX - x) * (endX - x));
                                _openListWeights[cellId] = heuristic + movementCost;
                                if(_openList.indexOf(cellId) == -1)
                                {
                                    _openList.push(cellId);
                                }
                            }
                        }
                    }
                }
            }

            CellList result = new CellList();

            CellData first = map.Cells[startCellId];
            result.First = new Cell(first, first.mapChangeData != 0, first.mov, first.los, 0, 0, startCellId, PathingUtils.CellIdToCoord((short)startCellId));

            if (_parentOfCell[endCellId] == -1)
            {
                endCellId = endCellAuxId;
                CellData last = map.Cells[endCellId];                
                result.Last = new Cell(last, map.ArrowsCells.ContainsKey(endCellId), last.mov, last.los, 0, 0, endCellId, PathingUtils.CellIdToCoord((short)endCellId));
            }
            else
            {
                CellData last = map.Cells[endCellId];
                result.Last = new Cell(last, last.mapChangeData != 0, last.mov, last.los, 0, 0, endCellId, PathingUtils.CellIdToCoord((short)endCellId));
                //result.Last = end;
            }

            for(int cursor = endCellId;cursor != startCellId; cursor = (int)_parentOfCell[cursor])
            {
                if (diag)
                {
                    parent = _parentOfCell[cursor];
                    grandParent = parent == -1 ? -1 : _parentOfCell[parent];
                    grandGrandParent = grandParent == -1 ? -1 : _parentOfCell[grandParent];
                    Point k = PathingUtils.CellIdToCoord((short)cursor);
                    kX = (int)k.X;
                    kY = (int)k.Y;
                    if (grandParent != -1 && PathingUtils.DistanceToPoint(PathingUtils.CellIdToCoord((short)cursor), PathingUtils.CellIdToCoord((short)grandParent)) == 1)
                    {
                        if (pointMov(map, kX, kY, througEntity, grandParent, endCellId, true, _isEntityOnCell))
                        {
                            _parentOfCell[cursor] = grandParent;
                        }
                    }
                    else if (grandGrandParent != -1 && PathingUtils.DistanceToPoint(PathingUtils.CellIdToCoord((short)cursor), PathingUtils.CellIdToCoord((short)grandGrandParent)) == 2)
                    {
                        Point next = PathingUtils.CellIdToCoord((short)grandGrandParent);
                        nextX = (int)next.X;
                        nextY = (int)next.Y;
                        interX = kX + (int)((nextX - kX) / 2);
                        interY = kY + (int)((nextY - kY) / 2);
                        if(pointMov(map, interX, interY, througEntity, cursor, endCellId, true, _isEntityOnCell) && 
                           fpointWeight(map, interX, interY, _isEntityOnCell) < 2)
                        {
                            _parentOfCell[cursor] = PathingUtils.CoordToCellId(interX, interY);
                        }
                    }
                    else if(grandParent != -1 && PathingUtils.DistanceToPoint(PathingUtils.CellIdToCoord((short)cursor), PathingUtils.CellIdToCoord((short)grandParent)) == 2)
                    {
                        Point next = PathingUtils.CellIdToCoord((short)grandParent);
                        Point inter = PathingUtils.CellIdToCoord((short)parent);
                        nextX = (int)next.X;
                        nextY = (int)next.Y;
                        interX = (int)inter.X;
                        interY = (int)inter.Y;
                        if(kX + kY == nextX + nextY && kX - kY != nextX - nextY
                        && !isChangeZone(map, PathingUtils.CoordToCellId(kX, kY), PathingUtils.CoordToCellId(interX, interY))
                        && !isChangeZone(map, PathingUtils.CoordToCellId(interX, interY), PathingUtils.CoordToCellId(nextX, nextY)))
                        {
                            _parentOfCell[cursor] = grandParent;
                        }
                        else if (kX - kY == nextX + nextY && kX - kY != nextX - nextY
                       && !isChangeZone(map, PathingUtils.CoordToCellId(kX, kY), PathingUtils.CoordToCellId(interX, interY))
                       && !isChangeZone(map, PathingUtils.CoordToCellId(interX, interY), PathingUtils.CoordToCellId(nextX, nextY)))
                        {
                            _parentOfCell[cursor] = grandParent;
                        }
                        else if (kX == nextX && kX != interX
                       && fpointWeight(map, kX, interY, _isEntityOnCell) < 2
                       && pointMov(map, kX, interY, througEntity, cursor, endCellId, true, _isEntityOnCell))
                        {
                            _parentOfCell[cursor] = PathingUtils.CoordToCellId(kX, interY);
                        }
                        else if (kY == nextY && kX != interX
                       && fpointWeight(map, interX, kY, _isEntityOnCell) < 2
                       && pointMov(map, interX, kY, througEntity, cursor, endCellId, true, _isEntityOnCell))
                        {
                            _parentOfCell[cursor] = PathingUtils.CoordToCellId(interX, kY);
                        }
                    }
                }
                CellData _c4 = map.Cells[cursor];
                result.Add(new Cell(_c4, false, _c4.mov, _c4.los, 0,0, cursor, PathingUtils.CellIdToCoord((short)cursor)));
            }
            result.Reverse();
            return result;
        }

        public static bool isChangeZone(Map map, int cell1, int cell2)
        {
            CellData _cell1 = map.Cells[cell1];
            CellData _cell2 = map.Cells[cell2];
            int dif = Math.Abs(Math.Abs(_cell1.floor) - Math.Abs(_cell2.floor));
            return _cell1.moveZone != _cell2.moveZone && dif == 0;
        }

        public static int pointSpecialEffects(Map map, int x,int y)
        {
            return 0;
        }

        public static bool isValidCoord(Map map, int x,int y)
        {
            return x >= 0 && y >= 0 && x <= 13 && y <= 19;
            ///return PathingUtils.CoordToCellId(x, y) is short cell && cell >= 0 && cell < 560;
        }

        public static bool pointMov(Map map, int x,int y, bool throughEntity, int parentId, int endCellId, bool avoidObstacle, bool[] occupiedCells)
        {
            bool useNewSystem = false;
            int cellId = 0;
            CellData cell;
            bool mov = false;
            CellData previous;
            int dif = 0;

            if(isValidCoord(map, x, y))
            {
                useNewSystem = map.IsUsingNewMovementSystem;
                cellId = PathingUtils.CoordToCellId(x, y);
                cell = map.Cells[cellId];
                mov = cell.mov; // check in fight
                if(mov && useNewSystem && parentId != -1 && parentId != cellId) // check updatedCell
                {
                    previous = map.Cells[parentId];
                    dif = Math.Abs(Math.Abs(cell.floor) - Math.Abs(previous.floor));
                    if(previous.moveZone != cell.moveZone && dif > 0 || previous.moveZone == cell.moveZone && cell.moveZone == 0 && dif > 11)
                    {
                        mov = false;
                    }
                }

                if (throughEntity)
                {
                    for(int i = 0; i < map.CellsCount; i++)
                    {
                        if (!map.Cells[i].mov) return false;
                    }
                }

                if(avoidObstacle && cellId != endCellId)
                {
                    return false;
                }
            }

            return mov;
        }

        public static double fpointWeight(Map map, int x,int y, bool[] occupiedCell, bool throughEntity = true)
        {
            double weight = 1.0d;

            short cellId = PathingUtils.CoordToCellId(x, y);
            int speed = map.Cells[cellId].speed;

            if (throughEntity)
            {
                if(speed >= 0)
                {
                    weight = weight + (5.0d - speed);
                }
                else
                {
                    weight = weight + (11.0d + Math.Abs(speed));
                }

                if (occupiedCell[cellId])
                {
                    weight = 20.0d;
                }
            }
            else
            {
                if (occupiedCell[cellId])
                {
                    weight = weight + 0.3d;
                }
                if (occupiedCell[PathingUtils.CoordToCellId(x + 1, y)])
                {
                    weight = weight + 0.3d;
                }
                if (occupiedCell[PathingUtils.CoordToCellId(x, y + 1)])
                {
                    weight = weight + 0.3d;
                }
                if (occupiedCell[PathingUtils.CoordToCellId(x - 1, y)])
                {
                    weight = weight + 0.3d;
                }
                if (occupiedCell[PathingUtils.CoordToCellId(x, y - 1)])
                {
                    weight = weight + 0.3d;
                }
                if (pointSpecialEffects(map, x, y) == 2)
                {
                    weight = weight + 0.2d;
                }
            }

            return weight;
        }
    }*/
}
