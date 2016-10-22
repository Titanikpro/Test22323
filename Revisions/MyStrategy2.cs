using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {
        const double speedControl = 9;
        const int ZalipStep = 50;
        double lastX = 0;
        int promTileX = 0;
        int promTileY = 0;
        double lastY = 0;
        int lastTileX = 0;
        int lastTileY = 0;
        int TileUpdateTick = 0;
        int PositionUpdateTick = 0;
        int ZalipAngle = 0;
        int ZalipCount = 0;
        int dolyaX = 2;
        int dolyaY = 2;


        public void Move(Car self, World world, Game game, Move move)
        {

            if (ZalipCount > 0)
            {
                if (ZalipCount < ZalipStep / 4)
                {
                    move.EnginePower = 0.5;
                    move.WheelTurn = ZalipAngle;
                    move.IsBrake = true;
                }
                /*else if (ZalipCount > (ZalipStep * 3 / 4))
                {
                    move.EnginePower = 0;
                    move.WheelTurn = ZalipAngle;
                    move.IsBrake = true;
                }*/
                else
                {
                    move.EnginePower = -1.0;
                    move.WheelTurn = ZalipAngle;
                    move.IsBrake = false;
                }
                ZalipCount--;
            } else
            {
                move.EnginePower = .9D;
                dolyaX = 2;
                dolyaY = 2;

                int myNextTileX = self.NextWaypointX;
                int myNextTileY = self.NextWaypointY;
                int myTileX = Convert.ToInt32(Math.Floor(self.X / game.TrackTileSize));
                int myTileY = Convert.ToInt32(Math.Floor(self.Y / game.TrackTileSize));

                if (world.Tick > game.InitialFreezeDurationTicks)
                {
                    PositionUpdateTick++;
                    TileUpdateTick++;
                } else
                {
                    lastTileX = myTileX;
                    lastTileY = myTileY;
                    promTileX = myTileX;
                    promTileY = myTileY;
                }
                if (promTileX != myTileX || promTileY != myTileY)
                {
                    lastTileX = promTileX;
                    lastTileY = promTileY;
                    promTileY = myTileY;
                    promTileX = myTileX;
                    TileUpdateTick = 0;
                }

                if (myTileX == 0 && myTileY == 0)
                { /*Console.WriteLine();*/ }

                int[] TraceResult = TraceAnalis(myTileX, myTileY, self.NextWaypointIndex, world);
                myNextTileX = TraceResult[0];
                myNextTileY = TraceResult[1];
                dolyaX = TraceResult[2];
                dolyaY = TraceResult[3];

                if (world.MapName != "map12" && world.MapName != "map13" && world.MapName != "map14" && world.MapName != "_fdoke" && world.MapName != "_tyamgin")
                {
                    double angleToWaypoin = self.GetAngleTo(self.NextWaypointX * game.TrackTileSize + game.TrackTileSize / 2, self.NextWaypointY * game.TrackTileSize + game.TrackTileSize / 2);
                    double angleToNextTile = self.GetAngleTo(myNextTileX * game.TrackTileSize + game.TrackTileSize * dolyaX / 4, myNextTileY * game.TrackTileSize + game.TrackTileSize * dolyaY / 4);
                    if (Math.Abs(angleToWaypoin - angleToNextTile) < Math.PI / 10)
                    {
                        myNextTileX = self.NextWaypointX;
                        myNextTileY = self.NextWaypointY;
                        dolyaX = 2;
                        dolyaY = 2;
                    }
                }

                int myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize * dolyaX / 4);
                int myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize * dolyaY / 4);

                double myDist = self.GetDistanceTo(myNextX, myNextY);
                double myAngle = self.GetAngleTo(myNextX, myNextY);

                //притормаживание
                /*if (Math.Abs(myNextTileX - myTileX) > 0 && Math.Abs(myNextTileY - myTileY) > 0)
                {
                    if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY + self.SpeedY) > speedControl)
                    { move.IsBrake = true; }
                } else*/
                {
                    if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY + self.SpeedY) > speedControl)
                    { move.IsBrake = true; }
                    if (Math.Abs(myAngle) < Math.PI / 12 /*|| Math.Abs(self.WheelTurn) == 1*/)
                    { move.IsBrake = false; }
                }

                /*if (world.TilesXY[myNextTileX][myNextTileY] != TileType.Unknown && world.TilesXY[myNextTileX][myNextTileY] != TileType.Empty &&
                    world.TilesXY[myNextTileX][myNextTileY] != TileType.Horizontal && world.TilesXY[myNextTileX][myNextTileY] != TileType.Vertical)
                {
                    
                }*/

                //игра началась и съехали с первой клетки
                if (/*world.MapName!="map06" && world.MapName != "map09" && */world.Tick > game.InitialFreezeDurationTicks && (lastTileX != myTileX || lastTileY != myTileY))
                {

                    //ускорение
                    if (world.TilesXY[myTileX][myTileY] == TileType.Horizontal || world.TilesXY[myTileX][myTileY] == TileType.Vertical || world.TilesXY[myTileX][myTileY] == TileType.Crossroads)
                    {
                        try
                        {
                            int dx = lastTileX - myTileX;
                            int dy = lastTileY - myTileY;
                            if (world.TilesXY[myTileX - dx][myTileY - dy] == TileType.Horizontal || world.TilesXY[myTileX - dx][myTileY - dy] == TileType.Vertical || world.TilesXY[myTileX - dx][myTileY - dy] == TileType.Crossroads)
                            {
                                if (world.TilesXY[myTileX - 2 * dx][myTileY - 2 * dy] == TileType.Horizontal || world.TilesXY[myTileX - 2 * dx][myTileY - 2 * dy] == TileType.Vertical || world.TilesXY[myTileX - 2 * dx][myTileY - 2 * dy] == TileType.Crossroads)
                                {
                                    if (world.TilesXY[myTileX - 3 * dx][myTileY - 3 * dy] == TileType.Horizontal || world.TilesXY[myTileX - 3 * dx][myTileY - 3 * dy] == TileType.Vertical || world.TilesXY[myTileX - 3 * dx][myTileY - 3 * dy] == TileType.Crossroads)
                                    {
                                        move.EnginePower = 1.0D;
                                        move.IsBrake = false;
                                        if (world.TilesXY[myTileX - 4 * dx][myTileY - 4 * dy] == TileType.Horizontal || world.TilesXY[myTileX - 4 * dx][myTileY - 4 * dy] == TileType.Vertical || world.TilesXY[myTileX - 4 * dx][myTileY - 4 * dy] == TileType.Crossroads)
                                        {
                                            if (world.TilesXY[myTileX - 5 * dx][myTileY - 5 * dy] == TileType.Horizontal || world.TilesXY[myTileX - 5 * dx][myTileY - 5 * dy] == TileType.Vertical || world.TilesXY[myTileX - 5 * dx][myTileY - 5 * dy] == TileType.Crossroads)
                                            {
                                                //шесть прямых клеток подряд
                                                if (self.GetAngleTo(((myTileX - 5 * dx) * game.TrackTileSize + game.TrackTileSize / 2), ((myTileY - 5 * dy) * game.TrackTileSize + game.TrackTileSize / 2)) < Math.PI / 10)
                                                {
                                                    move.IsUseNitro = true;
                                                    //move.IsBrake = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine("Nitro ERROR");
                        }

                    }

                    //сброс масла
                    if (world.TilesXY[lastTileX][lastTileY] == TileType.LeftBottomCorner || world.TilesXY[lastTileX][lastTileY] == TileType.LeftTopCorner ||
                        world.TilesXY[lastTileX][lastTileY] == TileType.RightBottomCorner || world.TilesXY[lastTileX][lastTileY] == TileType.RightTopCorner ||
                        world.TilesXY[lastTileX][lastTileY] == TileType.Crossroads)
                    {
                        if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY * self.SpeedY) > 5) { move.IsSpillOil = true; }
                    }

                    //стрельба по врагам
                    foreach (Car vrag in world.Cars)
                    {
                        if (self.Id != vrag.Id)
                        {
                            if (self.GetAngleTo(vrag) < Math.PI / 30 && self.GetDistanceTo(vrag) < game.TrackTileSize * 1.5)
                            {
                                if (Math.Sign(Math.Cos(self.Angle) / (vrag.X - self.X)) > 0 && Math.Sign(Math.Sin(self.Angle) / (vrag.Y - self.Y)) > 0)
                                {
                                    move.IsThrowProjectile = true;
                                }
                            }
                        }
                    }

                    //подруливание на бонусы
                    if (world.MapName == "default" || world.MapName == "map01" || world.MapName == "map02" || world.MapName == "map03" || world.MapName == "map04" ||
                        world.MapName == "map05" || world.MapName == "map06" || world.MapName == "map07" || world.MapName == "map08" || world.MapName == "map09" ||
                        world.MapName == "map10" || world.MapName == "map11" || world.MapName == "map15" || world.MapName == "_fdoke")
                    {
                        foreach (Unit bon in world.Bonuses)
                        {
                            int bonusTileX = Convert.ToInt32(Math.Floor(bon.X / game.TrackTileSize));
                            int bonusTileY = Convert.ToInt32(Math.Floor(bon.Y / game.TrackTileSize));
                            for (int i = 2; i < TraceResult.Length / 2 - 1; i++)
                            {
                                if (TraceResult[i * 2] == bonusTileX && TraceResult[i * 2 + 1] == bonusTileY)
                                {
                                    double angleToBonus = self.GetAngleTo(bon);
                                    double distToBonus = self.GetDistanceTo(bon);
                                    if (Math.Abs(angleToBonus - myAngle) < Math.PI / 12 && distToBonus < myDist)
                                    {
                                        myDist = distToBonus;
                                        myAngle = self.GetAngleTo(bon);
                                    }
                                }
                            }
                        }
                    }
                }
                move.WheelTurn = AngleRegul(myAngle);

                if (PositionUpdateTick > 50)
                {
                    //защита от залипания на одном месте
                    ZalipCount = ZalipStep;
                    ZalipAngle = -Math.Sign(AngleRegul(myAngle));
                }

                if (self.Durability < 0.1) //если мало HP
                {
                    //использовать ремкомплект
                }


                if (Math.Abs(lastX - self.X) > 10 || Math.Abs(lastY - self.Y) > 10)
                {
                    lastY = self.Y;
                    lastX = self.X;
                    PositionUpdateTick = 0;
                }
            }
        }

        public int[] CrossroadAnalis(int TileX, int TileY, TileType tileType, int WPX, int WPY, double carAngle)
        {
            if (TileX == WPX && TileY == WPY)
            {
                return new int[] { TileX, TileY, 2, 2 };
            } else
            {
                int[,] points = new int[2, 4];
                //составим массив соседних точек
                points[0, 0] = TileX + 1;
                points[1, 0] = TileY;
                points[0, 1] = TileX;
                points[1, 1] = TileY + 1;
                points[0, 2] = TileX - 1;
                points[1, 2] = TileY;
                points[0, 3] = TileX;
                points[1, 3] = TileY - 1;

                //удалим лишнюю точку
                if (tileType == TileType.LeftHeadedT)
                {
                    points[0, 0] = 255;
                    points[1, 0] = 255;
                }
                if (tileType == TileType.TopHeadedT)
                {
                    points[0, 1] = 255;
                    points[1, 1] = 255;
                }
                if (tileType == TileType.RightHeadedT)
                {
                    points[0, 2] = 255;
                    points[1, 2] = 255;
                }
                if (tileType == TileType.BottomHeadedT)
                {
                    points[0, 3] = 255;
                    points[1, 3] = 255;
                }

                if (carAngle < 0)
                { carAngle = Math.PI * 2.0 + carAngle; }
                //точка на прямом движении от нас
                int priorAngle = Convert.ToInt16(Math.Floor((carAngle + 0.25 * Math.PI) / (0.5 * Math.PI)));
                if (priorAngle > 3) { priorAngle = 0; }
                //и точка противоположная
                int negatAngle = priorAngle + 2;
                if (negatAngle > 3) { negatAngle -= 4; }
                points[0, negatAngle] = 255;
                points[1, negatAngle] = 255;
                //ставим ее на первое место в массиве
                //а на место priorPoint ставим точку с первого места массива
                int prom = points[0, 0];
                points[0, 0] = points[0, priorAngle];
                points[0, priorAngle] = prom;
                prom = points[1, 0];
                points[1, 0] = points[1, priorAngle];
                points[1, priorAngle] = prom;
                //выберем в массиве точку с наименьшим расстояние до вейпоинта
                for (int i = 1; i < 4; i++)
                {
                    if (points[0, i] > -1 && points[1, i] > -1 && points[0, i] < 15 && points[1, i] < 15)
                    {
                        int dX = Math.Abs(WPX - points[0, 0]);
                        int dY = Math.Abs(WPY - points[1, 0]);
                        int dx = Math.Abs(WPX - points[0, i]);
                        int dy = Math.Abs(WPY - points[1, i]);
                        if ((dx + dy) < (dX + dY) || points[0, 0] > 15 || points[1, 0] > 15 || points[0, 0] < 0 || points[1, 0] < 0)
                        {
                            points[0, 0] = points[0, i];
                            points[1, 0] = points[1, i];
                        }
                    }
                }
                int dolyaX = 2;
                int dolyaY = 2;
                switch (priorAngle)
                {
                    case 0:
                        dolyaX = 3;
                        dolyaY = dolyaY - Math.Sign(points[1, 0] - TileY);
                        break;
                    case 1:
                        dolyaX = dolyaX - Math.Sign(points[0, 0] - TileX);
                        dolyaY = 3;
                        break;
                    case 2:
                        dolyaX = 1;
                        dolyaY = dolyaY - Math.Sign(points[1, 0] - TileY);
                        break;
                    case 3:
                        dolyaX = dolyaX - Math.Sign(points[0, 0] - TileX);
                        dolyaY = 1;
                        break;
                    default:
                        break;
                }
                return new int[] { points[0, 0], points[1, 0], dolyaX, dolyaY };
            }
        }

        double erroldAngle = 0;
        public double AngleRegul(double ActualAngle)
        {
            double kp = 2.5;
            double kd = 0.0;
            double ki = 0;
            double U = 0.0;
            try
            {
                double err = ActualAngle;
                double P = err * kp;
                double D = kd * (err - erroldAngle);
                double I = 0;
                U = P + D + I;
                erroldAngle = err;
                return U;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int[] TraceAnalis(int TileX, int TileY, int WPI, World world)
        {
            int width = world.Width;
            int height = world.Height;
            GraphPoint[,] myGraph = new GraphPoint[width, height];
            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Height; j++)
                {
                    myGraph[i, j] = new GraphPoint();
                    myGraph[i, j].isLooking = false;
                    myGraph[i, j].TileX = i;
                    myGraph[i, j].TileY = j;
                    myGraph[i, j].dolyaX = 2;
                    myGraph[i, j].dolyaY = 2;
                }
            }
            GraphPoint startPoint = new GraphPoint();
            startPoint.TileX = TileX;
            startPoint.TileY = TileY;
            startPoint.parrentPoint = new int[] { TileX, TileY };
            myGraph[TileX, TileY].isLooking = true;
            myGraph[TileX, TileY].rasst = 0;
            List<GraphPoint> soseds = new List<GraphPoint>();
            soseds.Add(startPoint);
            int count = width * height * 4;
            bool stopFlag = false;
            //ищем оптимальный маршрут от стартовой точки до вейпоинта
            while (!stopFlag)
            {

                soseds = GetSosedPoints(soseds, myGraph, world);
                foreach (GraphPoint pnt in soseds)
                {
                    if (pnt.TileX == world.Waypoints[WPI][0] && pnt.TileY == world.Waypoints[WPI][1])
                    { stopFlag = true; }
                }
                if (count < 0)
                {
                    stopFlag = true;
                }
                count--;
            }

            int nxtTileX = world.Waypoints[WPI][0];
            int nxtTileY = world.Waypoints[WPI][1];
            byte porog = 2;
            //если уже вплотную подъехали к вейпоинту
            if (myGraph[nxtTileX, nxtTileY].rasst == 1)
            {
                //porog = 2;
                //очищаем граф от прошлых проходов
                for (int i = 0; i < world.Width; i++)
                {
                    for (int j = 0; j < world.Height; j++)
                    {
                        if ((i == TileX && j == TileY) || (i == nxtTileX && j == nxtTileY))
                        {
                            //Console.WriteLine();
                        } else
                        {
                            myGraph[i, j].isLooking = false;
                            myGraph[i, j].dolyaX = 2;
                            myGraph[i, j].dolyaY = 2;
                            myGraph[i, j].rasst = width * height * 2;
                        }
                    }
                }
                //берем след вейпоинт
                int nxtWPI = WPI + 1;
                if (nxtWPI > world.Waypoints.Length - 1)
                { nxtWPI = 0; }
                //и считаем путь до него
                //очищаем коллекцию соседних точек
                soseds.Clear();
                //считать начинаем с текущего вейпонита
                startPoint = new GraphPoint();
                startPoint.TileX = nxtTileX;
                startPoint.TileY = nxtTileY;
                startPoint.parrentPoint = myGraph[nxtTileX, nxtTileY].parrentPoint;
                soseds.Add(startPoint);
                stopFlag = false;
                while (!stopFlag)
                {
                    soseds = GetSosedPoints(soseds, myGraph, world);
                    foreach (GraphPoint pnt in soseds)
                    {
                        if (pnt.TileX == world.Waypoints[nxtWPI][0] && pnt.TileY == world.Waypoints[nxtWPI][1])
                        { stopFlag = true; }
                    }
                    if (count < 0)
                    {
                        stopFlag = true;
                    }
                    count--;
                }

                //а это зачем?!
                nxtTileX = world.Waypoints[nxtWPI][0];
                nxtTileY = world.Waypoints[nxtWPI][1];
            }
            int[] returnMass = new int[(Convert.ToInt32(myGraph[nxtTileX, nxtTileY].rasst)+1) * 2 + 4];
            //try {
                stopFlag = false;
                while (!stopFlag)
                {
                    if (myGraph[nxtTileX, nxtTileY].rasst > porog)
                    {
                        int promX = nxtTileX;
                        int promY = nxtTileY;
                        nxtTileX = myGraph[promX, promY].parrentPoint[0];
                        nxtTileY = myGraph[promX, promY].parrentPoint[1];
                        returnMass[4 + (Convert.ToInt32(myGraph[nxtTileX, nxtTileY].rasst) + 1) * 2] = promX;
                        returnMass[5 + (Convert.ToInt32(myGraph[nxtTileX, nxtTileY].rasst) + 1) * 2] = promY;
                    } else { stopFlag = true; }
                    if (count < 0)
                    {
                        stopFlag = true;
                    }
                    count--;
                }
                if (returnMass.Length < 3) { returnMass = new int[4]; }
                returnMass[0] = nxtTileX;
                returnMass[1] = nxtTileY;
                returnMass[2] = myGraph[nxtTileX, nxtTileY].dolyaX;
                returnMass[3] = myGraph[nxtTileX, nxtTileY].dolyaY;
                stopFlag = false;
                while (!stopFlag)
                {
                    /*if (myGraph[nxtTileX, nxtTileY].rasst == 0)
                    {
                        stopFlag = true;
                    }*/
                    if (count < 0)
                    {
                        stopFlag = true;
                    }
                    int promX = nxtTileX;
                    int promY = nxtTileY;
                    nxtTileX = myGraph[promX, promY].parrentPoint[0];
                    nxtTileY = myGraph[promX, promY].parrentPoint[1];
                    returnMass[4 + (Convert.ToInt32(myGraph[nxtTileX, nxtTileY].rasst) + 1) * 2] = promX;
                    returnMass[5 + (Convert.ToInt32(myGraph[nxtTileX, nxtTileY].rasst) + 1) * 2] = promY;
                    count--;
                    if (myGraph[nxtTileX, nxtTileY].rasst == 0)
                    {
                        stopFlag = true;
                        returnMass[4] = nxtTileX;
                        returnMass[5] = nxtTileY;
                    }
                }
            //}
            //catch(Exception e) { }
            return returnMass;
        }

        public List<GraphPoint> GetSosedPoints(List<GraphPoint> startPoint, GraphPoint[,] graph, World world)
        {
            List<GraphPoint> mySoseds = new List<GraphPoint>();
            #region
            foreach (GraphPoint myPoint in startPoint)
            {
                switch (world.TilesXY[myPoint.TileX][myPoint.TileY])
                {
                    case TileType.Vertical:
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                        }
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                        }
                        break;
                    case TileType.Horizontal:
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                        }
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                        }
                        break;
                    case TileType.LeftTopCorner:
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                            graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                            graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 1;
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 1;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                        }
                        break;
                    case TileType.LeftBottomCorner:
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                            graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                            graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 3;
                        }
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 1;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                        }
                        break;
                    case TileType.RightTopCorner:
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 3;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                        }
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                            graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                            graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 1;
                        }
                        break;
                    case TileType.RightBottomCorner:
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                            graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                            graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 3;
                        }
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 3;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                        }
                        break;
                    case TileType.TopHeadedT:
                        //если входим справа
                        if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                        {
                            //проверяем сразу левый выход
                            if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                            {
                                graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                                graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                                graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                                mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                                graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 2;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 2;
                            }
                        }
                        //если входим слева
                        if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                        {
                            //проверяем сразу правый выход
                            if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                            {
                                graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                                graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                                graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                                mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                                graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 2;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 2;
                            }
                        }

                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                            {
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 3;
                            }
                        }
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                            {
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 1;
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                            }
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                            {
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 3;
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                            }
                        }
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                            {
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 3;
                            }
                        }
                        break;
                    case TileType.BottomHeadedT:
                        //если входим слева
                        if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                        {
                            //проверяем сразу правый выход
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                            graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 2;
                            graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 2;
                        }
                        //если входим справа
                        if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                        {
                            //проверяем сразу левый выход
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                            graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 2;
                            graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 2;
                        }
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                            {
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 1;
                            }
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                            {
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 1;
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                            }
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                            {
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 3;
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                            }
                        }
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                            {
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 1;
                            }
                        }
                        break;
                    case TileType.LeftHeadedT:
                        //входим сверху
                        if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                        {
                            //проверяем сразу выход снизу
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 2;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 2;
                        }
                        //входим снизу
                        if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                        {
                            //проверяем сразу выход сверху
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 2;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 2;
                        }
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                            {
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 3;
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                            }
                        }
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                            {
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 3;
                            }
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                            {
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 1;
                            }
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                            {
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 3;
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                            }
                        }
                        break;
                    case TileType.RightHeadedT:
                        //входим сверху
                        if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                        {
                            //проверяем сразу выход снизу
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 2;
                            graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 2;
                        }
                        //входим снизу
                        if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                        {
                            //проверяем сразу выход сверху
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 2;
                            graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 2;
                        }
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                            {
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 1;
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                            }
                        }
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                            {
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 3;
                            }
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                            {
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 1;
                            }
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                            {
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 1;
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                            }
                        }
                        break;
                    case TileType.Crossroads:
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                            {
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 1;
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                            }
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                            {
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaX = 3;
                                graph[myPoint.TileX, myPoint.TileY - 1].dolyaY = 3;
                            }
                        }
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                            {
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 3;
                            }
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                            {
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaX = 1;
                                graph[myPoint.TileX + 1, myPoint.TileY].dolyaY = 1;
                            }
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == 1)
                            {
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 1;
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                            }
                            if ((myPoint.parrentPoint[0] - myPoint.TileX) == -1)
                            {
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaX = 3;
                                graph[myPoint.TileX, myPoint.TileY + 1].dolyaY = 1;
                            }
                        }
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == -1)
                            {
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 3;
                            }
                            if ((myPoint.parrentPoint[1] - myPoint.TileY) == 1)
                            {
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaX = 3;
                                graph[myPoint.TileX - 1, myPoint.TileY].dolyaY = 1;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            #endregion
            return mySoseds;
        }

        public class GraphPoint
        {
            public int TileX;
            public int TileY;
            public int dolyaX = 2;
            public int dolyaY = 2;
            public double X;
            public double Y;
            public double rasst=-1;
            public bool isLooking = false;
            public int[] parrentPoint = new int[2];
        }
    }
}