using Bot.Wrappers;
using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Bot
{
    public partial class Controller
    {

        private ArmyManagementMode armyManagementMode = ArmyManagementMode.defendMode;

        public void ManageArmyMovements()
        {
            float medianX = 0;
            float medianY = 0;

            if (units.army.Count < 4)
                return;
            int enemyArmyCount = enemyUnits.army.Count;

            // determine density via clumpiness because mass / volume makes no sense
            List<float> unitXs = new List<float>();
            List<float> unitYs = new List<float>();

            foreach (UnitWrapper unit in units.army)
            {
                unitXs.Add(GetPosition(unit.unit).X);
                unitYs.Add(GetPosition(unit.unit).Y);
            }
            unitXs.Sort();
            unitYs.Sort();

            medianX = unitXs[unitXs.Count / 2];
            medianY = unitYs[unitYs.Count / 2];


            List<MineralDistanceWrapper> unitsToConsiderForClumpMedian = new List<MineralDistanceWrapper>();
            foreach (UnitWrapper unit in units.army)
            {
                
                double distanceAway = GetDistance(unit.unit, new Vector3(medianX, medianY, GetPosition(unit.unit).Z));
                unitsToConsiderForClumpMedian.Add(new MineralDistanceWrapper(unit.unit, null, distanceAway));
            }
            //remove the outlier units
            unitsToConsiderForClumpMedian.Sort();
            for (int i = 0; i < Math.Round(unitsToConsiderForClumpMedian.Count / 4f); i++)
            {
                unitsToConsiderForClumpMedian.RemoveAt(0);
                unitsToConsiderForClumpMedian.RemoveAt(unitsToConsiderForClumpMedian.Count - 1);
            }
            unitXs.Clear();
            unitYs.Clear();
            foreach (MineralDistanceWrapper unit in unitsToConsiderForClumpMedian)
            {
                unitXs.Add(GetPosition(unit.leftMineralField).X);
                unitYs.Add(GetPosition(unit.leftMineralField).Y);
            }
            unitXs.Sort();
            unitYs.Sort();

            medianX = unitXs[unitXs.Count / 2];
            medianY = unitYs[unitYs.Count / 2];

            List<double> distanceFromMedian = new List<double>();
            foreach (MineralDistanceWrapper unit in unitsToConsiderForClumpMedian)
            {

                double distanceAway = GetDistance(unit.leftMineralField, new Vector3(medianX, medianY, GetPosition(unit.leftMineralField).Z));
                distanceFromMedian.Add(distanceAway);
            }
            distanceFromMedian.Sort();



            double somePercentileOfDistancesFromTheMedian = distanceFromMedian[distanceFromMedian.Count / 4];

            //
            // determine what mode our army should be in
            //

            //Logger.Info("averageDistance = " + somePercentileOfDistancesFromTheMedian + " " + medianX + " " + medianY);
            // This number needs to scale with the army count!
            if (somePercentileOfDistancesFromTheMedian > 2 * (units.army.Count / 20f))
            {
                armyManagementMode = ArmyManagementMode.clumpMode;
            }
            else if (somePercentileOfDistancesFromTheMedian <= 1.25 || armyManagementMode != ArmyManagementMode.clumpMode)
            {
                // is this average distance small? (Are we well clumped)?
                if (enemyArmyCount == 0)
                {
                    if (units.army.Count > (units.workers.Count / 2.2f))
                        armyManagementMode = ArmyManagementMode.attackMode;
                    else if (units.army.Count <= (units.workers.Count / 4))
                        armyManagementMode = ArmyManagementMode.defendMode;
                    else
                        armyManagementMode = ArmyManagementMode.defendMode;
                }
                else
                {
                    //if the distance is small, we are well clumped so:
                    if (enemyArmyCount <= units.army.Count)
                    {
                        armyManagementMode = ArmyManagementMode.attackMode;
                    }
                    else if (enemyArmyCount > units.army.Count)
                    {
                        armyManagementMode = ArmyManagementMode.defendMode;
                    }
                }
            }
            else
            {
                armyManagementMode = ArmyManagementMode.clumpMode;
            }


            //
            // take action based on the mode and if there are any enemies around
            //
            //Logger.Info("Mode = " + armyManagementMode);
            Vector3 medianVector = new Vector3(medianX, medianY, zVal);

            Attack(units.armySupport, medianVector);

            if (armyManagementMode == ArmyManagementMode.clumpMode)
            {
                if (enemyArmyCount == 0)
                {
                    Attack(units.army, medianVector);
                }
                else
                {
                    foreach (UnitWrapper unit in units.army)
                    {
                        Unit targetUnit = new Unit();
                        double minDistanceToEnemy = double.MaxValue;
                        List<Unit> tooCloseUnits = new List<Unit>();
                        bool melee = false;

                        foreach (UnitWrapper enemyUnit in enemyUnits.army)
                        {
                            double distanceToEnemy = GetDistance(unit.unit, enemyUnit.unit);
                            if (distanceToEnemy < minDistanceToEnemy)
                            {
                                targetUnit = enemyUnit.unit;
                                minDistanceToEnemy = distanceToEnemy;
                            }
                            if(distanceToEnemy < 4 && Units.MeleeOrLowRangeUnits.Contains(enemyUnit.unit.UnitType))
                            {
                                melee = true;
                            }
                        }
                        
                        if (GetDistance(targetUnit, medianVector) > 10)
                        {
                            // engage but head towards the middle
                            Attack(new List<UnitWrapper> { unit }, medianVector);
                        }
                        else
                        {
                            Attack(new List<UnitWrapper> { unit }, GetPosition(targetUnit), melee);
                        }

                    }
                }
            }
            else
            {
                if (enemyArmyCount == 0)
                {
                    //are there any workers or buildings?
                    if (enemyUnits.workers.Count > 0)
                    {
                        //attack them?
                    }
                    if (enemyUnits.buildings.Count > 0)
                    {
                        //attack them?
                    }
                    if (armyManagementMode == ArmyManagementMode.defendMode)
                    {
                        Attack(units.army, defendPoint);
                    }
                    else if (armyManagementMode == ArmyManagementMode.attackMode)
                    {
                        if (enemyUnits.buildings.Count == 0)
                            Attack(units.army, enemyLocations[0]);
                        else
                        {

                            foreach (UnitWrapper unit in units.army)
                            {
                                Unit targetBuilding = new Unit();
                                double minDistanceToEnemyBuilding = double.MaxValue;
                                foreach (Unit enemyBuilding in enemyUnits.buildings)
                                {
                                    double distanceToEnemyBuilding = GetDistance(unit.unit, enemyBuilding);
                                    if (distanceToEnemyBuilding < minDistanceToEnemyBuilding)
                                    {
                                        targetBuilding = enemyBuilding;
                                        minDistanceToEnemyBuilding = distanceToEnemyBuilding;
                                    }
                                }
                                Attack(new List<UnitWrapper> { unit }, GetPosition(targetBuilding));
                            }
                        }
                    }
                }
                else //attack or defend mode
                {
                    foreach(UnitWrapper unit in units.army)
                    {
                        Unit targetUnit = new Unit();
                        double minDistanceToEnemy = double.MaxValue;
                        bool melee = false;
                        foreach(UnitWrapper enemyUnit in enemyUnits.army)
                        {
                            double distanceToEnemy = GetDistance(unit.unit, enemyUnit.unit);
                            if (distanceToEnemy < minDistanceToEnemy)
                            {
                                targetUnit = enemyUnit.unit;
                                minDistanceToEnemy = distanceToEnemy;
                            }
                            if (distanceToEnemy < 4 && Units.MeleeOrLowRangeUnits.Contains(enemyUnit.unit.UnitType))
                            {
                                melee = true;
                            }
                        }

                        Attack(new List<UnitWrapper> { unit }, GetPosition(targetUnit), melee);
                    }
                    
                }
            }
        }

        public void ManageArmySupportMovements()
        {

        }

        public void ManageStim()
        {
            int enemyCount = enemyUnits.army.Count;

            if (enemyCount == 0)
            {
                return;
            }
            else
            {
                foreach (Unit armyUnit in GetUnits(Units.ArmyUnits))
                {
                    bool isEnemyNearby = false;
                    foreach(UnitWrapper unit in enemyUnits.army)
                    {
                        if(GetDistance(unit.unit, armyUnit) < 10)
                        {
                            isEnemyNearby = true;
                            break;
                        }
                    }
                    if (!isEnemyNearby)
                        continue;

                    bool alreadyStimmed = false;
                    if (armyUnit.UnitType == Units.MARAUDER || armyUnit.UnitType == Units.MARINE)
                    {

                        foreach(int buffId in armyUnit.BuffIds)
                        {
                            if (buffId == Buffs.STIM || buffId == Buffs.STIM_MARAUDER)
                            {
                                alreadyStimmed = true;
                                break;
                            }
                        }
                        if (alreadyStimmed)
                            continue;
                        int command = 0;
                        if (armyUnit.UnitType == Units.MARAUDER)
                            command = Abilities.STIM_MARAUDER;
                        else
                            command = Abilities.STIM_MARINE;
                        var stimAction = CreateRawUnitCommand(command);
                        stimAction.ActionRaw.UnitCommand.UnitTags.Add(armyUnit.Tag);
                        AddAction(stimAction);
                        break;
                    }

                }
            }
        }

        public void PopulateRefineries()
        {
            foreach (Unit refinery in units.refineries)
            {
                if (refinery.BuildProgress < 1 || refinery.VespeneContents == 0)
                    continue;
                if (refinery.AssignedHarvesters > refinery.IdealHarvesters)
                {
                    //how did this happen...
                    ;
                }
                else if (refinery.AssignedHarvesters < refinery.IdealHarvesters)
                {
                    var returnAction = CreateRawUnitCommand(Abilities.SMART);
                    returnAction.ActionRaw.UnitCommand.UnitTags.Add(GetAvailableWorker().Tag);
                    returnAction.ActionRaw.UnitCommand.TargetUnitTag = refinery.Tag;
                    returnAction.ActionRaw.UnitCommand.QueueCommand = true;
                    AddAction(returnAction);
                }
            }

        }

        public void DealWithLazyWorkers()
        {
            Unit FreshCommandCenter = null;
            foreach (Unit commandCenter in units.resourceCenters)
            {
                if (commandCenter.AssignedHarvesters <= commandCenter.IdealHarvesters)
                {
                    FreshCommandCenter = commandCenter;
                }
            }
            if (FreshCommandCenter == null)
                return;
            Unit closeMineralField = new Unit();
            foreach (Unit mineralField in units.mineralFields)
            {
                if (GetDistance(mineralField, FreshCommandCenter) < 10)
                    closeMineralField = mineralField;
            }
            foreach (Unit worker in units.workers)
            {
                if (worker.Orders.Count == 0 && worker.UnitType != Units.MULE)
                {
                    var returnAction = CreateRawUnitCommand(Abilities.SMART);
                    returnAction.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
                    returnAction.ActionRaw.UnitCommand.TargetUnitTag = closeMineralField.Tag;
                    returnAction.ActionRaw.UnitCommand.QueueCommand = true;
                    AddAction(returnAction);
                    break;
                }
            }
            foreach (Unit worker in units.workers)
            {
                if (worker.Orders.Count == 0 && worker.UnitType == Units.MULE)
                {
                    foreach(Unit mineralField in units.mineralFields)
                    {
                        if(GetDistance(worker, mineralField) < 1)
                        {
                            var returnAction = CreateRawUnitCommand(Abilities.SMART);
                            returnAction.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
                            returnAction.ActionRaw.UnitCommand.TargetUnitTag = mineralField.Tag;
                            returnAction.ActionRaw.UnitCommand.QueueCommand = true;
                            AddAction(returnAction);
                            break;
                        }
                    }

                }
            }
        }

        public void DealWithOverSaturation(List<Unit> buildingsToCheck)
        {
            if (buildingsToCheck.Count == 1 && buildingsToCheck[0].UnitType == Units.COMMAND_CENTER)
                return;
            foreach (Unit buildingToCheck in buildingsToCheck)
            {
                if (buildingToCheck.AssignedHarvesters > buildingToCheck.IdealHarvesters)
                {
                    foreach (Unit worker in units.workers)
                    {
                        // returning minerals
                        if (worker.Orders.Count == 1 && (worker.Orders[0].AbilityId == 295 || worker.Orders[0].AbilityId == 296) && worker.Orders[0].TargetUnitTag == buildingToCheck.Tag)
                        {
                            var returnAction = new SC2APIProtocol.Action();
                            
                            if (buildingToCheck.UnitType != Units.REFINERY)
                            {
                                Unit FreshCommandCenter = new Unit();
                                foreach (Unit commandCenterNotOurs in units.resourceCenters)
                                {
                                    if (commandCenterNotOurs.AssignedHarvesters <= commandCenterNotOurs.IdealHarvesters)
                                        FreshCommandCenter = commandCenterNotOurs;
                                }
                                if (FreshCommandCenter.Tag == 0)
                                    return;
                                Unit closeMineralField = new Unit();
                                foreach (Unit mineralField in units.mineralFields)
                                {
                                    if (GetDistance(mineralField, FreshCommandCenter) < 10)
                                        closeMineralField = mineralField;
                                }
                                returnAction = CreateRawUnitCommand(Abilities.STOP);
                            }
                            else
                            {
                                returnAction = CreateRawUnitCommand(Abilities.SMART);
                            }
                            returnAction.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
                            AddAction(returnAction);
                            break;
                        }
                    }
                }
            }
        }
    }
}
    