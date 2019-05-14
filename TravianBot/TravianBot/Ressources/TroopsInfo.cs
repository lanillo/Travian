
using TravianBot.Functionnalities.Utilities;
using TravianBot.Functionnalities.Enums;

namespace TravianBot.Ressources
{
    public class TroopsInfo
    {
        public Unit Legionnaire = new Unit()
        {
            Name = "Legionnaire",
            building = Buildings.Barracks,
            Cost = new Cost()
            {
                Wood = 120,
                Clay = 100,
                Iron = 150,
                Wheat = 30,
                Crop = 1,
                Time = Utilities.TimeToMs("00:17:30")
            },
            Amount = 0
        };

        public Unit Praetorian = new Unit()
        {
            Name = "Praetorian",
            building = Buildings.Barracks,
            Cost = new Cost()
            {
                Wood = 100,
                Clay = 130,
                Iron = 160,
                Wheat = 70,
                Crop = 1,
                Time = Utilities.TimeToMs("00:19:15")
            },
            Amount = 0
        };

        public Unit EquitesLegati = new Unit()
        {
            Name = "Equites Legati",
            building = Buildings.Stables,
            Cost = new Cost()
            {
                Wood = 140,
                Clay = 160,
                Iron = 20,
                Wheat = 40,
                Crop = 2,
                Time = Utilities.TimeToMs("00:14:52")
            },
            Amount = 0
        };

        public Unit EquitesImperatoris = new Unit()
        {
            Name = "Equites Imperatoris",
            building = Buildings.Stables,
            Cost = new Cost()
            {
                Wood = 550,
                Clay = 440,
                Iron = 320,
                Wheat = 100,
                Crop = 3,
                Time = Utilities.TimeToMs("00:28:52")
            },
            Amount = 0
        };

        public Unit Settlers = new Unit
        {
            Name = "Equites Imperatoris",
            building = Buildings.Residence,
            Cost = new Cost()
            {
                Wood = 4600,
                Clay = 4200,
                Iron = 5800,
                Wheat = 4400,
                Crop = 1,
                Time = Utilities.TimeToMs("02:53:42")
            },
            Amount = 0
        };

        public Unit Hero = new Unit
        {
            Name = "Hero",
            building = Buildings.Residence,
            Cost = new Cost()
            {
                Wood = 0,
                Clay = 0,
                Iron = 0,
                Wheat = 0,
                Crop = 0,
                Time = Utilities.TimeToMs("02:53:42")
            },
            Amount = 0
        };
    }

    public class Unit
    {
        public string Name { get; set; }
        public Buildings building;
        public Cost Cost { get; set; }
        public int Amount { get; set; }
    }

    public class Cost
    {
        public int Wood { get; set; }
        public int Clay { get; set; }
        public int Iron { get; set; }
        public int Wheat { get; set; }
        public int Crop { get; set; }
        public int Time { get; set; }
    }
}
