using System.Collections.Generic;
using SC2APIProtocol;
using System;

namespace Bot
{
    public class MineralDistanceWrapper : IComparable
    {
        public MineralDistanceWrapper(Unit leftMineralField, Unit rightMineralField, double distance)
        {
            this.leftMineralField = leftMineralField;
            this.rightMineralField = rightMineralField;
            this.distance = distance;
        }
        public Unit leftMineralField { get; set; }
        public Unit rightMineralField { get; set; }
        public double distance { get; set; }

        public int CompareTo(object other)
        {
            
            return this.distance.CompareTo(((MineralDistanceWrapper)other).distance);
        }

    }
}
