using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Bot
{
    public partial class Controller
    {
        public void spendOrbitalEnergy()
        {
            //mules
            foreach(Unit orbital in units.resourceCenters)
            {
                if(orbital.Energy >= 50)
                {
                    foreach (Unit mineralField in units.mineralFields)
                    {
                        if (GetDistance(orbital, mineralField) < 10)
                        {
                            var muleAction = CreateRawUnitCommand(Abilities.CALLDOWN_MULE);
                            muleAction.ActionRaw.UnitCommand.TargetUnitTag = mineralField.Tag;
                            muleAction.ActionRaw.UnitCommand.UnitTags.Add(orbital.Tag);
                            AddAction(muleAction);
                            break;
                        }
                    }

                }
            }
        }


    }
}