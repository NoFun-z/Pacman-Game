using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    public class PlayerRecord
    {
        public string Name;
        public int HighScore;

        public PlayerRecord(string name, int newhighscore)
        {
            Name = name; 
            HighScore = newhighscore;
        }

        public override string ToString()
        {
            return $"{{playername: \"{Name}\", highscore: {HighScore}}}";
        }
    }
}
