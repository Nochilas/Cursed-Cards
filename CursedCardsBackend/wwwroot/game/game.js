function game() {
    return {
        roomId: "",
        playerName: "",

        // Game state
        czar: null,
        roundStatus: 0,
        blackCard: "",
        scores: {},
        hands: {},
        playedCards: {},

        // Player state
        hand: [],
        selectedCards: [],
        blanksRequired: 1,
        canSubmit: true,

        // Czar flow
        isCzar: false,
        czarRevealing: false,
        czarPicking: false,
        revealIndex: 0,
        revealOrder: [],
        shuffledPlayedCards: [],

        // Winner
        winnerMessage: "",
        showWinnerBanner: false,

        async init() {
            const params = new URLSearchParams(window.location.search);
            this.roomId = params.get("roomId");
            this.playerName = params.get("playerName");

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/gamehub")
                .withAutomaticReconnect()
                .build();

            this.connection.on("GameUpdated", state => {
                this.applyState(state);
            });

            this.connection.on("WinnerChosen", winner => {
                this.showWinner(winner);
            });

            this.connection.onreconnected(async () => {
                await this.connection.invoke("JoinRoom", this.roomId);
            });

            await this.connection.start();
            await this.connection.invoke("JoinRoom", this.roomId);
        },

        get currentReveal() {
            const player = this.revealOrder[this.revealIndex - 1];
            return player ? this.playedCards[player] : [];
        },

        resetCzarFlow() {
            this.czarRevealing = false;
            this.czarPicking = false;
            this.revealIndex = 0;
            this.revealOrder = [];
            this.shuffledPlayedCards = [];

            if (this.roundStatus === 0) {
                this.selectedCards = [];
            }
        },

        toggleCard(card) {
            if (this.selectedCards.includes(card)) {
                this.selectedCards = this.selectedCards.filter(c => c !== card);
            } else if (this.selectedCards.length < this.blanksRequired) {
                this.selectedCards.push(card);
            }
        },

        async submitCards() {
            // Disable button to avoid multiple submits
            this.canSubmit = false;

            const res = await fetch(`/play-cards/${this.roomId}/${this.playerName}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ selectedCards: this.selectedCards })
            });

            const data = await res.json();
            if (data.hasError) {
                alert(data.errorMessage);
                this.canSubmit = true;
                return;
            }

            // Draw white
            await fetch(
                `/draw-white/${this.roomId}/${this.playerName}/${this.blanksRequired}`,
                { method: "POST" }
            );
        },

        async drawBlack() {
            const res = await fetch(`/draw-black/${this.roomId}`, { method: "POST" });
            const data = await res.json();
            if (!data.hasError) this.blackCard = data.response;
            else alert(data.errorMessage)
        },

        async startRound() {
            const res = await fetch(`/start-round/${this.roomId}/${this.playerName}`, {
                method: "POST"
            });
            const data = await res.json();
            if (data.hasError) {
                alert(data.errorMessage);
            }
        },

        prepareReveal() {
            if (this.czarRevealing || this.czarPicking) return;
            if (!Object.keys(this.playedCards).length) return;

            this.revealOrder = Object.keys(this.playedCards);
            this.shuffle(this.revealOrder);
            this.czarRevealing = true;
        },

        revealNext() {
            this.revealIndex++;

            if (this.revealIndex > this.revealOrder.length) {
                this.czarRevealing = false;
                this.czarPicking = true;

                this.shuffledPlayedCards =
                    Object.entries(this.playedCards);
                this.shuffle(this.shuffledPlayedCards);
            }
        },

        async selectWinner(player) {
            const res = await fetch(
                `/select-winner?roomId=${this.roomId}&czarPlayerName=${this.playerName}&winnerPlayer=${player}`,
                { method: "POST" }
            );
            const data = await res.json();
            if (data.hasError) {
                alert(data.errorMessage)
            } else {
                this.resetCzarFlow();
            }
        },

        shuffle(arr) {
            for (let i = arr.length - 1; i > 0; i--) {
                const j = Math.floor(Math.random() * (i + 1));
                [arr[i], arr[j]] = [arr[j], arr[i]];
            }
        },

        applyState(state) {
            this.czar = state.czar;
            this.roundStatus = state.roundStatus;
            this.blackCard = state.currentBlackCard ?? "";
            this.scores = state.scores ?? {};
            this.playedCards = state.playedCards ?? {};

            this.blanksRequired =
                (this.blackCard.match(/_/g) || []).length || 1;

            this.isCzar = this.czar === this.playerName;

            // Player hand
            if (this.czar !== this.playerName) {
                this.hand = state.hands?.[this.playerName] ?? [];
            } else {
                this.hand = [];
            }

            // Reset czar flow if round changed
            if (this.roundStatus !== 2) {
                // Reset submit if a new round started
                if (this.roundStatus === 0) {
                    this.canSubmit = true;
                }

                this.resetCzarFlow();
                return;
            }

            // Prepare reveal for czar
            if (this.isCzar
                // Czar is not picking nor revealing
                // But the status (2) says that the czar must pick or reveal
                && !this.czarPicking
                && !this.czarRevealing) {
                this.prepareReveal();
            }
        },

        showWinner(winner) {
            this.winnerMessage = winner;
            this.showWinnerBanner = true;

            setTimeout(() => {
                this.showWinnerBanner = false;
                this.winnerMessage = "";
            }, 2000);
        }

    };
}
