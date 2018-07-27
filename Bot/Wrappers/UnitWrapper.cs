using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Bot.Wrappers
{
    public class UnitWrapper
    {
        public UnitWrapper(Unit unit)
        {
            this.unit = unit;
            setFrameCounts();
        }

        public UnitWrapper(Unit unit, ulong targetUnitTag) : this (unit)
        {
            this.targetUnitTag = targetUnitTag;
        }
        private void setFrameCounts()
        {
            if(unit.UnitType == Units.MARAUDER)
            {
                attackingFrames = 12;
                movingFrames = 12;
            }
            else if (unit.UnitType == Units.MARINE)
            {
                attackingFrames = 8;
                movingFrames = 8;
            }
        }
        public Unit unit { get; set; }
        public ulong targetUnitTag { get; set; }
        public ulong attackingFrames { get; set; }
        public ulong movingFrames { get; set; }

        public ulong frameStartedAttack { get; set; }

    }
}
    