﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution
{
    public class FactoryEntity : Entity
    {
        public int CyborgCount
        {
            get { return Arg2; }
        }
        public int ProductionCount
        {
            get { return Arg3; }
        }

        public int TurnsTillProduction
        {
            get { return Arg4; }
        }

        public FactoryEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        : base(id, arg1, arg2, arg3, arg4, arg5)
        {

        }

        public bool IsProducing()
        {
            return TurnsTillProduction == 0;
        }
    }
}
