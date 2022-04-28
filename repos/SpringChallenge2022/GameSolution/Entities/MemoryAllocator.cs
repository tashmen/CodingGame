using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public static class MemoryAllocator
    {
        public static Hero[] Heroes { get; set; } = new Hero[30000];
        public static int HeroCount { get; set; } = 0;

        public static Monster[] Monsters { get; set; } = new Monster[400000];
        public static int MonsterCount { get; set; } = 0;

        public static Base[] Bases { get; set; } = new Base[10000];
        public static int BaseCount { get; set; } = 0;

        public static bool AllowRecycle { get; set; } = false;

        public static Hero GetHero()
        {
            var hero = Heroes[HeroCount];
            if (HeroCount == Heroes.Length - 1)
            {
                if (AllowRecycle)
                    HeroCount = 0;
                else
                {
                    Console.Error.WriteLine("Ran out of heroes");
                    return new Hero();
                }
            }
            else HeroCount++;

            return hero;
        }

        public static Monster GetMonster()
        {
            var monster = Monsters[MonsterCount];
            if (MonsterCount == Monsters.Length - 1)
            {
                if (AllowRecycle)
                    MonsterCount = 0;
                else
                {
                    Console.Error.WriteLine("Ran out of Monsters");
                    return new Monster();
                }
            }
            else MonsterCount++;

            return monster;
        }

        public static Base GetBase()
        {
            var b = Bases[BaseCount];
            if (BaseCount == Bases.Length - 1)
            {
                if (AllowRecycle)
                    BaseCount = 0;
                else
                {
                    Console.Error.WriteLine("Ran out of bases");
                    return new Base();
                }
                    
                
            }
            else BaseCount++;

            return b;
        }

        public static void Initialize(bool allowRecylce = false)
        {
            MemoryAllocator.AllowRecycle = allowRecylce;
            for(int i = 0; i < Heroes.Length; i++)
            {
                Heroes[i] = new Hero();
            }

            for (int i = 0; i < Monsters.Length; i++)
            {
                Monsters[i] = new Monster();
            }

            for (int i = 0; i < Bases.Length; i++)
            {
                Bases[i] = new Base();
            }

            Reset();
        }

        public static void Reset()
        {
            HeroCount = 0;
            MonsterCount = 0;
            BaseCount = 0;
        }
    }
}
