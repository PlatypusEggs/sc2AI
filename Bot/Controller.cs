using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Schema;
using Google.Protobuf.Reflection;
using SC2APIProtocol;
using Bot.Wrappers;
using System.Collections;

namespace Bot {

    public partial class Controller {
        //editable
        private int frameDelay = 0;

        //don't edit
        private ResponseObservation obs;
        private List<SC2APIProtocol.Action> actions;
        private SC2APIProtocol.RequestQuery queries;
        private SC2APIProtocol.ResponseQuery responses;
        private ResponseGameInfo gameInfo;
        private static Random random = new Random();
        private double FRAMES_PER_SECOND = 22.4;

        public ulong frame = 0;
        public uint currentSupply = 0;
        public uint maxSupply = 0;
        public uint minerals = 0;
        public uint vespene = 0;

        public List<Vector3> enemyLocations = new List<Vector3>();
        public List<string> chatLog = new List<string>();

        public HashSet<VectorWrapper> expandLocations = new HashSet<VectorWrapper>();

        public class UnitsHolder {
            public List<Unit> workers = new List<Unit>();
            public List<UnitWrapper> army = new List<UnitWrapper>();
            public List<Unit> armySupport = new List<Unit>();
            public List<Unit> barracks = new List<Unit>();
            public List<Unit> depots = new List<Unit>();
            public List<Unit> buildings = new List<Unit>();
            public List<Unit> resourceCenters = new List<Unit>();
            public List<Unit> mineralFields = new List<Unit>();
            public List<Unit> vespeneGeysers = new List<Unit>();
            public List<Unit> refineries = new List<Unit>();
            public List<Unit> reactors = new List<Unit>();
            public List<Unit> factories = new List<Unit>();
            public List<Unit> starports = new List<Unit>();
            public List<Unit> techLabs = new List<Unit>();
        }


        public List<UnitWrapper> mariners = new List<UnitWrapper>();
        public List<Unit> medivacs = new List<Unit>();
        public List<UnitWrapper> marauders = new List<UnitWrapper>();

        public UnitsHolder units = new UnitsHolder();
        public UnitsHolder enemyUnits = new UnitsHolder();
        private Vector3 defendPoint = new Vector3();
        public List<int> researchedTech = new List<int>();
        public float zVal = 0;

        public Controller(int wait) {
            Logger.Info("Instantiated Controller");
            this.frameDelay = wait;
        }

        public void Pause() {
            Console.WriteLine("Press any key to continue...");
            while (Console.ReadKey().Key != ConsoleKey.Enter) {
                //do nothing
            }
        }

        #region probably don't touch
        public List<SC2APIProtocol.Action> CloseFrame() {
            queries = null;
            CloseFrameResponse a = new CloseFrameResponse();
            a.actions = actions;
            a.queries = queries;
            return actions;
        }
        public SC2APIProtocol.RequestQuery ClosePreFrame()
        {
            return queries;
        }
        public ulong SecsToFrames(int seconds) {
            return (ulong)(FRAMES_PER_SECOND * seconds);
        }
        public void OpenFrame(ResponseGameInfo gameInfo, ResponseObservation obs, ResponseQuery responses) {
            this.obs = obs;
            this.gameInfo = gameInfo;
            this.responses = responses;
            this.actions = new List<SC2APIProtocol.Action>();
            if(this.queries == null)
                this.queries = new SC2APIProtocol.RequestQuery();

            if (obs == null) {
                Logger.Info("ResponseObservation is null! The application will terminate.");
                Pause();
                Environment.Exit(0);
            }

            foreach (var chat in obs.Chat) {
                this.chatLog.Add(chat.Message);
            }

            this.frame = obs.Observation.GameLoop;
            this.currentSupply = obs.Observation.PlayerCommon.FoodUsed;
            this.maxSupply = obs.Observation.PlayerCommon.FoodCap;
            this.minerals = obs.Observation.PlayerCommon.Minerals;
            this.vespene = obs.Observation.PlayerCommon.Vespene;

            

            PopulateInventory();
            PopulateEnemyInventory();

            zVal = GetPosition(units.resourceCenters[0]).Z;

            if (frame == 0) {
                var rcPosition = GetPosition(units.resourceCenters[0]);
                foreach (var startLocation in gameInfo.StartRaw.StartLocations) {
                    var enemyLocation = new Vector3(startLocation.X, startLocation.Y, 0);
                    var distance = GetDistance(enemyLocation, rcPosition);
                    if (distance > 30)
                        enemyLocations.Add(enemyLocation);
                }
            }

            if (frameDelay > 0)
                System.Threading.Thread.Sleep(frameDelay);

        }
        public void AddAction(SC2APIProtocol.Action action) {
            actions.Add(action);
        }
        #endregion

