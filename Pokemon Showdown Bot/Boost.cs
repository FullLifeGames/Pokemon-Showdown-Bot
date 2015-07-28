using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pokemon_Showdown_Bot
{
    class Boost
    {
        public enum Type { GOOD, BAD };
        public Type type;
        public string text;

        private static string[] stats = { "Atk", "Def", "SpA", "SpD", "Spe" };
        private static string[] badboosts = { "0.67", "0.5", "0.4", "0.33", "0.29", "0.25" };
        private static string[] goodboosts = { "1.5", "2", "2.5", "3", "3.5", "4" };

        public int[] formatBoost()
        {
            string[] workArray = null;
            switch (type)
            {
                case Type.GOOD:
                    workArray = goodboosts;
                    break;
                case Type.BAD:
                    workArray = badboosts;
                    break;
                default:
                    break;
            }
            string workingText = text.Replace("&nbsp;", " ");
            if (workingText.Contains('×'))
            {
                string boost = workingText.Substring(0, workingText.IndexOf('×'));
                string writtentype = workingText.Substring(workingText.IndexOf(' ') + 1);

                return new int[] { (int)type, getIndex(workArray, boost), getIndex(stats, writtentype) };
            }
            else
            {
                return null;
            }
        }

        private int getIndex(string[] search, string find)
        {
            for (int i = 0; i < search.Length; i++)
            {
                if (search[i] == find)
                {
                    return i;
                }
            }
            return -1;
        }

        public static string getBoost(string boost)
        {
            if (boost == "--")
            {
                return "1,0";
            }
            if (boost.Contains("+"))
            {
                boost = boost.Replace("+", "");
                int boo = int.Parse(boost) - 1;
                return goodboosts[boo].Replace('.', ',');
            }
            if (boost.Contains("-"))
            {
                boost = boost.Replace("-", "");
                int boo = int.Parse(boost) - 1;
                return badboosts[boo].Replace('.', ',');
            }
            return "1,0";
        }

    }
}
