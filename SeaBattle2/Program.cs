using SeaBattle2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattle2
{
    class Program
    {
        static void Main(string[] args)
        {
            GameSession gameSession = new GameSession();
            gameSession.Begin();
        }
    }

    class BattleArea
    {
        public int Dimension;
        public char[,] Layout;

        public BattleArea(int dimension)
        {
            Dimension = dimension;
            Layout = new char[Dimension, Dimension];
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    Layout[i, j] = '~';
                }
            }
        }

        public void Show(bool hideUnits = true)
        {
            Console.Write("   ");
            for (int i = 0; i < Dimension; i++)
            {
                Console.Write($"{i} ");
            }
            Console.WriteLine();

            for (int i = 0; i < Dimension; i++)
            {
                Console.Write($"{i}  ");
                for (int j = 0; j < Dimension; j++)
                {
                    if (hideUnits && Layout[i, j] == 'S')
                    {
                        Console.Write("~ ");
                    }
                    else
                    {
                        Console.Write($"{Layout[i, j]} ");
                    }
                }
                Console.WriteLine();
            }
        }

        public bool CanDeploy(int row, int col)
        {
            return row >= 0 && row < Dimension && col >= 0 && col < Dimension && Layout[row, col] == '~';
        }

        public bool DeployUnit(int row, int col, int size, bool horizontal)
        {
            if (horizontal)
            {
                if (col + size > Dimension) return false;
                for (int i = 0; i < size; i++)
                {
                    if (!CanDeploy(row, col + i) || !IsZoneSafe(row, col + i)) return false;
                }

                for (int i = 0; i < size; i++)
                {
                    Layout[row, col + i] = 'S';
                }
            }
            else
            {
                if (row + size > Dimension) return false;
                for (int i = 0; i < size; i++)
                {
                    if (!CanDeploy(row + i, col) || !IsZoneSafe(row + i, col)) return false;
                }
                for (int i = 0; i < size; i++)
                {
                    Layout[row + i, col] = 'S';
                }
            }
            return true;
        }

        private bool IsZoneSafe(int row, int col)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < Dimension && newCol >= 0 && newCol < Dimension && Layout[newRow, newCol] == 'S')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool Fire(int row, int col)
        {
            if (Layout[row, col] == 'S')
            {
                Layout[row, col] = 'X';
                return true;
            }
            else if (Layout[row, col] == '~')
            {
                Layout[row, col] = 'O';
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool HasUnitsLeft()
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    if (Layout[i, j] == 'S')
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void MarkDestroyedArea(int row, int col)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < Dimension && newCol >= 0 && newCol < Dimension && Layout[newRow, newCol] == '~')
                    {
                        Layout[newRow, newCol] = 'O';
                    }
                }
            }
        }

        public bool IsUnitWipedOut(int row, int col)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < Dimension && newCol >= 0 && newCol < Dimension && Layout[newRow, newCol] == 'S')
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    class Participant
    {
        public BattleArea Area;
        public string Title;

        public Participant(string title, int areaDimension)
        {
            Title = title;
            Area = new BattleArea(areaDimension);
        }

        public (int row, int col) GetCoordinatesFromInput()
        {
            int row, col;
            while (true)
            {
                Console.Write($"{Title}, введите координаты цели (строка столбец, например, 1 2): ");
                string input = Console.ReadLine();
                string[] parts = input.Split(' ');

                if (parts.Length == 2 && int.TryParse(parts[0], out int rowResult) && int.TryParse(parts[1], out int colResult))
                {
                    row = rowResult;
                    col = colResult;
                    return (row, col);
                }
                else
                {
                    Console.WriteLine("Неверный формат ввода. Попробуйте снова.");
                }
            }
        }

        public int GetTargetCoordinates(int row, int col)
        {
            if (row >= 0 && row < Area.Dimension && col >= 0 && col < Area.Dimension)
            {
                return 0;
            }
            else
            {
                Console.WriteLine("Неверные координаты.");
                return -1;
            }
        }
    }

    class Computer : Participant
    {
        private Random generator = new Random();
        public int row;
        public int col;

        public Computer(string title, int areaDimension) : base(title, areaDimension) { }

        public void AutoDeployUnits(int[] unitSizes)
        {
            foreach (int size in unitSizes)
            {
                bool deployed = false;
                while (!deployed)
                {
                    row = generator.Next(0, Area.Dimension);
                    col = generator.Next(0, Area.Dimension);
                    bool isHorizontal = generator.Next(0, 2) == 0;

                    deployed = Area.DeployUnit(row, col, size, isHorizontal);
                }
            }
        }

        public int GetRandomTargetCoordinates()
        {
            row = generator.Next(0, Area.Dimension);
            col = generator.Next(0, Area.Dimension);
            return 0;
        }
    }

    class GameSession
    {
        private Participant player;
        private Computer bot;
        private int fieldSize = 10;
        private int[] unitSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

        public GameSession()
        {
            player = new Participant("Игрок", fieldSize);
            bot = new Computer("Бот", fieldSize);
        }

        public void Begin()
        {
            Console.WriteLine("Добро пожаловать в игру Морской бой!");

            PositionPlayerUnits();

            bot.AutoDeployUnits(unitSizes);


            RunGame();

            AnnounceWinner();
        }

        private void PositionPlayerUnits()
        {
            Console.WriteLine("Разместите свои корабли на поле:");
            player.Area.Show(false);
            foreach (int size in unitSizes)
            {
                bool placed = false;
                while (!placed)
                {
                    Console.WriteLine($"Разместите корабль длиной {size}.");
                    (int row, int col) = player.GetCoordinatesFromInput();
                    Console.Write("Горизонтально? (y/n): ");
                    bool isHorizontal = Console.ReadLine().ToLower() == "y";

                    if (player.GetTargetCoordinates(row, col) == 0)
                    {
                        placed = player.Area.DeployUnit(row, col, size, isHorizontal);
                        if (!placed)
                        {
                            Console.WriteLine("Невозможно разместить корабль здесь. Попробуйте снова.");
                        }
                        else
                        {
                            player.Area.Show(false);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверные координаты. Попробуйте снова.");
                    }
                }
            }

            Console.WriteLine("Корабли расставлены!");
        }

        private void RunGame()
        {
            while (player.Area.HasUnitsLeft() && bot.Area.HasUnitsLeft())
            {
                PlayerAction();
                if (!bot.Area.HasUnitsLeft()) break;

                BotAction();
                if (!player.Area.HasUnitsLeft()) break;
            }
        }

        private void PlayerAction()
        {
            Console.WriteLine("\nВаш ход:");
            bot.Area.Show();

            (int row, int col) = player.GetCoordinatesFromInput();
            if (player.GetTargetCoordinates(row, col) == 0)
            {
                bool hit = bot.Area.Fire(row, col);

                if (hit)
                {
                    Console.WriteLine("Попадание!");

                    if (bot.Area.IsUnitWipedOut(row, col))
                    {
                        Console.WriteLine("Корабль уничтожен!");
                        bot.Area.MarkDestroyedArea(row, col);
                    }
                }
                else
                {
                    Console.WriteLine("Промах.");
                }
            }
            else
            {
                Console.WriteLine("Неверные координаты. Попробуйте снова.");
            }
        }

        private void BotAction()
        {
            Console.WriteLine("\nХод бота:");
            int result = bot.GetRandomTargetCoordinates();

            if (result == 0 && player.GetTargetCoordinates(bot.row, bot.col) == 0)
            {
                bool hit = player.Area.Fire(bot.row, bot.col);

                Console.WriteLine($"Бот стреляет в {bot.row} {bot.col}.");

                if (hit)
                {
                    Console.WriteLine("Бот попал!");
                    if (player.Area.IsUnitWipedOut(bot.row, bot.col))
                    {
                        Console.WriteLine("Ваш корабль уничтожен!");
                        player.Area.MarkDestroyedArea(bot.row, bot.col);
                    }
                }
                else
                {
                    Console.WriteLine("Бот промахнулся.");
                }
                player.Area.Show(false);
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Бот выбрал неверные координаты (этого не должно происходить).");
            }
        }

        private void AnnounceWinner()
        {
            Console.WriteLine("\nИгра окончена!");

            if (!bot.Area.HasUnitsLeft())
            {
                Console.WriteLine("Поздравляем! Вы победили!");
            }
            else
            {
                Console.WriteLine("Бот победил.");
            }
        }
    }
}