using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Bot
{
    public partial class Controller
    {
        public void ClotheNakedBarracks()
        {
            if (vespene < 25)
                return;
            foreach (Unit barracks in units.barracks)
            {
                if(barracks.BuildProgress == 1 && barracks.AddOnTag == 0 && barracks.Orders.Count == 0)
                {
                    SC2APIProtocol.Action action = new SC2APIProtocol.Action();
                    if (units.techLabs.Count > units.reactors.Count)
                    {
                        if (vespene > 50)
                            action = CreateRawUnitCommand(Abilities.BUILD_REACTOR_BARRACKS);
                        else
                            return;
                    }
                    else
                    {
                        if(vespene > 25)
                            action = CreateRawUnitCommand(Abilities.BUILD_TECH_LAB_BARRACKS);
                    }
                    action.ActionRaw.UnitCommand.UnitTags.Add(barracks.Tag);
                    AddAction(action);
                }
            }
        }

        public bool researchTech()
        {
            //Orbitals
            foreach(Unit commandCenter in units.resourceCenters)
            {
                bool haveBarracks = false;
                if (commandCenter.UnitType != Units.COMMAND_CENTER)
                    continue;

                foreach(Unit barracks in units.barracks)
                    if (barracks.BuildProgress == 1 || barracks.Orders.Count > 0)
                        haveBarracks = true;

                if (units.resourceCenters.Count == 1 && minerals >= 100 && haveBarracks && commandCenter.Orders.Count > 0)
                {
                    if(commandCenter.Orders.Count > 0)
                    {
                        var cancelAction = CreateRawUnitCommand(Abilities.CANCEL_LAST);
                        cancelAction.ActionRaw.UnitCommand.UnitTags.Add(commandCenter.Tag);
                        AddAction(cancelAction);
                    }
                    var constructAction = CreateRawUnitCommand(Abilities.UPGRADE_TO_ORBITAL);
                    constructAction.ActionRaw.UnitCommand.UnitTags.Add(commandCenter.Tag);
                    AddAction(constructAction);
                }
                else if (minerals >= 150 && haveBarracks)
                {
                    if (commandCenter.Orders.Count > 0)
                    {
                        var cancelAction = CreateRawUnitCommand(Abilities.CANCEL_LAST);
                        cancelAction.ActionRaw.UnitCommand.UnitTags.Add(commandCenter.Tag);
                        AddAction(cancelAction);
                    }
                    var constructAction = CreateRawUnitCommand(Abilities.UPGRADE_TO_ORBITAL);
                    constructAction.ActionRaw.UnitCommand.UnitTags.Add(commandCenter.Tag);
                    AddAction(constructAction);
                }


            }


            //stim
            if(units.army.Count > 2 && !researchedTech.Contains(Abilities.RESEARCH_STIM))
            {
                foreach(Unit techLab in units.techLabs)
                {
                    if(techLab.Orders.Count == 0 && minerals > 100 && vespene > 100 && techLab.BuildProgress == 1)
                    {
                        SC2APIProtocol.Action action = new SC2APIProtocol.Action();
                        action = CreateRawUnitCommand(Abilities.RESEARCH_STIM);
                        action.ActionRaw.UnitCommand.UnitTags.Add(techLab.Tag);
                        AddAction(action);

                        researchedTech.Add(Abilities.RESEARCH_STIM);
                        return false;
                    }
                    else if (techLab.Orders.Count == 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            //combat shields
            if (mariners.Count > 5 && !researchedTech.Contains(Abilities.RESEARCH_COMBAT_SHIELDS))
            {
                foreach (Unit techLab in units.techLabs)
                {
                    if (techLab.Orders.Count == 0 && minerals > 100 && vespene > 100 && techLab.BuildProgress == 1)
                    {
                        SC2APIProtocol.Action action = new SC2APIProtocol.Action();
                        action = CreateRawUnitCommand(Abilities.RESEARCH_COMBAT_SHIELDS);
                        action.ActionRaw.UnitCommand.UnitTags.Add(techLab.Tag);
                        AddAction(action);

                        researchedTech.Add(Abilities.RESEARCH_COMBAT_SHIELDS);
                        return false;
                    }
                    else if (techLab.Orders.Count == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            //concussive shells
            if (marauders.Count > 5 && !researchedTech.Contains(Abilities.RESEARCH_CONCUSSIVE_SHELLS))
            {
                foreach (Unit techLab in units.techLabs)
                {
                    if (techLab.Orders.Count == 0 && minerals > 50 && vespene > 50 && techLab.BuildProgress == 1)
                    {
                        SC2APIProtocol.Action action = new SC2APIProtocol.Action();
                        action = CreateRawUnitCommand(Abilities.RESEARCH_CONCUSSIVE_SHELLS);
                        action.ActionRaw.UnitCommand.UnitTags.Add(techLab.Tag);
                        AddAction(action);

                        researchedTech.Add(Abilities.RESEARCH_CONCUSSIVE_SHELLS);
                        return false;
                    }
                    else if (techLab.Orders.Count == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }


    }
}