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
        public void TrainWorker(Unit resourceCenter, bool queue = false)
        {
            if (resourceCenter == null) return;

            if ((!queue) && (GetUnitOrders(resourceCenter) == Abilities.TRAIN_SCV))
                return;

            var action = CreateRawUnitCommand(Abilities.TRAIN_SCV);
            action.ActionRaw.UnitCommand.UnitTags.Add(resourceCenter.Tag);
            AddAction(action);
        }

        public void TrainMarauder(Unit barracks)
        {
            if (barracks == null) return;
            if (barracks.Orders.Count == 1)
                return;

            var action = CreateRawUnitCommand(Abilities.TRAIN_MARAUDER);
            action.ActionRaw.UnitCommand.UnitTags.Add(barracks.Tag);
            AddAction(action);

        }
        public void TrainMedivac(Unit starport)
        {
            if (starport == null) return;
            if (starport.Orders.Count > 1)
                return;

            var action = CreateRawUnitCommand(Abilities.TRAIN_MEDIVAC);
            action.ActionRaw.UnitCommand.UnitTags.Add(starport.Tag);
            AddAction(action);
        }

        public void TrainMarine(Unit barracks)
        {
            if (barracks == null) return;
            if (barracks.Orders.Count == 2)
                return;
            if (barracks.Orders.Count == 1 && barracks.AddOnTag == 0)
                return;

            var action = CreateRawUnitCommand(Abilities.TRAIN_MARINE);
            action.ActionRaw.UnitCommand.UnitTags.Add(barracks.Tag);
            AddAction(action);
        }

        public void CommandCenterPathingQueries()
        {
            foreach (VectorWrapper expandLocation in expandLocations)
            {
                bool AlreadyCommandCenterHere = false;
                foreach (Unit commandCenter in units.resourceCenters)
                {
                    if (GetDistance(commandCenter, expandLocation.vector) < 10)
                        AlreadyCommandCenterHere = true;
                }
                if (AlreadyCommandCenterHere)
                    continue;

                Vector3 originalCCPosition = GetPosition(units.resourceCenters[0]);
                Unit worker = GetAvailableWorker();
                Vector3 workerPos = GetPosition(worker);

                RequestQueryPathing requestQueryPathing = new RequestQueryPathing();
                requestQueryPathing.UnitTag = worker.Tag;
                requestQueryPathing.EndPos = new Point2D();
                requestQueryPathing.EndPos.X = expandLocation.vector.X;
                requestQueryPathing.EndPos.Y = expandLocation.vector.Y;
                this.queries.Pathing.Add(requestQueryPathing);
            }
        }

        public void ConstructQuereies(uint unitType)
        {
            var worker = GetAvailableWorker();
            if (worker == null) return;

            Vector3 startingSpot;
            Unit constructionTarget = new Unit();
            if (units.resourceCenters.Count > 0)
            {
                var cc = units.resourceCenters[0];
                startingSpot = GetPosition(cc);
            }
            else
                startingSpot = GetPosition(worker);

            var radius = 12;
            var innerRadius = 0;

            //trying to find a valid construction spot
            Vector3 constructionSpot = new Vector3();
            if (unitType == Units.COMMAND_CENTER)
            {
                //constructionSpot = FindCommandCenterLocation(startingSpot, worker);
            }
            else if (unitType == Units.REFINERY)
            {
                constructionTarget = FindVespeneGeyser();
            }
            else
            {
                while (true)
                {
                    bool valid = true;
                    constructionSpot = new Vector3(startingSpot.X + innerRadius + random.Next(-radius, radius + 1), startingSpot.Y + innerRadius + random.Next(-radius, radius + 1), worker.Pos.Z);
                    foreach (Unit mineralField in units.mineralFields)
                    {
                        if (GetDistance(mineralField, constructionSpot) <= 7)
                        {
                            valid = false;
                            break;
                        }
                    }
                    foreach (Unit building in units.buildings)
                    {
                        if (GetDistance(building, constructionSpot) <= 5)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (valid)
                        break;
                }
            }
            Vector3 AddOnPlacement = new Vector3(constructionSpot.X + 2, constructionSpot.Y, constructionSpot.Z);

            {
                RequestQueryBuildingPlacement requestQueryBP = new RequestQueryBuildingPlacement();
                requestQueryBP.AbilityId = Abilities.FromBuilding[unitType];
                requestQueryBP.PlacingUnitTag = worker.Tag;
                requestQueryBP.TargetPos = new Point2D();
                requestQueryBP.TargetPos.X = constructionSpot.X;
                requestQueryBP.TargetPos.Y = constructionSpot.Y;
                this.queries.Placements.Add(requestQueryBP);
            }

            {
                RequestQueryBuildingPlacement requestQueryBP = new RequestQueryBuildingPlacement();
                requestQueryBP.AbilityId = Abilities.FromBuilding[unitType];
                requestQueryBP.PlacingUnitTag = worker.Tag;
                requestQueryBP.TargetPos = new Point2D();
                requestQueryBP.TargetPos.X = AddOnPlacement.X;
                requestQueryBP.TargetPos.Y = AddOnPlacement.Y;
                this.queries.Placements.Add(requestQueryBP);
            }
        }

        public void ConstructV2()
        {
            for (int i = 0; i < responses.Placements.Count; i++)
            {
                if (responses.Placements[i].Result == ActionResult.Success)
                {
                    if (queries.Placements[i].AbilityId == Abilities.BUILD_BARRACKS || queries.Placements[i].AbilityId == Abilities.BUILD_BARRACKS | queries.Placements[i].AbilityId == Abilities.BUILD_STARPORT || queries.Placements[i].AbilityId == Abilities.BUILD_FACTORY)
                    {
                        if (responses.Placements[i + 1].Result == ActionResult.Success)
                        {
                            var constructAction = CreateRawUnitCommand(queries.Placements[i].AbilityId);
                            constructAction.ActionRaw.UnitCommand.UnitTags.Add(queries.Placements[i].PlacingUnitTag);
                            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
                            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.X = queries.Placements[i].TargetPos.X;
                            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = queries.Placements[i].TargetPos.Y;
                            AddAction(constructAction);
                        }
                        i++;
                    }
                    else
                    {
                        var constructAction = CreateRawUnitCommand(queries.Placements[i].AbilityId);
                        constructAction.ActionRaw.UnitCommand.UnitTags.Add(queries.Placements[i].PlacingUnitTag);
                        constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
                        constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.X = queries.Placements[i].TargetPos.X;
                        constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = queries.Placements[i].TargetPos.Y;
                        AddAction(constructAction);
                    }
                }
                else if (queries.Placements[i].AbilityId == Abilities.BUILD_BARRACKS || queries.Placements[i].AbilityId == Abilities.BUILD_BARRACKS | queries.Placements[i].AbilityId == Abilities.BUILD_STARPORT || queries.Placements[i].AbilityId == Abilities.BUILD_FACTORY)
                {
                    i++;
                }
            }
        }

        public int ResolvePathings()
        {
            float minDistance = float.MaxValue;
            int index = 0;

            for (int i = 0; i < responses.Pathing.Count; i++)
            {
                if (responses.Pathing[i].Distance == 0)
                    continue;
                if(responses.Pathing[i].Distance < minDistance)
                {
                    minDistance = responses.Pathing[i].Distance;
                    index = i;
                }
            }
            return index;
        }

        public void Construct(uint unitType)
        {
            var worker = GetAvailableWorker();
            if (worker == null) return;

            Vector3 startingSpot;
            Unit constructionTarget = new Unit();
            if (units.resourceCenters.Count > 0)
            {
                var cc = units.resourceCenters[0];
                startingSpot = GetPosition(cc);
            }
            else
                startingSpot = GetPosition(worker);

            var radius = 12;
            var innerRadius = 0;

            //trying to find a valid construction spot
            Vector3 constructionSpot = new Vector3();
            if (unitType == Units.COMMAND_CENTER)
            {
                RequestQueryPathing closestPathing = queries.Pathing[ResolvePathings()];
                constructionSpot = new Vector3 (closestPathing.EndPos.X, closestPathing.EndPos.Y, zVal);
                foreach (VectorWrapper expandLocation in expandLocations)
                {
                    if (expandLocation.vector.X == constructionSpot.X && expandLocation.vector.Y == constructionSpot.Y)
                        defendPoint = expandLocation.vectorAdditional;
                }
                //constructionSpot =  FindCommandCenterLocation(closestMineralField);
            }
            else if (unitType == Units.REFINERY)
            {
                constructionTarget = FindVespeneGeyser();
            }
            else
            {
                while (true)
                {
                    constructionSpot = new Vector3(startingSpot.X + innerRadius + random.Next(-radius, radius + 1), startingSpot.Y + innerRadius + random.Next(-radius, radius + 1), worker.Pos.Z);
                    bool valid = true;

                    
                    foreach (Unit mineralField in units.mineralFields)
                    {
                        if (GetDistance(mineralField, constructionSpot) <= 7)
                        {
                            valid = false;
                            break;
                        }
                    }
                    foreach (Unit building in units.buildings)
                    {
                        if (GetDistance(building, constructionSpot) <= 5)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (unitType == Units.BARRACKS)
                    { 
                    }

                        if (valid) break;
                }
            }


            var constructAction = CreateRawUnitCommand(Abilities.FromBuilding[unitType]);
            constructAction.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            if (constructionTarget.Tag == 0)
            {
                if (unitType == Units.REFINERY)
                    return;
                constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
                constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.X = constructionSpot.X;
                constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = constructionSpot.Y;
            }
            else
            {
                constructAction.ActionRaw.UnitCommand.TargetUnitTag = constructionTarget.Tag;
            }
            AddAction(constructAction);

            //var mf = GetMineralField();
            //if (mf != null)
            //{
            //    var returnAction = CreateRawUnitCommand(Abilities.SMART);
            //    returnAction.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            //    returnAction.ActionRaw.UnitCommand.TargetUnitTag = mf.Tag;
            //    returnAction.ActionRaw.UnitCommand.QueueCommand = true;
            //    AddAction(returnAction);
            //}

            Logger.Info("Attempting to construct: {0} @ {1} / {2}", unitType.ToString(), constructionSpot.X, constructionSpot.Y);
        }

        public void EstablishCCLocations()
        {
            foreach (Unit mineralField in units.mineralFields)
            {
                expandLocations.Add(FindCommandCenterLocation(GetPosition(mineralField)));
            }
        }

        private VectorWrapper FindCommandCenterLocation(Vector3 closestMineralFieldPosition)
        {
           
            var mineralFieldsInMiningArea = new List<Unit>();
            foreach (Unit mineralField in units.mineralFields)
            {
                if (GetDistance(mineralField, closestMineralFieldPosition) < 15)
                    mineralFieldsInMiningArea.Add(mineralField);
            }

            //Find endpoint mineral patches
            var mineralDistances = new List<MineralDistanceWrapper>();
            double totalDistanceMax = 0;
            Unit furthestComDistMinFld = new Unit();
            foreach (Unit mineralField in mineralFieldsInMiningArea)
            {
                double totalDistance = 0;
                foreach (Unit mineralFieldInnerGuy in mineralFieldsInMiningArea)
                {
                    mineralDistances.Add(new MineralDistanceWrapper(mineralField, mineralFieldInnerGuy, GetDistance(mineralField, mineralFieldInnerGuy)));
                    totalDistance += GetDistance(mineralField, mineralFieldInnerGuy);
                }
                if (totalDistance >= totalDistanceMax)
                {
                    totalDistanceMax = totalDistance;
                    furthestComDistMinFld = mineralField;
                }
            }

            mineralDistances.Sort();
            mineralDistances.Reverse();

            Vector3 leftSidePosition = GetPosition(mineralDistances[4].leftMineralField);
            Vector3 rightSidePosition = GetPosition(mineralDistances[4].rightMineralField);
            if (mineralDistances[5].leftMineralField == furthestComDistMinFld ||
                mineralDistances[5].rightMineralField == furthestComDistMinFld
                )
            {
                leftSidePosition = GetPosition(mineralDistances[5].leftMineralField);
                rightSidePosition = GetPosition(mineralDistances[5].rightMineralField);

            }
            else if (mineralDistances[6].leftMineralField == furthestComDistMinFld ||
                mineralDistances[6].rightMineralField == furthestComDistMinFld
                )
            {
                leftSidePosition = GetPosition(mineralDistances[6].leftMineralField);
                rightSidePosition = GetPosition(mineralDistances[6].rightMineralField);

            }

            Vector3 midpointOfMineralPatchEnpoints = leftSidePosition + rightSidePosition;
            midpointOfMineralPatchEnpoints /= 2;

            float slopeOfLine = (leftSidePosition.Y - rightSidePosition.Y) / (leftSidePosition.X - rightSidePosition.X);
            slopeOfLine = 1 / slopeOfLine;
            Vector3 perpendicularLine = new Vector3(slopeOfLine, slopeOfLine, slopeOfLine);

            //if(slopeOfLine >= 1)
            //    perpendicularLine.Y *= -1;

            Vector3 mineralFieldGoodPosition = new Vector3();

            bool acx = leftSidePosition.X > midpointOfMineralPatchEnpoints.X;
            bool acy = leftSidePosition.Y > midpointOfMineralPatchEnpoints.Y;

            foreach (Unit mineralField in mineralFieldsInMiningArea)
            {
                Vector3 mineralFieldLocation = GetPosition(mineralField);
                if (!acx == acy)
                {
                    if (((mineralFieldLocation.X >= midpointOfMineralPatchEnpoints.X) &&
                        (mineralFieldLocation.Y <= midpointOfMineralPatchEnpoints.Y))
                        ||
                        ((mineralFieldLocation.X <= midpointOfMineralPatchEnpoints.X) &&
                        (mineralFieldLocation.Y >= midpointOfMineralPatchEnpoints.Y))
                        )
                    {
                        continue;
                    }
                }
                else
                {
                    if (((mineralFieldLocation.X >= midpointOfMineralPatchEnpoints.X) &&
                        (mineralFieldLocation.Y >= midpointOfMineralPatchEnpoints.Y))
                        ||
                        ((mineralFieldLocation.X <= midpointOfMineralPatchEnpoints.X) &&
                        (mineralFieldLocation.Y <= midpointOfMineralPatchEnpoints.Y))
                        )
                    {
                        continue;
                    }
                }
                if (mineralFieldLocation.X == rightSidePosition.X ||
                        mineralFieldLocation.Y == rightSidePosition.Y ||
                        mineralFieldLocation.X == leftSidePosition.X ||
                        mineralFieldLocation.Y == leftSidePosition.X
                    )
                    continue;
                mineralFieldGoodPosition = GetPosition(mineralField);
                break;
            }

            //fix flat? idk
            if (slopeOfLine > 7 || Math.Abs(slopeOfLine) < 0.2f)
                perpendicularLine = new Vector3(1, 0, 0);

            if (mineralFieldGoodPosition.X >= midpointOfMineralPatchEnpoints.X)
            {
                perpendicularLine.X = Math.Abs(perpendicularLine.X) * -1; // go left because we are to the right
            }
            else
            {
                perpendicularLine.X = Math.Abs(perpendicularLine.X);
            }
            if (mineralFieldGoodPosition.Y >= midpointOfMineralPatchEnpoints.Y)
            {
                perpendicularLine.Y = Math.Abs(perpendicularLine.Y) * -1; // go down because we are above
            }
            else
            {
                perpendicularLine.Y = Math.Abs(perpendicularLine.Y);
            }

            Vector3 FinalVector;
            if (slopeOfLine > 7 || Math.Abs(slopeOfLine) < 0.2f)
                FinalVector = midpointOfMineralPatchEnpoints + (Vector3.Normalize(perpendicularLine) * 6f);
            else
            {
                FinalVector = midpointOfMineralPatchEnpoints + (Vector3.Normalize(perpendicularLine) * 5.5f);
                if (midpointOfMineralPatchEnpoints.Y > FinalVector.Y)
                    FinalVector.Y += 0.5f;
                else
                    FinalVector.Y -= 0.5f;
            }
            defendPoint = midpointOfMineralPatchEnpoints + (Vector3.Normalize(perpendicularLine) * 15f);
            defendPoint.Z = zVal;

            VectorWrapper returnValue = new VectorWrapper(FinalVector, defendPoint);

            return returnValue;

        }

        private Unit FindVespeneGeyser()
        {
            foreach (Unit CommandCenter in units.resourceCenters)
            {
                foreach (Unit vespeneGeyser in units.vespeneGeysers)
                {
                    if (GetDistance(CommandCenter, vespeneGeyser) < 10)
                        if (!vespeneGeyser.IsPowered)
                            return vespeneGeyser;
                }
            }
            return new Unit();
        }

        public bool CanConstruct(uint buildingType)
        {
            if (units.workers.Count == 0) return false;

            if (buildingType != Units.SUPPLY_DEPOT)
            {
                foreach (Unit unit in units.workers)
                {
                    if (GetUnitOrders(unit) == Abilities.FromBuilding[buildingType])
                        return false;
                }
            }

            // if we don't have a CC...make one
            if (units.resourceCenters.Count == 0 && buildingType != Units.COMMAND_CENTER)
                return false;

            if (buildingType == Units.SUPPLY_DEPOT)
            {
                int pendingSupplyDepots = 0;
                foreach(Unit worker in units.workers)
                {
                    if (GetUnitOrders(worker) == Abilities.BUILD_SUPPLY_DEPOT)
                        pendingSupplyDepots++;
                }
                foreach (Unit building in units.buildings)
                {
                    if (building.UnitType == Units.SUPPLY_DEPOT && building.BuildProgress < 1)
                        pendingSupplyDepots++;
                }
                if (maxSupply - currentSupply <= 5 && pendingSupplyDepots == 0)
                    return (minerals >= 100);
                else if (maxSupply - currentSupply <= 3 && pendingSupplyDepots <= 1 && currentSupply > 15)
                    return (minerals >= 100);
                else if (maxSupply - currentSupply <= 1 && pendingSupplyDepots <= 2 && currentSupply > 15)
                    return (minerals >= 100);
                else
                    return
                        false;
            }

            if (buildingType == Units.BARRACKS)
            {

                if (units.barracks.Count > 13)
                    return false;
                //don't go crazy.

                if (units.barracks.Count / 2f < units.resourceCenters.Count)
                    return (minerals >= 150);
                

            }

            if (buildingType == Units.FACTORY)
            {

                if (units.barracks.Count > 1 && units.factories.Count == 0)
                    return (minerals >= 150);
            }

            if (buildingType == Units.STARPORT)
            {
                if (units.factories.Count == 0)
                    return false;
                if (units.starports.Count > 3)
                    return false;
                if (units.starports.Count < units.resourceCenters.Count / 2)
                    return (minerals >= 150);
            }

            //go crazy
            if (buildingType == Units.COMMAND_CENTER)
                return (minerals >= 400);

            

            if (buildingType == Units.REFINERY)
            {

                if (currentSupply > (units.refineries.Count * 20) + 15)
                {
                    return (minerals >= 75);
                }
                else
                    return false;
            }


            return false;
        }

    }
}