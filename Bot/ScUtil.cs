using Bot;
using Bot.Wrappers;
using SC2APIProtocol;
using System;
using System.Collections.Generic;

public static class ScUtil
{
	private static readonly Dictionary<uint, double> UNIT_VALUES = new Dictionary<uint, double>()
	{
		{ Units.AUTO_TURRET, 0.1},
		{ Units.BANELING, 3 },
		{ Units.BANELING_BURROWED, 3 },
		{ Units.BANELING_COCOON, 1 },
		{ Units.BANSHEE, 4 },
		{ Units.BATTLECRUISER, 5 },
		{ Units.BROOD_LORD, 5 },
		{ Units.BROOD_LORD_COCOON, 1 },
		{ Units.CARRIER, 7 },
		{ Units.COLOSSUS, 6 },
		{ Units.CORRUPTOR, 3 },
		{ Units.DARK_TEMPLAR, 4 },
		{ Units.GHOST, 4 },
		{ Units.HELLION, 3 },
		{ Units.HIGH_TEMPLAR, 4 },
		{ Units.HYDRALISK, 4 },
		{ Units.HYDRALISK_BURROWED, 4 },
		{ Units.IMMORTAL, 5 },
		{ Units.INFESTED_TERRANS_EGG, 1 },
		{ Units.INFESTOR_BURROWED, 1 },
		{ Units.INFESTOR_TERRAN, 1 },
		{ Units.INFESTOR_TERRAN_BURROWED, 1 },
		{ Units.MARAUDER, 2 },
		{ Units.MARINE, 1 },
		{ Units.MEDIVAC, 2 },
		{ Units.MOTHERSHIP, 5 },
		{ Units.MUTALISK, 3 },
		{ Units.PHOENIX, 3 },
		{ Units.QUEEN, 2 },
		{ Units.QUEEN_BURROWED, 2 },
		{ Units.RAVEN, 6 },
		{ Units.REAPER, 3 },
		{ Units.ROACH, 3 },
		{ Units.ROACH_BURROWED, 3 },
		{ Units.SENTRY, 1 },
		{ Units.SIEGE_TANK, 4 },
		{ Units.SIEGE_TANK_SIEGED, 4 },
		{ Units.STALKER, 3 },
		{ Units.THOR, 6 },
		{ Units.ULTRALISK, 5 },
		{ Units.URSADON, 0.1 },
		{ Units.VIKING_ASSUALT, 3 },
		{ Units.VIKING_FIGHTER, 3 },
		{ Units.VOID_RAY, 6 },
		{ Units.ZEALOT, 2 },
		{ Units.ZERGLING, 1 },
		{ Units.ZERGLING_BURROWED, 1 }
	};

    public static double ArmyComparison(List<UnitWrapper> army1, List<UnitWrapper> army2)
    {
        double army1Value = getArmyValue(army1);
        double army2Value = getArmyValue(army2);
        return army1Value / army2Value;
    }

    public static double ArmyComparison(List<Unit> army1, List<Unit> army2)
	{
		double army1Value = getArmyValue(army1);
		double army2Value = getArmyValue(army2);
		return army1Value / army2Value;
	}

    public static double getArmyValue(List<UnitWrapper> army)
    {
        double value = 0;
        army.ForEach((u) =>
        {
            value += getUnitValue(u);
        });
        return value;
    }

    public static double getArmyValue(List<Unit> army)
	{
		double value = 0;
		army.ForEach((u) =>
		{
			value += getUnitValue(u);
		});
		return value;
	}

    public static double getUnitValue(UnitWrapper unit)
    {
        return getUnitValue(unit.unit);
    }

    public static double getUnitValue(Unit unit)
    {
        UNIT_VALUES.TryGetValue(unit.UnitType, out double unitValue);
        if (unitValue == 0)
        {
            unitValue = 5;
        }

        return unitValue;
    }
}
