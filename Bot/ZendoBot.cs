using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using System.Xml.Schema;
using Google.Protobuf.Collections;
using Microsoft.Win32;
using SC2APIProtocol;
using Action = System.Action;
using System.Numerics;
using Bot.Wrappers;

namespace Bot
{
    class ZendoBot : Bot
    {        
        private static Random random = new Random(GenerateSeed());       
        private Controller controller = new Controller(0);

        private ResponseGameInfo gameInfo;
        private ResponseData gameData;

        public ZendoBot(ResponseGameInfo _gameInfo, ResponseData _gameData)
        {
            gameInfo = _gameInfo;
            gameData = _gameData;
        }

        private static int GenerateSeed() {
            var currentDayOfYear = DateTime.Now.DayOfYear;
            var currentMinute = DateTime.Now.Minute;
            var seed = currentDayOfYear * 1000 + (currentMinute % 3);
            
            return seed;
        }



        public void OnStart(ResponseGameInfo gameInfo, ResponseObservation obs, uint playerId)
        {
            Logger.Info("GAME STARTED");
        }

        public void OnEnd(ResponseGameInfo gameInfo, ResponseObservation obs, uint playerId, Result result)
        {
            Logger.Info("GAME ENDED");
            Logger.Info("Result: {0}", result);
        }


        public List<SC2APIProtocol.Action> OnFrame(ResponseObservation obs, uint playerId, ResponseQuery responses)
        {

            bool closeToSupplyBlocked = false;
            bool needToExpand = false;
            bool needToResearch = false;
            //TODO

            // build tech up to medivacs
            // micro things like pull back if you are alone 
            // or pull back if you have low hp and others around you have more

            // build correct natural but pathing is hard ;_;

            controller.OpenFrame(gameInfo, obs, responses);

            if (controller.frame == 0) {
                Logger.Info("ZendoBot");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", gameInfo.MapName);
                Logger.Info("--------------------------------------");
            }

            if (controller.frame == controller.SecsToFrames(1)) {
                controller.EstablishCCLocations();
                controller.Chat("gl hf");
            }

            if ((controller.units.buildings.Count == 1) && (controller.units.buildings[0].Health <= controller.units.buildings[0].HealthMax * 0.35)) {
                if (!controller.chatLog.Contains("gg"))
                    controller.Chat("gg");                
            }
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //watch.Stop();
            //Logger.Info("Building supply depots" + watch.ElapsedMilliseconds.ToString());

            //keep on buildings depots if supply is tight
            if (controller.maxSupply - controller.currentSupply <= 5 && controller.currentSupply != controller.maxSupply)
            {
                closeToSupplyBlocked = true;
                if (controller.CanConstruct(Units.SUPPLY_DEPOT))
                {
                    controller.Construct(Units.SUPPLY_DEPOT);
                }
            }


            // if oversaturated
            if (controller.units.resourceCenters.Count == 0)
            {
                needToExpand = true;
            }
            else if (controller.units.resourceCenters.Count == 1)
            {
                if ((controller.units.resourceCenters[0].AssignedHarvesters + 1) >= controller.units.resourceCenters[0].IdealHarvesters)
                needToExpand = true;
            }
            else
            {
                foreach (Unit commandCenter in controller.units.resourceCenters)
                {
                    if ((commandCenter.AssignedHarvesters - 3) >= commandCenter.IdealHarvesters && commandCenter.IdealHarvesters > 0)
                    {
                        needToExpand = true;
                    }
                    else if (commandCenter.BuildProgress < 1)
                    {
                        needToExpand = false;
                        break;
                    }
                }
            }

            //build Command Center
            if (controller.CanConstruct(Units.COMMAND_CENTER))
            {
                controller.Construct(Units.COMMAND_CENTER);
            }

            //set rally points
            controller.setBuildingCCRallyPoints();

            //populate refineries and check for over saturation
            if(controller.frame % 60 == 0)
            {
                controller.DealWithOverSaturation(controller.units.resourceCenters);
                controller.DealWithOverSaturation(controller.units.refineries);
                controller.PopulateRefineries();
                controller.DealWithLazyWorkers();
            }

            if (!needToExpand)
            {
                needToResearch = controller.researchTech();
                controller.ClotheNakedBarracks();
                
                //train workers
                foreach (var cc in controller.units.resourceCenters)
                {
                    if (controller.units.workers.Count < 60 && (controller.currentSupply != 13 || controller.units.buildings.Count != 1))
                        controller.TrainWorker(cc);

                }

                controller.ConstructV2();

                //build Refineieierries
                if (controller.CanConstruct(Units.REFINERY))
                {
                    controller.Construct(Units.REFINERY);
                }

                //train barracks units
                if (!closeToSupplyBlocked && !needToResearch)
                {
                    foreach (var barracks in controller.units.barracks)
                    {
                        if (barracks.AddOnTag == 0)
                            controller.TrainMarine(barracks);
                        else
                        {
                            bool istechLab = false;
                            //todo fix this
                            foreach (Unit techLab in controller.units.techLabs)
                            {
                                if (barracks.AddOnTag == techLab.Tag)
                                {
                                    istechLab = true;
                                    break;
                                }
                            }
                            if (istechLab)
                            {
                                controller.TrainMarauder(barracks);
                                //controller.TrainMarine(barracks);
                            }
                            else
                            {
                                controller.TrainMarine(barracks);
                            }
                        }
                    }
                }
            }


            //Army stuff

            controller.ManageArmyMovements();
            controller.ManageStim();
            controller.spendOrbitalEnergy();


            return controller.CloseFrame();
        }

        public RequestQuery PreFrame(ResponseObservation observation, uint playerId)
        {
            controller.OpenFrame(gameInfo, observation, null);
            if (controller.CanConstruct(Units.BARRACKS))
            {
                controller.ConstructQuereies(Units.BARRACKS);
            }
            if (controller.CanConstruct(Units.COMMAND_CENTER))
            {
                controller.CommandCenterPathingQueries();
            }
            return controller.ClosePreFrame();
        }
    }

    class RaxBotFactory : BotFactory
    {
        public Bot GetBot(ResponseGameInfo gameInfo, ResponseData gameData)
        {
            return new ZendoBot(gameInfo, gameData);
        }
    }
}













