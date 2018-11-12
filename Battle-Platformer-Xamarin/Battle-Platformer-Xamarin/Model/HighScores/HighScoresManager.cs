﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Royale_Platformer.Model.HighScores
{
    class HighScoresManager
    {
        private List<HighScore> highScores;
        public HighScoresManager()
        {
            highScores = new List<HighScore>();
            WriteScores();
        }

        // Checks to see if score is a high score <score>
        // Returns true if score is a highscore (2500 or higher) and false if not a highscore
        public bool CheckScore(int score)
        {
            if (score >= 2500)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Adds a player's name <name> and score <score> to the dictionary
        public void AddHighScore(string playerName, int playerScore)
        {
            highScores.Add(new HighScore(playerName, playerScore));
        }

        // Writes the highScores List to a file
        public void WriteScores()
        {
            using (StreamWriter writer = new StreamWriter(File.Create("HighScores.txt")))
            {
                foreach (HighScore score in highScores)
                {
                    writer.WriteLine(score.GetName() + "," + score.GetScore());
                }
            }
        }

        // Reads names and scores from a file and puts them in the highScore list
        public void ReadScores()
        {
            int count = 0;
            if (File.Exists("HighScores.txt"))
            {
                using (StreamReader reader = new StreamReader(File.Open("HighScores.txt", FileMode.Open)))
                {
                    while (!reader.EndOfStream)
                    {
                        string score = reader.ReadLine();
                        string[] items = score.Split(',');
                        highScores[count] = new HighScore(items[0], Convert.ToInt32(items[1]));
                        count++;
                    }
                }
            }
            else
            {
                throw new Exception("HighScores.txt file does not exist.");
            }
        }

        // Returns names of players held in the list instance variable
        public List<HighScore> GetHighScores()
        {
            return highScores;
        }

        // Provides the ability to clear highScores list for testing purposes
        public void ClearHighScores()
        {
            highScores.Clear();
        }

        // Sorts the highScores list and updates it
        public void SortHighScores()
        {
            // OrderBy function found at "https://stackoverflow.com/questions/16620135/sort-a-list-of-objects-by-the-value-of-a-property/16620159"
            highScores = highScores.OrderByDescending(x => x.GetScore()).ToList();
        }
    }
}


