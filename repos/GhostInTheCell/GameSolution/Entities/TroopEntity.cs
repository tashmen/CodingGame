﻿namespace GameSolution.Entities
{
    public class TroopEntity : Entity
    {
        public int SourceFactory
        {
            get { return Arg2; }
        }

        public int TargetFactory
        {
            get { return Arg3; }
        }

        public int NumberOfCyborgs
        {
            get { return Arg4; }
        }

        public int TurnsToArrive
        {
            get { return Arg5; }
        }


        public TroopEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        : base(id, arg1, arg2, arg3, arg4, arg5)
        {

        }

        public TroopEntity(TroopEntity entity) : base(entity)
        {

        }
    }
}