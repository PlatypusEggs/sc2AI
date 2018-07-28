﻿using System;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace Bot
{
    class Program
    {
        // Settings for your bot.
        private static BotFactory botFactory = new RaxBotFactory();
        private static Race race = Race.Terran;

        // Settings for single player mode.
        //        private static string mapName = "AbyssalReefLE.SC2Map";
        //        private static string mapName = "AbiogenesisLE.SC2Map";

        //private static string mapName = "(2)DreamcatcherLE.SC2Map";
        //private static string mapName = "(2)LostandFoundLE.SC2Map";
        private static string mapName = "(2)AcidPlantLE.SC2Map";
        //        private static string mapName = "FrostLE.SC2Map";
        private static Race opponentRace = Race.Zerg;
        //rarely beat harder
        private static Difficulty opponentDifficulty = Difficulty.Harder;

        public static GameConnection gc = null;
        
        static void Main(string[] args)
        {
            try {
                gc = new GameConnection();
                if (args.Length == 0)
                    gc.RunSinglePlayer(botFactory, mapName, race, opponentRace, opponentDifficulty).Wait();
                else {
                    gc.RunLadder(botFactory, race, args).Wait();
                }
            }
            catch (Exception ex) {
                Logger.Info(ex.ToString());
            }
            
            Logger.Info("Terminated.");
        }
    }
}
