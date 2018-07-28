using System.Collections.Generic;

namespace Bot
{
    class Abilities
    {
        //you can get all these values from the stableid.json file (just search for it on your PC)

        public static int BUILD_COMMAND_CENTER = 318;
        public static int BUILD_SUPPLY_DEPOT = 319;
        public static int BUILD_REFINERY = 320;
        public static int BUILD_BARRACKS = 321;
        public static int BUILD_BUNKER = 324;
        public static int BUILD_FACTORY = 328;
        public static int BUILD_STARPORT = 329; 


        public static int BUILD_TECH_LAB_BARRACKS = 421;
        public static int BUILD_REACTOR_BARRACKS = 422;

        public static int BUILD_TECH_LAB_STARPORT = 3682;
        public static int BUILD_REACTOR_STARPORT = 3683;


        public static int TRAIN_SCV = 524;

        public static int TRAIN_MARINE = 560;
        public static int TRAIN_MARAUDER = 563;

        public static int TRAIN_MEDIVAC = 620;



        public static int CANCEL_CONSTRUCTION = 314;       
        public static int CANCEL = 3659;
        public static int CANCEL_LAST = 3671;
        public static int LIFT = 3679;
        public static int LAND = 3678;
        
        public static int SMART = 1;
        public static int STOP = 4;        
        public static int ATTACK = 3674;
        public static int MOVE = 16;        
        public static int PATROL = 17;
        public static int RALLY = 3673;

        public static int REPAIR = 3685;

        public static int DEPOT_RAISE = 558;
        public static int DEPOT_LOWER = 556;
        
        //gathering/returning minerals
        public static int GATHER_MINERALS = 295;
        public static int RETURN_MINERALS = 296;


        //tech
        public static int RESEARCH_STIM = 730;
        public static int RESEARCH_COMBAT_SHIELDS = 731;
        public static int RESEARCH_CONCUSSIVE_SHELLS = 732;
        

        //unit abilities
        public static int STIM_MARAUDER = 253;
        public static int STIM_MARINE = 380;
        public static int CALLDOWN_MULE = 171;

        //building upgrades
        public static int UPGRADE_TO_ORBITAL = 1516;
        //public static int CANCEL_QUEUE = 3659;




        public static readonly Dictionary<uint, int> FromBuilding = new Dictionary<uint, int>()
        {
            { Units.SUPPLY_DEPOT, BUILD_SUPPLY_DEPOT },
            { Units.BARRACKS, BUILD_BARRACKS },
            { Units.BUNKER, BUILD_BUNKER },
            { Units.COMMAND_CENTER, BUILD_COMMAND_CENTER },
            { Units.REFINERY, BUILD_REFINERY },
            { Units.FACTORY, BUILD_FACTORY},
            { Units.STARPORT, BUILD_STARPORT }
        };
        
    }
}