        private void PopulateInventory()
        {
            this.units = new UnitsHolder();
            foreach (Unit unit in obs.Observation.RawData.Units)
            {
                if (unit.Alliance != Alliance.Self) continue;
                //if (Units.ArmyUnits.Contains(unit.UnitType))
                //    this.units.army.Add(new UnitWrapper(unit));

                if (Units.Workers.Contains(unit.UnitType))
                    this.units.workers.Add(unit);

                if (Units.Buildings.Contains(unit.UnitType))
                    this.units.buildings.Add(unit);

                if (Units.ResourceCenters.Contains(unit.UnitType))
                    this.units.resourceCenters.Add(unit);

                if ((unit.UnitType == Units.SUPPLY_DEPOT) || (unit.UnitType == Units.SUPPLY_DEPOT_LOWERED))
                    this.units.depots.Add(unit);

                if ((unit.UnitType == Units.BARRACKS) || (unit.UnitType == Units.BARRACKS_FLYING))
                    this.units.barracks.Add(unit);

                if (unit.UnitType == Units.FACTORY)
                    this.units.factories.Add(unit);

                if (unit.UnitType == Units.STARPORT)
                    this.units.starports.Add(unit);

                if (unit.UnitType == Units.REFINERY)
                    this.units.refineries.Add(unit);

                if (unit.UnitType == Units.BARRACKS_REACTOR)
                    this.units.reactors.Add(unit);

                if (unit.UnitType == Units.BARRACKS_TECH_LAB)
                    this.units.techLabs.Add(unit);


                //These are maintained and never reset
                if (unit.UnitType == Units.MARINE && !mariners.Exists(x => x.unit.Tag == unit.Tag))
                    mariners.Add(new UnitWrapper(unit));

                if (unit.UnitType == Units.MARAUDER && !marauders.Exists(x => x.unit.Tag == unit.Tag))
                    marauders.Add(new UnitWrapper(unit));

                if (unit.UnitType == Units.MEDIVAC && !medivacs.Exists(x => x.Tag == unit.Tag))
                    medivacs.Add(new Unit(unit));

            }
            List<Unit> visibleUnits = new List<Unit>(obs.Observation.RawData.Units);

            // maintain state

            List<UnitWrapper> tempMarauders = new List<UnitWrapper>();
            MaintainUnitWrapperList(marauders, visibleUnits, Units.MARAUDER);
            MaintainUnitWrapperList(mariners, visibleUnits, Units.MARINE);
            //MaintainUnitWrapperList(medivacs, visibleUnits, Units.MEDIVAC);
            // copy over state values?


            //these are reset every frame
            this.units.army.AddRange(mariners);
            this.units.army.AddRange(marauders);
            this.units.armySupport.AddRange(medivacs);

            this.units.mineralFields = GetUnits(Units.MineralFields, alliance: Alliance.Neutral);
            this.units.vespeneGeysers = GetUnits(Units.VESPENE_GEYSER, alliance: Alliance.Neutral);
            this.units.vespeneGeysers.AddRange(GetUnits(Units.SPACE_PLATFORM_GEYSER, alliance: Alliance.Neutral));

        }

