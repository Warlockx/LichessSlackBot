using System;
using LichessGameCreatorSlackBot.Model;

namespace LichessGameCreatorSlackBot.Services
{
    public static class ChessVariantInfo
    {
        public static string GetInfo(ChessGameVariants chessGame)
        {
            switch (chessGame)
            {
                case ChessGameVariants.Standard:
                    return ">Standard: Standard rules of chess (FIDE)";
                case ChessGameVariants.Crazyhouse:
                    return ">Crazyhouse: Captured pieces can be dropped back on the board instead of moving a piece.";
                case ChessGameVariants.Chess960:
                    return ">Chess960: Starting position of the home rank pieces is randomized.";
                case ChessGameVariants.KingOfTheHill:
                    return ">KingOfTheHill: Bring your King to the center to win the game.";
                case ChessGameVariants.ThreeCheck:
                    return ">ThreeCheck: Check your opponent 3 times to win the game.";
                case ChessGameVariants.AntiChess:
                    return ">AntiChess: Lose all your pieces (or reach a stalemate) to win the game.";
                case ChessGameVariants.Atomic:
                    return ">Atomic: Nuke your opponent's king to win.";
                case ChessGameVariants.Horde:
                    return ">Horde: Destroy the horde to win!";
                case ChessGameVariants.RacingKings:
                    return ">RacingKings: Race your King to the eighth rank to win.";
                case ChessGameVariants.FromPosition:
                    return ">FromPosition: Custom starting position, needs to set fen parameter to work.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(chessGame), chessGame, null);
            }
        }
    }
}
