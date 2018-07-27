using System;
using System.Collections.Generic;
using System.Text;
using SC2APIProtocol;
using Bot.Wrappers;

namespace Bot
{
    public interface Bot
    {
        List<SC2APIProtocol.Action> OnFrame(ResponseObservation observation, uint playerId, ResponseQuery responses);

        SC2APIProtocol.RequestQuery PreFrame(ResponseObservation observation, uint playerId);
    }

    public interface BotFactory
    {
        Bot GetBot(ResponseGameInfo gameInfo, ResponseData gameData);
    }
}