        private void MaintainUnitWrapperList(List<UnitWrapper> units, List<Unit> visibleUnits, uint unitType)
        {
            List<UnitWrapper> toDelete = new List<UnitWrapper>();
            foreach (UnitWrapper unit in units)
            {
                Unit foundUnit = visibleUnits.Find(x => x.Tag == unit.unit.Tag);
                if (foundUnit == null)
                {
                    toDelete.Add(unit);
                }
                else
                {
                    unit.unit = foundUnit;
                }
            }

            foreach (UnitWrapper delete in toDelete)
            {
                units.Remove(delete);
            }
        }

        private void PopulateEnemyInventory()
        {
            this.enemyUnits = new UnitsHolder();
            foreach (Unit unit in obs.Observation.RawData.Units)
            {
                if (unit.Alliance != Alliance.Enemy) continue;
                if (Units.ArmyUnits.Contains(unit.UnitType))
                    this.enemyUnits.army.Add(new UnitWrapper(unit));

                if (Units.Workers.Contains(unit.UnitType))
                    this.enemyUnits.workers.Add(unit);

                if (Units.Buildings.Contains(unit.UnitType))
                    this.enemyUnits.buildings.Add(unit);

                if (Units.ResourceCenters.Contains(unit.UnitType))
                    this.enemyUnits.resourceCenters.Add(unit);

                if ((unit.UnitType == Units.SUPPLY_DEPOT) || (unit.UnitType == Units.SUPPLY_DEPOT_LOWERED))
                    this.enemyUnits.depots.Add(unit);

                if ((unit.UnitType == Units.BARRACKS) || (unit.UnitType == Units.BARRACKS_FLYING))
                    this.enemyUnits.barracks.Add(unit);

                if (unit.UnitType == Units.REFINERY)
                    this.enemyUnits.refineries.Add(unit);
            }
        }

        public void Chat(string message, bool team = false) {
            ActionChat actionChat = new ActionChat();
            if (team)
                actionChat.Channel = ActionChat.Types.Channel.Team;
            else
                actionChat.Channel = ActionChat.Types.Channel.Broadcast;
            actionChat.Message = message;

            SC2APIProtocol.Action action = new SC2APIProtocol.Action();
            action.ActionChat = actionChat;
            AddAction(action);
        }

        #region helper functions
        public Vector3 GetPosition(Unit unit) {
            return new Vector3(unit.Pos.X, unit.Pos.Y, unit.Pos.Z);
        }

        public double GetDistance(Unit unit1, Unit unit2) {
            return Vector3.Distance(GetPosition(unit1), GetPosition(unit2));
        }

        public double GetDistance(Unit unit, Vector3 location) {
            return Vector3.Distance(GetPosition(unit), location);
        }

        public double GetDistance(Vector3 pos1, Vector3 pos2) {
            return Vector3.Distance(pos1, pos2);
        }
        
        public List<Unit> GetUnits(HashSet<uint> hashset, Alliance alliance = Alliance.Self)
        {
            List<Unit> units = new List<Unit>();
            foreach (Unit unit in obs.Observation.RawData.Units) {
                if ((hashset.Contains(unit.UnitType)) && (unit.Alliance == alliance))
                    units.Add(unit);
            }
            return units;
        }

        private List<Unit> GetUnits(uint unitType, Alliance alliance = Alliance.Self)
        {
            List<Unit> units = new List<Unit>();
            foreach (Unit unit in obs.Observation.RawData.Units) {
                if ((unit.UnitType == unitType) && (unit.Alliance == alliance))
                    units.Add(unit);
            }
            return units;
        }
        #endregion

        public Unit GetMineralField() {
            foreach (var mf in units.mineralFields) {
                foreach (var rc in units.resourceCenters) {
                    if (GetDistance(mf, rc) < 10) return mf;
                }
            }
            return null;
        }

        public void Attack(List<Unit> units, Vector3 target) {
            var action = CreateRawUnitCommand(Abilities.ATTACK);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            foreach (var unit in units)
                action.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);
            AddAction(action);
        }

