using System;
using System.Collections.Generic;

namespace Chess.Core
{
    public sealed class GameState
    {
        private static readonly IReadOnlyList<Move> NoMoves = new Move[0];

        private readonly List<Move> _history = new List<Move>();
        private readonly Dictionary<string, int> _repetitions = new Dictionary<string, int>();
        private Position _position;
        private IReadOnlyList<Move> _legalMoves;
        private GameStatus _status;
        private GameResult _result;

        public GameState() : this(Position.CreateStart())
        {
        }

        public GameState(string fen) : this(ParseFen(fen))
        {
        }

        private GameState(Position position)
        {
            _position = position;
            Refresh();
        }

        public Position Position => _position;
        public Side SideToMove => _position.SideToMove;
        public IReadOnlyList<Move> LegalMoves => IsGameOver ? NoMoves : _legalMoves;
        public bool IsCheck => _position.IsInCheck();
        public GameStatus Status => _status;
        public GameResult Result => _result;
        public bool IsGameOver => _status != GameStatus.InProgress;
        public IReadOnlyList<Move> MoveHistory => _history;
        public Move? LastMove => _history.Count == 0 ? (Move?)null : _history[_history.Count - 1];

        public IReadOnlyList<Move> GetLegalMovesFrom(Square from)
        {
            var result = new List<Move>();
            foreach (Move m in LegalMoves)
            {
                if (m.From == from)
                    result.Add(m);
            }
            return result;
        }

        public bool TryMakeMove(Move move)
        {
            if (!TryFindLegal(move, out Move legal))
                return false;

            _position = _position.MakeMove(legal);
            _history.Add(legal);
            Refresh();
            return true;
        }

        public bool TryResolveUciMove(string uci, out Move legalMove)
        {
            legalMove = default;
            return Move.TryParseUci(uci, out Move parsed) && TryFindLegal(parsed, out legalMove);
        }

        public bool TryMakeUciMove(string uci)
        {
            return TryResolveUciMove(uci, out Move legal) && TryMakeMove(legal);
        }

        public string ToFen() => _position.ToFen();

        private bool TryFindLegal(Move move, out Move legal)
        {
            legal = default;
            if (IsGameOver)
                return false;
            for (int i = 0; i < _legalMoves.Count; i++)
            {
                if (_legalMoves[i] == move)
                {
                    legal = _legalMoves[i];
                    return true;
                }
            }
            return false;
        }

        private static Position ParseFen(string fen)
        {
            Position pos = Position.FromFen(fen); // throws ArgumentNullException / FormatException
            Side them = pos.SideToMove.Opposite();
            if (pos.IsSquareAttacked(pos.FindKing(them), pos.SideToMove))
                throw new FormatException("Invalid FEN: side not to move is in check. FEN: '" + fen + "'");
            return pos;
        }

        // Status order: mate/stalemate, insufficient material, fifty-move, threefold repetition.
        private void Refresh()
        {
            _legalMoves = _position.GetLegalMoves();

            string key = DrawRules.RepetitionKey(_position, _legalMoves);
            _repetitions.TryGetValue(key, out int count);
            count++;
            _repetitions[key] = count;

            if (_legalMoves.Count == 0)
            {
                if (_position.IsInCheck())
                {
                    _status = GameStatus.Checkmate;
                    _result = _position.SideToMove == Side.White ? GameResult.BlackWins : GameResult.WhiteWins;
                }
                else
                {
                    _status = GameStatus.Stalemate;
                    _result = GameResult.Draw;
                }
                return;
            }

            if (DrawRules.IsInsufficientMaterial(_position))
                _status = GameStatus.InsufficientMaterial;
            else if (_position.HalfmoveClock >= 100)
                _status = GameStatus.FiftyMoveRule;
            else if (count >= 3)
                _status = GameStatus.ThreefoldRepetition;
            else
            {
                _status = GameStatus.InProgress;
                _result = GameResult.None;
                return;
            }
            _result = GameResult.Draw;
        }
    }
}
