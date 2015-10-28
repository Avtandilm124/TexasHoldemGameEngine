﻿namespace TexasHoldem.Logic.GameMechanics
{
    using System.Collections.Generic;

    using TexasHoldem.Logic.Cards;
    using TexasHoldem.Logic.Players;

    internal class TwoPlayersBettingLogic
    {
        private readonly IList<InternalPlayer> allPlayers;

        private readonly List<PlayerActionAndName> bets;

        private readonly int smallBlind;

        private int pot = 0;

        public TwoPlayersBettingLogic(InternalPlayer firstPlayer, InternalPlayer secondPlayer, int smallBlind)
        {
            this.allPlayers = new[] { firstPlayer, secondPlayer };
            this.smallBlind = smallBlind;
            this.bets = new List<PlayerActionAndName>();
        }

        public void Start(GameRoundType gameRoundType, ICollection<Card> communityCards)
        {
            var potBeforeRound = this.pot;
            var playerIndex = 0;

            if (gameRoundType == GameRoundType.PreFlop)
            {
                this.Bet(this.allPlayers[0], this.smallBlind);
                this.bets.Add(new PlayerActionAndName(this.allPlayers[0].Name, PlayerAction.Raise(this.smallBlind)));
                playerIndex++;

                this.Bet(this.allPlayers[1], this.smallBlind * 2);
                this.bets.Add(new PlayerActionAndName(this.allPlayers[1].Name, PlayerAction.Raise(this.smallBlind * 2)));
                playerIndex++;
            }

            while (true)
            {
                var player = this.allPlayers[playerIndex % 2];
                var getTurnContext = new GetTurnContext(
                    communityCards,
                    gameRoundType,
                    potBeforeRound,
                    this.bets.AsReadOnly(),
                    this.pot);
                var action = player.GetTurn(getTurnContext);

                this.bets.Add(new PlayerActionAndName(player.Name, action));

                if (action.Type == PlayerActionType.Raise)
                {
                    this.Bet(player, action.Money);
                }
                else if (action.Type == PlayerActionType.Call)
                {
                    this.Bet(player, action.Money);
                }
                else if (action.Type == PlayerActionType.Check)
                {
                    // TODO: Is OK to check?
                }
                else
                {
                    // Fold
                    break;
                }

                playerIndex++;
            }
        }

        private void Bet(InternalPlayer player, int amount)
        {
            // TODO: What if small blind is bigger than player's money?
            player.Bet(amount);
            this.pot += amount;
        }
    }
}