        public void Attack(List<UnitWrapper> units, Vector3 target, bool melee = false)
        {
            // micro things like pull back if you are alone    
            // also pull back micro as a group if we are part of the clump?
            // or pull back if you have low hp and others around you have more
            
            foreach (UnitWrapper attackingUnit in units)
            { 
                if (GetDistance(attackingUnit.unit, target) < 3)
                {
                    continue;
                }

                SC2APIProtocol.Action action = null;

                if (frame > attackingUnit.frameStartedAttack + attackingUnit.attackingFrames + attackingUnit.movingFrames)
                {
                    attackingUnit.frameStartedAttack = frame;
                }

                if (frame < attackingUnit.frameStartedAttack + attackingUnit.attackingFrames)
                {
                    action = CreateRawUnitCommand(Abilities.ATTACK);
                }
                else
                {
                    action = CreateRawUnitCommand(Abilities.MOVE);
                    if (melee)
                    {
                        Logger.Info("we are in range of a melee unit!");
                        // to close. stutter step away!
                        if (GetDistance(attackingUnit.unit, target) <= 4)
                        {
                            //does this work?
                            Vector3 attackingUnitPosition = GetPosition(attackingUnit.unit);
                            float slope = (attackingUnitPosition.Y - target.Y) / (attackingUnitPosition.X - target.X);
                            Vector3 slopeVector = new Vector3(slope, slope, slope);
                            slopeVector = Vector3.Normalize(slopeVector);
                            Vector3 usefulVector = slopeVector * 7;

                            // FIX THIS!
                            if (attackingUnitPosition.X < target.X)
                                target.X += usefulVector.X;
                            else
                                target.X -= usefulVector.X;

                            if (attackingUnitPosition.Y < target.Y)
                                target.Y += usefulVector.Y;
                            else
                                target.Y -= usefulVector.Y;
                        }
                        else if (GetDistance(attackingUnit.unit, target) > 5)
                        {
                            // were too far
                            // continue moving toward the unit like normal
                        }
                        else
                        {
                            // don't move towrd the melee units this is the sweet spot
                            return;
                        }
                        
                    }
                    else if (GetDistance(attackingUnit.unit, target) <= 1)
                    {
                        // we are close enough!
                        return;
                    }
                }
                action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
                action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
                action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
                action.ActionRaw.UnitCommand.UnitTags.Add(attackingUnit.unit.Tag);
                AddAction(action);
            }
        }

        private SC2APIProtocol.Action CreateRawUnitCommand(int ability) {
            SC2APIProtocol.Action action = new SC2APIProtocol.Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = ability;
            return action;
        }

        private uint GetUnitOrders(Unit unit) {
            if (unit.Orders.Count == 0) return 0;
            return unit.Orders[0].AbilityId;
        }

        public Unit GetAvailableWorker() {
            foreach (Unit worker in units.workers) {
                var orders = GetUnitOrders(worker);
                if (orders == 0) return worker;

                if (orders != Abilities.GATHER_MINERALS && orders != Abilities.RETURN_MINERALS) continue;
                return worker;
            }

            return null;
        }

        private void FocusCamera(Unit unit) {
            if (unit == null) return;
            SC2APIProtocol.Action action = new SC2APIProtocol.Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.CameraMove = new ActionRawCameraMove();
            action.ActionRaw.CameraMove.CenterWorldSpace = new Point();
            action.ActionRaw.CameraMove.CenterWorldSpace.X = unit.Pos.X;
            action.ActionRaw.CameraMove.CenterWorldSpace.Y = unit.Pos.Y;
            action.ActionRaw.CameraMove.CenterWorldSpace.Z = unit.Pos.Z;
            actions.Add(action);
        }

        public void setBuildingCCRallyPoints()
        {
            foreach(Unit commandCenter in units.resourceCenters)
            {
                if(commandCenter.BuildProgress == 0)
                {
                    foreach (Unit mineralField in units.mineralFields)
                    {
                        if (GetDistance(commandCenter, mineralField) < 10)
                        {
                            var rallyAction = CreateRawUnitCommand(Abilities.SMART);
                            rallyAction.ActionRaw.UnitCommand.UnitTags.Add(commandCenter.Tag);
                            rallyAction.ActionRaw.UnitCommand.TargetUnitTag = mineralField.Tag;
                            AddAction(rallyAction);
                            break;
                        }
                    }
                }
            }
        }

    }
}