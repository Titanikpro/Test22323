using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
    public sealed class MyStrategy : IStrategy {
        const int ZalipStep = 20;
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



        public void Move(Car self, World world, Game game, Move move)
        {

            if (ZalipCount > 0)
            {
                if (ZalipCount < ZalipStep/4)
                {
                    move.EnginePower = 0;
                    move.WheelTurn = ZalipAngle;
                }
                else
                {
                    move.EnginePower = -1.0;
                    move.WheelTurn = ZalipAngle;
                }
                ZalipCount--;
            }
            else
            {
                move.EnginePower = .9D;

                

                int myNextTileX = self.NextWaypointX;
                int myNextTileY = self.NextWaypointY;
                int myTileX = Convert.ToInt32(Math.Floor(self.X / game.TrackTileSize));
                int myTileY = Convert.ToInt32(Math.Floor(self.Y / game.TrackTileSize));

                if (world.Tick > game.InitialFreezeDurationTicks)
                {
                    PositionUpdateTick++;
                    TileUpdateTick++;
                }
                else
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

                if(Math.Abs(myTileX-lastTileX)>=2 || Math.Abs(myTileY-lastTileY)>=2)
                {
                    Console.WriteLine();
                }

                bool flag = false;
                if (world.MapName != "default" && world.MapName != "map01" && world.MapName != "map02" && world.MapName != "map04" && world.MapName != "map05")
                {
                    //уже находимс€ в поворотном тайле
                    if (world.TilesXY[myTileX][myTileY] == TileType.LeftTopCorner || world.TilesXY[myTileX][myTileY] == TileType.RightBottomCorner)
                    {
                        myNextTileX = myTileX + (lastTileY - myTileY);
                        myNextTileY = myTileY + (lastTileX - myTileX);
                        flag = true;
                    }
                    if (world.TilesXY[myTileX][myTileY] == TileType.LeftBottomCorner || world.TilesXY[myTileX][myTileY] == TileType.RightTopCorner)
                    {
                        myNextTileX = myTileX - (lastTileY - myTileY);
                        myNextTileY = myTileY - (lastTileX - myTileX);
                        flag = true;
                    }

                    //уже находимс€ в перекрестке или коридоре
                    if (world.TilesXY[myTileX][myTileY] == TileType.Horizontal || world.TilesXY[myTileX][myTileY] == TileType.Vertical || world.TilesXY[myTileX][myTileY] == TileType.Crossroads)
                    {
                        myNextTileX = myTileX - (lastTileX - myTileX);
                        myNextTileY = myTileY - (lastTileY - myTileY);
                        double angleToWaypoin = self.GetAngleTo(self.NextWaypointX * game.TrackTileSize + game.TrackTileSize / 2, self.NextWaypointY * game.TrackTileSize + game.TrackTileSize / 2);
                        double angleToNextTile = self.GetAngleTo(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2, myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);
                        if(Math.Abs(angleToWaypoin-angleToNextTile)<Math.PI/6)
                        {
                            myNextTileX = self.NextWaypointX;
                            myNextTileY = self.NextWaypointY;
                        }
                    }

                    //уже находимс€ в “-обр перекрестке
                    //if(world.TilesXY[myTileX][myTileY] == TileType.LeftHeadedT)
                }

                if (true/*!flag**/)
                {
                    //повороты направо
                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.LeftTopCorner && (myTileY - self.NextWaypointY) == 1)
                    {
                        myNextTileX += 1;
                    }

                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.RightTopCorner && (myTileX - self.NextWaypointX) == -1)
                    {
                        myNextTileY += 1;
                    }
                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.RightBottomCorner && (myTileY - self.NextWaypointY) == -1)
                    {
                        myNextTileX -= 1;
                    }
                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.LeftBottomCorner && (myTileX - self.NextWaypointX) == 1)
                    {
                        myNextTileY -= 1;
                    }

                    //повороты налево
                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.LeftTopCorner && (myTileX - self.NextWaypointX) == 1)
                    {
                        myNextTileY += 1;
                    }

                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.RightTopCorner && (myTileY - self.NextWaypointY) == 1)
                    {
                        myNextTileX -= 1;
                    }
                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.RightBottomCorner && (myTileX - self.NextWaypointX) == -1)
                    {
                        myNextTileY -= 1;
                    }
                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.LeftBottomCorner && (myTileY - self.NextWaypointY) == -1)
                    {
                        myNextTileX += 1;
                    }

                    //“-обр перекрестки
                    if (world.TilesXY[myNextTileX][myNextTileY] == TileType.LeftHeadedT)
                    {
                        if (self.NextWaypointX == myNextTileX && self.NextWaypointY == myNextTileY) //если след вейпоинт на след перекрестке
                        {
                            //притормаживаем
                            if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY + self.SpeedY) > 4)
                            { move.IsBrake = true; }
                        }
                        else
                        {

                            //точка слева
                            double virtTileX1 = (myNextTileX - 1) * game.TrackTileSize + game.TrackTileSize / 2;
                            double virtTileY1 = myNextTileY * game.TrackTileSize + game.TrackTileSize / 2;
                            //точка снизу
                            double virtTileX2 = myNextTileX * game.TrackTileSize + game.TrackTileSize / 2;
                            double virtTileY2 = (myNextTileY + 1) * game.TrackTileSize + game.TrackTileSize / 2;
                            //точка сверху
                            double virtTileX3 = myNextTileX * game.TrackTileSize + game.TrackTileSize / 2;
                            double virtTileY3 = (myNextTileY - 1) * game.TrackTileSize + game.TrackTileSize / 2;
                            double wayPoinX = self.NextWaypointX * game.TrackTileSize + game.TrackTileSize / 2;
                            double wayPoinY = self.NextWaypointY * game.TrackTileSize + game.TrackTileSize / 2;
                            myNextTileX = myTileX - 1;
                            myNextTileY = myTileY;
                            //if()
                        }
                    }
                }

                //ускорение
                if(world.Tick > game.InitialFreezeDurationTicks && (world.TilesXY[myTileX][myTileY] == TileType.Horizontal || world.TilesXY[myTileX][myTileY] == TileType.Vertical))
                {
                    try
                    {
                        int dx = lastTileX - myTileX;
                        int dy = lastTileY - myTileY;
                        if (world.TilesXY[myTileX-dx][myTileY-dy] == TileType.Horizontal || world.TilesXY[myTileX-dx][myTileY-dy] == TileType.Vertical || world.TilesXY[myTileX - dx][myTileY - dy] == TileType.Crossroads)
                        {
                            if(world.TilesXY[myTileX-2*dx][myTileY-2*dy] == TileType.Horizontal || world.TilesXY[myTileX-2*dx][myTileY-2*dy] == TileType.Vertical || world.TilesXY[myTileX - 2 * dx][myTileY - 2 * dy] == TileType.Crossroads)
                            {
                                if(world.TilesXY[myTileX-3*dx][myTileY-3*dy] == TileType.Horizontal || world.TilesXY[myTileX-3*dx][myTileY-3*dy] == TileType.Vertical || world.TilesXY[myTileX - 3 * dx][myTileY - 3 * dy] == TileType.Crossroads)
                                {
                                    //четыре пр€мых клетки подр€д
                                    if (self.GetAngleTo(((myTileX - 3 * dx) * game.TrackTileSize + game.TrackTileSize / 2), ((myTileY - 3 * dy) * game.TrackTileSize + game.TrackTileSize / 2)) > Math.PI / 8)
                                    {
                                        move.IsUseNitro = true;
                                    }
                                }
                            }
                        }
                    }catch(Exception e)
                    {
                        Console.WriteLine("Nitro ERROR");
                    }
                }

                //сброс масла
                if(world.TilesXY[lastTileX][lastTileY] == TileType.LeftBottomCorner || world.TilesXY[lastTileX][lastTileY] == TileType.LeftTopCorner || 
                    world.TilesXY[lastTileX][lastTileY] == TileType.RightBottomCorner || world.TilesXY[lastTileX][lastTileY] == TileType.RightTopCorner || 
                    world.TilesXY[lastTileX][lastTileY] == TileType.Crossroads)
                {
                    if(Math.Sqrt(self.SpeedX* self.SpeedX+ self.SpeedY* self.SpeedY) > 5) { move.IsSpillOil = true; }
                }

                //стрельба по врагам
                foreach(Car vrag in world.Cars)
                {
                    if(self.Id!=vrag.Id)
                    {
                        if ((self.GetAngleTo(vrag) < Math.PI / 8) && (self.GetDistanceTo(vrag) < game.InitialFreezeDurationTicks * 2))
                        {
                            try
                            {
                                //if (Math.Sign(Math.Cos(self.Angle) / (vrag.Y - self.Y)) > 0)
                                {
                                    move.IsThrowProjectile = true;
                                }
                            }catch(Exception e)
                            { }
                        }
                    }
                }
                int myNextX = Convert.ToInt32(myNextTileX * game.TrackTileSize + game.TrackTileSize / 2);
                int myNextY = Convert.ToInt32(myNextTileY * game.TrackTileSize + game.TrackTileSize / 2);

                double myAngle = self.GetAngleTo(myNextX, myNextY);
                move.WheelTurn = AngleRegul(myAngle);

                if (world.TilesXY[self.NextWaypointX][self.NextWaypointY] == TileType.LeftTopCorner || world.TilesXY[self.NextWaypointX][self.NextWaypointY] == TileType.LeftBottomCorner || world.TilesXY[self.NextWaypointX][self.NextWaypointY] == TileType.RightBottomCorner || world.TilesXY[self.NextWaypointX][self.NextWaypointY] == TileType.RightTopCorner)
                {
                    if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY + self.SpeedY) > 4)
                    { move.IsBrake = true; }
                    if (Math.Abs(myAngle) < Math.PI / 4)
                    { move.IsBrake = false; }
                }
                if (world.TilesXY[myTileX][myTileY] == TileType.LeftTopCorner || world.TilesXY[myTileX][myTileY] == TileType.RightBottomCorner || world.TilesXY[myTileX][myTileY] == TileType.RightTopCorner || world.TilesXY[myTileX][myTileY] == TileType.LeftTopCorner)
                {
                    if (Math.Sqrt(self.SpeedX * self.SpeedX + self.SpeedY + self.SpeedY) > 3)
                    { move.IsBrake = true; }
                    if (Math.Abs(myAngle) < Math.PI / 4)
                    { move.IsBrake = false; }
                }

                if (PositionUpdateTick > 50)
                {
                    //защита от залипани€ на одном месте
                    ZalipCount = ZalipStep;
                    ZalipAngle = -Math.Sign(AngleRegul(myAngle));
                }

                if (self.Durability < 0.1) //если мало HP
                {
                    //использовать ремкомплект
                }
                if (Math.Abs(lastX - self.X) > 10)
                {
                    lastX = self.X;
                    PositionUpdateTick = 0;
                }
                if (Math.Abs(lastY - self.Y) > 10)
                {
                    lastY = self.Y;
                    PositionUpdateTick = 0;
                }
                
                
                Console.Clear();
                Console.WriteLine("Tick = " + world.Tick);
                Console.WriteLine("TileUpdateTick = " + TileUpdateTick);
                Console.WriteLine("PositionUpdateTick = " + PositionUpdateTick);
                Console.WriteLine("NextTileX = " + myNextTileX + "   | MyTileX = " + myTileX);
                Console.WriteLine("NextTileY = " + myNextTileY + "   | MyTileY = " + myTileY);
                Console.WriteLine();
                Console.WriteLine("lastTileX = " + lastTileX);
                Console.WriteLine("lastTileY = " + lastTileY);
                Console.WriteLine();
                Console.WriteLine("NextX = " + myNextX + "   | MyX = " + self.X);
                Console.WriteLine("NextY = " + myNextY + "   | MyY = " + self.Y);
                Console.WriteLine();
                Console.WriteLine("MyAngle = " + myAngle);
                Console.WriteLine("WheelTurn = " + move.WheelTurn);
                Console.WriteLine();
                Console.WriteLine("SpeedX = " + self.SpeedX);
                Console.WriteLine("SpeedY = " + self.SpeedY);
                Console.WriteLine();
                Console.WriteLine(world.MapName);
                Console.WriteLine();
                
            }
        }
                
        double erroldAngle = 0;
        public double AngleRegul(double ActualAngle)
        {
            double kp = 2.5;
            double kd = 0.0;
            double ki = 0;
            double U=0.0;
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

    }
}