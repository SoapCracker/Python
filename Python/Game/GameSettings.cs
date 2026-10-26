namespace Python.Game
{
    // Player choices from the settings screen, shared across all screens.
    public class GameSettings
    {
        public Difficulty Difficulty = Difficulty.Makkelijk;
        public int Level = 1;
        public Topic Topic = Topic.Programmeertalen;
        public int SpeedChoice = 1; // 0 = Langzaam, 1 = Normaal, 2 = Snel
        public string SkinColor = "#57C785";

        // Extra speed multiplier on top of the level speed (lower delay = faster).
        public double SpeedFactor
        {
            get
            {
                if (SpeedChoice == 0) return 1.4; // Langzaam
                if (SpeedChoice == 2) return 0.7; // Snel
                return 1.0;                        // Normaal
            }
        }

        // 7x7, 10x10 or 14x14 depending on level.
        public int GridSize
        {
            get
            {
                if (Level == 2) return 10;
                if (Level >= 3) return 14;
                return 7;
            }
        }

        // Milliseconds between snake steps; lower is faster. Higher levels start quicker.
        public int BaseMoveDelayMs
        {
            get
            {
                if (Level == 2) return 100;
                if (Level >= 3) return 85;
                return 120;
            }
        }
    }
}
