using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LichessGameCreatorSlackBot.Model;

namespace LichessGameCreatorSlackBot
{
    public static class ChessVariantInfo
    {
        public static string GetInfo(ChessGameVariants chessGame)
        {
            switch (chessGame)
            {
                case ChessGameVariants.Standard:
                    return ">Standard rules of chess (FIDE)";
                case ChessGameVariants.Crazyhouse:
                    return ">Captured pieces can be dropped back on the board instead of moving a piece.";
                case ChessGameVariants.Chess960:
                    return ">Starting position of the home rank pieces is randomized.";
                case ChessGameVariants.KingOfTheHill:
                    return ">Bring your King to the center to win the game.";
                case ChessGameVariants.ThreeCheck:
                    return ">Check your opponent 3 times to win the game.";
                case ChessGameVariants.AntiChess:
                    return ">Lose all your pieces (or reach a stalemate) to win the game.";
                case ChessGameVariants.Atomic:
                    return ">Nuke your opponent's king to win.";
                case ChessGameVariants.Horde:
                    return ">Destroy the horde to win!";
                case ChessGameVariants.RacingKings:
                    return ">Race your King to the eighth rank to win.";
                case ChessGameVariants.FromPosition:
                    return ">Custom starting position";
                default:
                    throw new ArgumentOutOfRangeException(nameof(chessGame), chessGame, null);
            }
        }
    }
}
