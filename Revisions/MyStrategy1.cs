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
                move.EnginePower = 1.0D;
                dolyaX = 2;
                dolyaY = 2;
                
                int myNextTileX = self.NextWaypointX;
                int myNextTileY = self.NextWaypointY;
                int myTileX = Convert.ToInt32(Math.Floor(self.X / game.TrackTileSize));
                int myTileY = Convert.ToInt32(Math.Floor(self.Y / game.TrackTileSize));

                TraceAnalis(myTileX, myTileY, self.NextWaypointIndex, world);

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


                //уже находимся в поворотном тайле
                if (world.TilesXY[myTileX][myTileY] == TileType.LeftTopCorner || world.TilesXY[myTileX][myTileY] == TileType.RightBottomCorner)
                {
                    myNextTileX = myTileX + (lastTileY - myTileY);
                    myNextTileY = myTileY + (lastTileX - myTileX);
                }
                if (world.TilesXY[myTileX][myTileY] == TileType.LeftBottomCorner || world.TilesXY[myTileX][myTileY] == TileType.RightTopCorner)
                {
                    myNextTileX = myTileX - (lastTileY - myTileY);
                    myNextTileY = myTileY - (lastTileX - myTileX);
                }

                //уже находимся в перекрестке или коридоре
                if (world.TilesXY[myTileX][myTileY] == TileType.Horizontal || world.TilesXY[myTileX][myTileY] == TileType.Vertical || world.TilesXY[myTileX][myTileY] == TileType.Crossroads)
                {
                    if (lastTileX != myTileX || lastTileY != myTileY)
                    {
                        myNextTileX = myTileX - (lastTileX - myTileX);
                        myNextTileY = myTileY - (lastTileY - myTileY);
                        double angleToWaypoin = self.GetAngleTo(self.NextWaypointX * game.TrackTileSize + game.TrackTileSize / 2, self.NextWaypointY * game.TrackTileSize + game.TrackTileSize / 2);
                        double angleToNextTile = self.GetAngleTo(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2, myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);
                        if (Math.Abs(angleToWaypoin - angleToNextTile) < Math.PI / 10)
                        {
                            myNextTileX = self.NextWaypointX;
                            myNextTileY = self.NextWaypointY;
                        }
                    }

                }

                // уже находимся в Т - обр перекрестке
                if (world.TilesXY[myTileX][myTileY] == TileType.LeftHeadedT || world.TilesXY[myTileX][myTileY] == TileType.BottomHeadedT ||
                    world.TilesXY[myTileX][myTileY] == TileType.RightHeadedT || world.TilesXY[myTileX][myTileY] == TileType.TopHeadedT || world.TilesXY[myTileX][myTileY] == TileType.Crossroads)
                {
                    if (world.MapName != "map01" && world.MapName != "map02" && world.MapName != "map04")
                    {
                        int[] nxtPNT = CrossroadAnalis(myTileX, myTileY, world.TilesXY[myTileX][myTileY], self.NextWaypointX, self.NextWaypointY, self.Angle);
                        myNextTileX = nxtPNT[0];
                        myNextTileY = nxtPNT[1];
                        dolyaX = nxtPNT[2];
                        dolyaY = nxtPNT[3];
                    }
                }

                int myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2);
                int myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);
                double myAngle = self.GetAngleTo(myNextX, myNextY);

                if (world.TilesXY[myTileX][myTileY] != TileType.Unknown && world.TilesXY[myTileX][myTileY] != TileType.Empty &&
                    world.TilesXY[myTileX][myTileY] != TileType.Horizontal && world.TilesXY[myTileX][myTileY] != TileType.Vertical)
                {
                    if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY + self.SpeedY) > speedControl)
                    { move.IsBrake = true; }
                    if (Math.Abs(myAngle) < Math.PI / 12 /*|| Math.Abs(self.WheelTurn) == 1*/)
                    { move.IsBrake = false; }
                }

                if (Math.Abs(myNextTileX - myTileX) < 2 && Math.Abs(myNextTileY - myTileY) < 2)
                {
                    //выбираем след тайл
                    switch ((world.TilesXY[myNextTileX][myNextTileY]))
                    {
                        case TileType.LeftTopCorner:
                            if ((myTileY - myNextTileY) == 1)
                            {
                                myNextTileX += 1;
                                dolyaX = 1;
                                dolyaY = 1;
                            }
                            if ((myTileX - myNextTileX) == 1)
                            {

                                myNextTileY += 1;
                                dolyaY = 1;
                                dolyaX = 1;
                            }
                            if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY * self.SpeedY) > speedControl * 2.0)
                            { move.IsBrake = true; }
                            myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2);
                            myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);
                            myAngle = self.GetAngleTo(myNextX, myNextY);
                            if (Math.Abs(myAngle) < Math.PI / 10)
                            { move.IsBrake = false; }
                            break;
                        case TileType.RightTopCorner:
                            if ((myTileX - myNextTileX) == -1)
                            {
                                myNextTileY += 1;
                                dolyaY = 1;
                                dolyaX = 3;
                            }
                            if ((myTileY - myNextTileY) == 1)
                            {
                                myNextTileX -= 1;
                                dolyaX = 3;
                                dolyaY = 1;
                            }
                            if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY * self.SpeedY) > speedControl * 2.0)
                            { move.IsBrake = true; }
                            myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2);
                            myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);
                            myAngle = self.GetAngleTo(myNextX, myNextY);
                            if (Math.Abs(myAngle) < Math.PI / 10)
                            { move.IsBrake = false; }
                            break;
                        case TileType.RightBottomCorner:
                            if ((myTileY - myNextTileY) == -1)
                            {
                                myNextTileX -= 1;
                                dolyaX = 3;
                                dolyaY = 3;
                            }
                            if ((myTileX - myNextTileX) == -1)
                            {
                                myNextTileY -= 1;
                                dolyaY = 3;
                                dolyaX = 3;
                            }
                            if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY * self.SpeedY) > speedControl * 2.0)
                            { move.IsBrake = true; }
                            myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2);
                            myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);
                            myAngle = self.GetAngleTo(myNextX, myNextY);
                            if (Math.Abs(myAngle) < Math.PI / 10)
                            { move.IsBrake = false; }
                            break;
                        case TileType.LeftBottomCorner:
                            if ((myTileX - myNextTileX) == 1)
                            {
                                myNextTileY -= 1;
                                dolyaY = 3;
                                dolyaX = 1;
                            }
                            if ((myTileY - myNextTileY) == -1)
                            {
                                myNextTileX += 1;
                                dolyaX = 1;
                                dolyaY = 3;
                            }
                            if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY * self.SpeedY) > speedControl * 2.0)
                            { move.IsBrake = true; }
                            myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2);
                            myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);
                            myAngle = self.GetAngleTo(myNextX, myNextY);
                            if (Math.Abs(myAngle) < Math.PI / 10)
                            { move.IsBrake = false; }
                            break;
                        //case TileType.Crossroads:
                        case TileType.LeftHeadedT: //притормаживаем
                        case TileType.RightHeadedT: //притормаживаем
                        case TileType.BottomHeadedT: //притормаживаем
                        case TileType.TopHeadedT: //притормаживаем
                            if (myNextTileX != self.NextWaypointX && myNextTileY != self.NextWaypointY)
                            {
                                int[] nxtPNT = CrossroadAnalis(myNextTileX, myNextTileY, world.TilesXY[myNextTileX][myNextTileY], self.NextWaypointX, self.NextWaypointY, self.Angle);
                                myNextTileX = nxtPNT[0];
                                myNextTileY = nxtPNT[1];
                                dolyaX = nxtPNT[2];
                                dolyaY = nxtPNT[3];
                            } else
                            {
                                int nextTileIndex = self.NextWaypointIndex + 1;
                                if (nextTileIndex > world.Waypoints.Length - 1)
                                { nextTileIndex = 0; }
                                int[] nxtPNT = CrossroadAnalis(myNextTileX, myNextTileY, world.TilesXY[myNextTileX][myNextTileY], world.Waypoints[nextTileIndex][0], myNextTileY = world.Waypoints[nextTileIndex][1], self.Angle);
                                myNextTileX = nxtPNT[0];
                                myNextTileY = nxtPNT[1];
                                dolyaX = nxtPNT[2];
                                dolyaY = nxtPNT[3];
                            }
                            if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY * self.SpeedY) > speedControl * 1.6 && Math.Abs(myNextTileX - myTileX) < 2 && Math.Abs(myNextTileY - myTileY) < 2)
                            { move.IsBrake = true; }
                            /*if (Math.Abs(self.WheelTurn) == 1)
                            { move.IsBrake = false; }*/
                            break;
                        default:
                            break;
                    }
                }

                myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize * dolyaX / 4);
                myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize * dolyaY / 4);

                double myDist = self.GetDistanceTo(myNextX, myNextY);
                myAngle = self.GetAngleTo(myNextX, myNextY);

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
                                        if (world.TilesXY[myTileX - 4 * dx][myTileY - 4 * dy] == TileType.Horizontal || world.TilesXY[myTileX - 4 * dx][myTileY - 4 * dy] == TileType.Vertical || world.TilesXY[myTileX - 4 * dx][myTileY - 4 * dy] == TileType.Crossroads)
                                        {
                                            if (world.TilesXY[myTileX - 5 * dx][myTileY - 5 * dy] == TileType.Horizontal || world.TilesXY[myTileX - 5 * dx][myTileY - 5 * dy] == TileType.Vertical || world.TilesXY[myTileX - 5 * dx][myTileY - 5 * dy] == TileType.Crossroads)
                                            {
                                                //шесть прямых клеток подряд
                                                if (self.GetAngleTo(((myTileX - 5 * dx) * game.TrackTileSize + game.TrackTileSize / 2), ((myTileY - 5 * dy) * game.TrackTileSize + game.TrackTileSize / 2)) < Math.PI / 10)
                                                {
                                                    move.IsUseNitro = true;
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
                    foreach (Unit bon in world.Bonuses)
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
            //метод Дейкстры
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
            startPoint.rasst = 0;
            startPoint.isLooking = true;
            startPoint.TileX = TileX;
            startPoint.TileY = TileY;
            List<GraphPoint> soseds = new List<GraphPoint>();
            soseds.Add(startPoint);
            bool stopFlag = false;
            while(!stopFlag)
            {
                soseds = GetSosedPoints(soseds, myGraph, world);
                foreach(GraphPoint pnt in soseds)
                {
                    if(pnt.TileX== world.Waypoints[WPI][0] && pnt.TileY== world.Waypoints[WPI][1])
                    { stopFlag = true; }
                }
            }
            for (int i = 0; i < world.Width - 1; i++)
            {
                for (int j = 0; j < world.Height - 1; j++)
                {
                    Console.WriteLine("Tile " + i + "-" + j + " parrentX=" + myGraph[i, j].parrentPoint[0]);
                    Console.WriteLine("Tile " + i + "-" + j + " parrentY=" + myGraph[i, j].parrentPoint[1]);
                }
            }

            return new int[] { 0, 0, 2, 2 };
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
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst> graph[myPoint.TileX, myPoint.TileY].rasst + 1)
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
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX+1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
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
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
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
                    case TileType.RightTopCorner:
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
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
                    case TileType.RightBottomCorner:
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
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
                    case TileType.BottomHeadedT:
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
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
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                        }
                        break;
                    case TileType.TopHeadedT:
                        //слева
                        if (!graph[myPoint.TileX - 1, myPoint.TileY].isLooking || graph[myPoint.TileX - 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX - 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX - 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX - 1, myPoint.TileY]);
                            graph[myPoint.TileX - 1, myPoint.TileY].isLooking = true;
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                        }
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                        }
                        break;
                    case TileType.RightHeadedT:
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
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
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
                        }
                        break;
                    case TileType.LeftHeadedT:
                        //сверху
                        if (!graph[myPoint.TileX, myPoint.TileY - 1].isLooking || graph[myPoint.TileX, myPoint.TileY - 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY - 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY - 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY - 1]);
                            graph[myPoint.TileX, myPoint.TileY - 1].isLooking = true;
                        }
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
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
                        }
                        //справа
                        if (!graph[myPoint.TileX + 1, myPoint.TileY].isLooking || graph[myPoint.TileX + 1, myPoint.TileY].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX + 1, myPoint.TileY].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX + 1, myPoint.TileY].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX + 1, myPoint.TileY]);
                            graph[myPoint.TileX + 1, myPoint.TileY].isLooking = true;
                        }
                        //снизу
                        if (!graph[myPoint.TileX, myPoint.TileY + 1].isLooking || graph[myPoint.TileX, myPoint.TileY + 1].rasst > graph[myPoint.TileX, myPoint.TileY].rasst + 1)
                        {
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[0] = myPoint.TileX;
                            graph[myPoint.TileX, myPoint.TileY + 1].parrentPoint[1] = myPoint.TileY;
                            graph[myPoint.TileX, myPoint.TileY + 1].rasst = graph[myPoint.TileX, myPoint.TileY].rasst + 1;
                            mySoseds.Add(graph[myPoint.TileX, myPoint.TileY + 1]);
                            graph[myPoint.TileX, myPoint.TileY + 1].isLooking = true;
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
                    default:
                        break;
                }
            }
            #endregion
            return mySoseds;
        }
        class MapPoint
        {
            public int[,] Connections = new int[4, 2];

            public MapPoint(int X, int Y, TileType tileType)
            {
                Connections[0, 0] = -1;
                Connections[1, 0] = -1;
                Connections[2, 0] = -1;
                Connections[3, 0] = -1;
                Connections[0, 1] = -1;
                Connections[1, 1] = -1;
                Connections[2, 1] = -1;
                Connections[3, 1] = -1;
                switch (tileType)
                {
                    case TileType.Horizontal:
                        Connections[0, 0] = X + 1;
                        Connections[0, 1] = Y;
                        Connections[2, 0] = X - 1;
                        Connections[2, 1] = Y;
                        break;
                    case TileType.Vertical:
                        Connections[1, 0] = X;
                        Connections[1, 1] = Y + 1;
                        Connections[3, 0] = X;
                        Connections[3, 1] = Y - 1;
                        break;
                    case TileType.LeftTopCorner:
                        //справа
                        Connections[0, 0] = X + 1;
                        Connections[0, 1] = Y;
                        //снизу
                        Connections[1, 0] = X;
                        Connections[1, 1] = Y + 1;
                        break;
                    case TileType.RightTopCorner:
                        //слева
                        Connections[2, 0] = X - 1;
                        Connections[2, 1] = Y;
                        //снизу
                        Connections[1, 0] = X;
                        Connections[1, 1] = Y + 1;
                        break;
                    case TileType.LeftBottomCorner:
                        //справа
                        Connections[0, 0] = X + 1;
                        Connections[0, 1] = Y;
                        //сверху
                        Connections[3, 0] = X;
                        Connections[3, 1] = Y - 1;
                        break;
                    case TileType.RightBottomCorner:
                        //слева
                        Connections[2, 0] = X - 1;
                        Connections[2, 1] = Y;
                        //сверху
                        Connections[3, 0] = X;
                        Connections[3, 1] = Y - 1;
                        break;
                    case TileType.LeftHeadedT:
                        //снизу
                        Connections[1, 0] = X;
                        Connections[1, 1] = Y + 1;
                        //слева
                        Connections[2, 0] = X - 1;
                        Connections[2, 1] = Y;
                        //сверху
                        Connections[3, 0] = X;
                        Connections[3, 1] = Y - 1;
                        break;
                    case TileType.TopHeadedT:
                        //справа
                        Connections[0, 0] = X + 1;
                        Connections[0, 1] = Y;
                        //слева
                        Connections[2, 0] = X - 1;
                        Connections[2, 1] = Y;
                        //сверху
                        Connections[3, 0] = X;
                        Connections[3, 1] = Y - 1;
                        break;
                    case TileType.RightHeadedT:
                        //справа
                        Connections[0, 0] = X + 1;
                        Connections[0, 1] = Y;
                        //снизу
                        Connections[1, 0] = X;
                        Connections[1, 1] = Y + 1;
                        //сверху
                        Connections[3, 0] = X;
                        Connections[3, 1] = Y - 1;
                        break;
                    case TileType.BottomHeadedT:
                        //справа
                        Connections[0, 0] = X + 1;
                        Connections[0, 1] = Y;
                        //снизу
                        Connections[1, 0] = X;
                        Connections[1, 1] = Y + 1;
                        //слева
                        Connections[2, 0] = X - 1;
                        Connections[2, 1] = Y;
                        break;
                    case TileType.Crossroads:
                        //справа
                        Connections[0, 0] = X + 1;
                        Connections[0, 1] = Y;
                        //снизу
                        Connections[1, 0] = X;
                        Connections[1, 1] = Y + 1;
                        //слева
                        Connections[2, 0] = X - 1;
                        Connections[2, 1] = Y;
                        //сверху
                        Connections[3, 0] = X;
                        Connections[3, 1] = Y - 1;
                        break;
                    default:
                        break;
                }
            }
        }
        public class GraphPoint
        {
            public int TileX;
            public int TileY;
            public int dolyaX = 2;
            public int dolyaY = 2;
            public double X;
            public double Y;
            public double rasst = -1;
            public bool isLooking = false;
            public int[] parrentPoint = new int[2];
        }
    }
}