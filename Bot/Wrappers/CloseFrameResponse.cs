using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Bot.Wrappers
{
    public class CloseFrameResponse
    {

        public List<SC2APIProtocol.Action> actions { get; set; }
        public RequestQuery queries { get; set; }

    }
}
    