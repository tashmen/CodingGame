using GameSolution.Utility;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public class BombEntity : Entity
    {
        public int SourceFactoryId
        {
            get { return Arg2; }
        }
        public int TargetFactoryId
        {
            get { return Arg3; }
        }
        public int TurnsToArrive
        {
            get { return Arg4; }
        }

        /// <summary>
        /// Creates a new Bomb Entity
        /// </summary>
        /// <param name="id">Unique Identifier</param>
        /// <param name="arg1">Owner</param>
        /// <param name="arg2">Source Factory Id</param>
        /// <param name="arg3">Target Factory Id (-1 if enemy)</param>
        /// <param name="arg4">Number of turns until bomb hits the target (-1 if enemy)</param>
        /// <param name="arg5">unused</param>
        public BombEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        : base(id, arg1, arg2, arg3, arg4, arg5)
        {

        }

        public BombEntity(BombEntity entity) : base(entity)
        {

        }
    }
}
