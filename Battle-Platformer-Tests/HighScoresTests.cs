﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Royale_Platformer.Model.HighScores
{
    [TestFixture]
    class HighScoresUnitTests
    {
        [Test]
        public void AddHighScore_PlayerAndScore_AddsToLists()
        {
            List<HighScore> myHighScores = new List<HighScore>();
            myHighScores.Add(new HighScore("David", 100));
            myHighScores.Add(new HighScore("Matthew", 200));
            myHighScores.Add(new HighScore("Stephen", 300));
            List<string> myNames = new List<string>();
            List<int> myScores = new List<int>();
            foreach (HighScore item in myHighScores)
            {
                myNames.Add(item.GetName());
                myScores.Add(item.GetScore());
            }

            HighScoresManager h = new HighScoresManager();
            h.AddHighScore("David", 100);
            h.AddHighScore("Matthew", 200);
            h.AddHighScore("Stephen", 300);
            List<HighScore> highScores = h.GetHighScores();
            List<string> names = new List<string>();
            List<int> scores = new List<int>();
            foreach (HighScore item in highScores)
            {
                names.Add(item.GetName());
                scores.Add(item.GetScore());
            }

            for (int i = 0; i < myScores.Count; i++)
            {
                Assert.IsTrue(myNames[i].Equals(names[i]) && myScores[i].Equals(scores[i]));
            }
        }

        [Test]
        public void SortHighScores_UnsortedScores_SortedScores()
        {
            HighScoresManager h = new HighScoresManager();
            h.AddHighScore("David", 2000);
            h.AddHighScore("Matthew", 1500);
            h.AddHighScore("Stephen", 3000);
            h.AddHighScore("Isaac", 1000);
            h.AddHighScore("Elias", 2500);

            h.SortHighScores();
            Assert.IsTrue(h.GetHighScores()[0].GetScore() == 3000);
            Assert.IsTrue(h.GetHighScores()[1].GetScore() == 2500);
            Assert.IsTrue(h.GetHighScores()[2].GetScore() == 2000);
            Assert.IsTrue(h.GetHighScores()[3].GetScore() == 1500);
            Assert.IsTrue(h.GetHighScores()[4].GetScore() == 1000);
        }

        [Test]
        public void WriteScores_CreateFile_FileIsCreated()
        {
            HighScoresManager h = new HighScoresManager();
            h.WriteScores();
            Assert.IsTrue(File.Exists("HighScores.txt"));
        }

        [Test]
        public void ReadScoresToUpdate_ScoresWritten_ScoresRead()
        {
            HighScoresManager h = new HighScoresManager();

            h.AddHighScore("David", 2000);
            h.AddHighScore("Matthew", 1500);
            h.AddHighScore("Stephen", 3000);
            h.AddHighScore("Isaac", 1000);
            h.AddHighScore("Elias", 2500);

            h.SortHighScores();
            h.WriteScores();
            h.ClearHighScores();
            h.ReadScoresToUpdate();
            Assert.IsTrue(h.GetHighScores()[0].GetScore() == 3000);
            Assert.IsTrue(h.GetHighScores()[1].GetScore() == 2500);
            Assert.IsTrue(h.GetHighScores()[2].GetScore() == 2000);
            Assert.IsTrue(h.GetHighScores()[3].GetScore() == 1500);
            Assert.IsTrue(h.GetHighScores()[4].GetScore() == 1000);
        }

        [Test]
        public void ReadScoresToUpdate_ScoresNotWritten_ThrowsException()
        {
            try
            {
                HighScoresManager h = new HighScoresManager();

                h.AddHighScore("David", 2000);
                h.AddHighScore("Matthew", 1500);
                h.AddHighScore("Stephen", 3000);
                h.AddHighScore("Isaac", 1000);
                h.AddHighScore("Elias", 2500);

                h.ReadScoresToUpdate();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message == "HighScores.txt file does not exist.");
            }
        }

        [Test]
        public void CheckScore_HighScore_True()
        {
            HighScoresManager h = new HighScoresManager();
            bool check = h.CheckScore(3000);
            Assert.IsTrue(check == true);
        }

        [Test]
        public void CheckScore_LowScore_False()
        {
            HighScoresManager h = new HighScoresManager();
            bool check = h.CheckScore(2000);
            Assert.IsTrue(check == false);
        }

        [Test]
        public void ClearHighScores_ClearedScores_Passes()
        {
            HighScoresManager h = new HighScoresManager();

            h.AddHighScore("David", 2000);
            h.AddHighScore("Matthew", 1500);
            h.AddHighScore("Stephen", 3000);
            h.AddHighScore("Isaac", 1000);
            h.AddHighScore("Elias", 2500);

            h.ClearHighScores();

            Assert.IsTrue(h.GetHighScores().Count == 0);
        }
    }
}
