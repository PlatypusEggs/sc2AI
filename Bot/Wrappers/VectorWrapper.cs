using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Bot.Wrappers
{
    public class VectorWrapper
    {
        public VectorWrapper(Vector3 vector, Vector3 vectorAdditional)
        {
            this.vector = vector;
            this.vectorAdditional = vectorAdditional;
        }

        public Vector3 vector { get; set; }
        public Vector3 vectorAdditional { get; set; }

    }
}
    