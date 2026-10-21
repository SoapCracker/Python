using System;
using System.Collections.Generic;

namespace Python.Game
{
    // Builds the content of one round: four apple labels with exactly one correct,
    // plus the question text.
    public static class Questions
    {
        private static readonly string[] EchteTalen = {
            "Python", "Java", "C#", "C++", "PHP", "Ruby", "Go", "Swift", "Kotlin", "Rust", "HTML", "SQL", "JavaScript"
        };

        private static readonly string[] NepTalen = {
            "Pythra", "Cobrax", "Slurm", "Zapp", "Gronk", "Jabba", "Snakr", "Blorp", "Crumb", "Zython", "Flok", "Nimbus"
        };

        public static Round Build(GameSettings settings, Random rng)
        {
            if (settings.Topic == Topic.Wiskunde)
                return BuildWiskunde(settings, rng);
            return BuildTalen(rng);
        }

        // One real language plus three fake ones, shuffled.
        private static Round BuildTalen(Random rng)
        {
            string juist = EchteTalen[rng.Next(EchteTalen.Length)];

            List<string> nep = new List<string>(NepTalen);
            Shuffle(nep, rng);

            List<string> vier = new List<string> { juist, nep[0], nep[1], nep[2] };
            Shuffle(vier, rng);

            Round r = new Round();
            r.Labels = vier.ToArray();
            r.CorrectIndex = vier.IndexOf(juist);
            r.Question = "Eet de echte programmeertaal";
            return r;
        }

        // A sum scaled to the level, with one correct and three wrong answers.
        private static Round BuildWiskunde(GameSettings settings, Random rng)
        {
            int a, b, answer;
            string vraag;

            if (settings.Level == 1)
            {
                a = rng.Next(1, 10);
                b = rng.Next(1, 10);
                if (rng.Next(2) == 0) { answer = a + b; vraag = a + " + " + b + " = ?"; }
                else { if (b > a) { int t = a; a = b; b = t; } answer = a - b; vraag = a + " - " + b + " = ?"; }
            }
            else if (settings.Level == 2)
            {
                a = rng.Next(1, 11);
                b = rng.Next(1, 11);
                if (rng.Next(2) == 0) { answer = a * b; vraag = a + " x " + b + " = ?"; }
                else { int product = a * b; answer = a; vraag = product + " : " + b + " = ?"; }
            }
            else
            {
                a = rng.Next(2, 13);
                b = rng.Next(2, 13);
                int c = rng.Next(1, 6);
                if (rng.Next(2) == 0) { answer = a * b; vraag = a + " x " + b + " = ?"; }
                else { answer = (a + b) * c; vraag = "(" + a + " + " + b + ") x " + c + " = ?"; }
            }

            // Three distractors: all distinct, non-negative, and different from the answer.
            List<int> opties = new List<int> { answer };
            int pogingen = 0;
            while (opties.Count < 4 && pogingen < 100)
            {
                pogingen++;
                int verschil = rng.Next(1, 6) * (rng.Next(2) == 0 ? 1 : -1);
                int fout = answer + verschil;
                if (fout >= 0 && !opties.Contains(fout)) opties.Add(fout);
            }
            while (opties.Count < 4) opties.Add(answer + opties.Count); // fallback if the loop above ran out of tries

            Shuffle(opties, rng);

            Round r = new Round();
            r.Labels = new string[4];
            for (int i = 0; i < 4; i++) r.Labels[i] = opties[i].ToString();
            r.CorrectIndex = opties.IndexOf(answer);
            r.Question = vraag;
            return r;
        }

        private static void Shuffle<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tijdelijk = list[i];
                list[i] = list[j];
                list[j] = tijdelijk;
            }
        }
    }
}
