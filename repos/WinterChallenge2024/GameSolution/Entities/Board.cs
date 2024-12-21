using Algorithms.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Entities
{
    public class Board
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Dictionary<Point2d, Entity> Entities { get; set; }
        public int GlobalOrganId { get; set; } = -1;

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Entities = new Dictionary<Point2d, Entity>();
        }

        public Board(Board board)
        {
            Width = board.Width;
            Height = board.Height;
            Entities = board.Entities.ToDictionary(e => e.Key.Clone(), e => e.Value.Clone());
            GlobalOrganId = board.GlobalOrganId;
        }

        public bool Equals(Board board)
        {
            if (Width != board.Width || Height != board.Height)
                return false;

            foreach (Point2d location in Entities.Keys)
            {
                if (!board.Entities.TryGetValue(location, out Entity entity))
                    return false;
                else if (!Entities[location].Equals(entity))
                    return false;

            }

            return true;
        }

        public void ApplyMove(Move move, bool isMine)
        {
            foreach (MoveAction action in move.Actions)
            {
                switch (action.Type)
                {
                    case MoveType.GROW:
                        Entities[action.Location] = new Entity(action.Location, action.EntityType, isMine, GlobalOrganId++, action.OrganId, action.OrganRootId, OrganDirection.North);
                        break;
                }
            }
        }

        public List<Move> GetGrowMoves(bool isMine)
        {
            List<Move> moves = new List<Move>();
            var entities = GetEntities(isMine);
            foreach (Entity entity in entities)
            {
                if (IsOpenSpaceNorth(entity))
                {
                    Move move = new Move();
                    move.AddAction(MoveAction.CreateGrow(entity.OrganId, new Point2d(entity.Location.x, entity.Location.y + 1), EntityType.BASIC, entity.OrganRootId));
                    moves.Add(move);
                }
                if (IsOpenSpaceSouth(entity))
                {
                    Move move = new Move();
                    move.AddAction(MoveAction.CreateGrow(entity.OrganId, new Point2d(entity.Location.x, entity.Location.y - 1), EntityType.BASIC, entity.OrganRootId));
                    moves.Add(move);
                }

                if (IsOpenSpaceEast(entity))
                {
                    Move move = new Move();
                    move.AddAction(MoveAction.CreateGrow(entity.OrganId, new Point2d(entity.Location.x + 1, entity.Location.y), EntityType.BASIC, entity.OrganRootId));
                    moves.Add(move);
                }
                if (IsOpenSpaceWest(entity))
                {
                    Move move = new Move();
                    move.AddAction(MoveAction.CreateGrow(entity.OrganId, new Point2d(entity.Location.x - 1, entity.Location.y), EntityType.BASIC, entity.OrganRootId));
                    moves.Add(move);
                }
            }

            return moves;
        }

        public List<Move> GetHarvestMoves(bool isMine)
        {
            List<Move> moves = new List<Move>();
            List<Move> growMoves = GetGrowMoves(isMine);

            foreach (Move growMove in growMoves)
            {
                foreach (MoveAction growAction in growMove.Actions)
                {
                    if (IsHarvestSpaceNorth(growAction.Location))
                    {
                        Move move = new Move();
                        move.AddAction(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, OrganDirection.North));
                        moves.Add(move);
                    }
                    if (IsHarvestSpaceSouth(growAction.Location))
                    {
                        Move move = new Move();
                        move.AddAction(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, OrganDirection.South));
                        moves.Add(move);
                    }

                    if (IsHarvestSpaceEast(growAction.Location))
                    {
                        Move move = new Move();
                        move.AddAction(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, OrganDirection.East));
                        moves.Add(move);
                    }
                    if (IsHarvestSpaceWest(growAction.Location))
                    {
                        Move move = new Move();
                        move.AddAction(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, OrganDirection.West));
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        public bool IsHarvestSpaceNorth(Point2d location)
        {
            return location.y < Height - 1 && Entities.TryGetValue(new Point2d(location.x, location.y + 1), out Entity entityNorth) && entityNorth.IsOpenSpace();
        }

        public bool IsHarvestSpaceSouth(Point2d location)
        {
            return location.y > 0 && Entities.TryGetValue(new Point2d(location.x, location.y - 1), out Entity entitySouth) && entitySouth.IsOpenSpace();
        }

        public bool IsHarvestSpaceEast(Point2d location)
        {
            return location.x < Width - 1 && Entities.TryGetValue(new Point2d(location.x + 1, location.y), out Entity entityEast) && entityEast.IsOpenSpace();
        }

        public bool IsHarvestSpaceWest(Point2d location)
        {
            return location.x > 0 && Entities.TryGetValue(new Point2d(location.x - 1, location.y), out Entity entityWest) && entityWest.IsOpenSpace();
        }


        public bool IsOpenSpaceNorth(Entity entity)
        {
            return entity.Location.y < Height - 1 && (!Entities.TryGetValue(new Point2d(entity.Location.x, entity.Location.y + 1), out Entity entityNorth) || entityNorth.IsOpenSpace());
        }

        public bool IsOpenSpaceSouth(Entity entity)
        {
            return entity.Location.y > 0 && (!Entities.TryGetValue(new Point2d(entity.Location.x, entity.Location.y - 1), out Entity entitySouth) || entitySouth.IsOpenSpace());
        }

        public bool IsOpenSpaceEast(Entity entity)
        {
            return entity.Location.x < Width - 1 && (!Entities.TryGetValue(new Point2d(entity.Location.x + 1, entity.Location.y), out Entity entityEast) || entityEast.IsOpenSpace());
        }

        public void Harvest(bool isMine, Dictionary<EntityType, int> proteins)
        {
            var harvesters = GetEntities(isMine).Where(e => e.Type == EntityType.HARVESTER);
            foreach (Entity harvester in harvesters)
            {
                switch (harvester.OrganDirection)
                {
                    case OrganDirection.North:
                        break;
                    case OrganDirection.South:
                        break;

                }
            }
        }

        public bool IsOpenSpaceWest(Entity entity)
        {
            return entity.Location.x > 0 && (!Entities.TryGetValue(new Point2d(entity.Location.x - 1, entity.Location.y), out Entity entityWest) || entityWest.IsOpenSpace());
        }

        public Entity GetEntityByLocation(Point2d location)
        {
            Entities.TryGetValue(location, out Entity entity);
            return entity;
        }

        public List<Entity> GetEntities(bool isMine)
        {
            return Entities.Values.Where(e => e.IsMine.HasValue && e.IsMine.Value == isMine).ToList();
        }

        public Dictionary<Point2d, Entity> GetEntities()
        {
            return Entities;
        }

        public int GetMyEntityCount()
        {
            return GetEntities(true).Count();
        }

        public int GetOppEntityCount()
        {
            return GetEntities(false).Count();
        }

        public void SetEntities(IList<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                Entities[entity.Location] = entity;
                if (entity.OrganId > 0 && GlobalOrganId <= entity.OrganId)
                {
                    GlobalOrganId = entity.OrganId + 1;
                }
            }

            /*
            for(int x = 0; x< Width; x++)
            {
                for(int y = 0; y< Height; y++)
                {
                    if(!Entities.TryGetValue(new Point2d(x, y), out Entity entity) || entity.IsOpenSpace())
                    {
                        var node = new Node(x + (y * Width));
                        Graph.AddNode(node);
                        if (x < Width - 1 && (!Entities.TryGetValue(new Point2d(x + 1, y), out Entity entityEast) || entityEast.IsOpenSpace()))
                        {
                            node.AddLink(new Link(node, new Node(x + 1 + (y * Width)), 1));
                        }
                        if (x > 0 && (!Entities.TryGetValue(new Point2d(x - 1, y), out Entity entityWest) || entityWest.IsOpenSpace()))
                        {
                            node.AddLink(new Link(node, new Node(x - 1 + (y * Width)), 1));
                        }

                        if (y < Height - 1 && (!Entities.TryGetValue(new Point2d(x, y + 1), out Entity entityNorth) || entityNorth.IsOpenSpace()))
                        {
                            node.AddLink(new Link(node, new Node(x + ((y + 1) * Width)), 1));
                        }
                        if (y > 0 && (!Entities.TryGetValue(new Point2d(x, y - 1), out Entity entitySouth) || entitySouth.IsOpenSpace()))
                        {
                            node.AddLink(new Link(node, new Node(x + ((y - 1) * Width)), 1));
                        }
                    }
                }
            }
            */
        }

        public Board Clone()
        {
            return new Board(this);
        }

        public double? GetWinner()
        {
            return null;
        }

        public void Print()
        {

            for (int y = 0; y < Height; y++)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int x = 0; x < Width; x++)
                {
                    if (!Entities.TryGetValue(new Point2d(x, y), out Entity entity))
                    {
                        stringBuilder.Append(" ");
                    }
                    else
                    {
                        stringBuilder.Append(GetCharacter(entity.Type, entity.IsMine));
                    }
                }
                Console.Error.WriteLine(stringBuilder.ToString());
            }
        }
        public char GetCharacter(EntityType type, bool? isMine)
        {
            bool isMineInt = isMine.HasValue && isMine.Value;
            switch (type)
            {
                case EntityType.WALL:
                    return 'X';
                case EntityType.ROOT:
                    return isMineInt ? 'R' : 'r';
                case EntityType.BASIC:
                    return isMineInt ? 'B' : 'b';
                case EntityType.TENTACLE:
                    return isMineInt ? 'T' : 't';
                case EntityType.HARVESTER:
                    return isMineInt ? 'H' : 'h';
                case EntityType.SPORER:
                    return isMineInt ? 'S' : 's';
                case EntityType.A:
                    return 'A';
                case EntityType.B:
                    return 'B';
                case EntityType.C:
                    return 'C';
                case EntityType.D:
                    return 'D';
            }
            throw new ArgumentException($"Type: {type} not supported");
        }

        public void UpdateBoard()
        {

        }
    }
}
