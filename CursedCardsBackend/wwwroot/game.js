function game() {
    return {
        roomId: "",
        playerName: "",

        // game state
        czar: null,
        roundStatus: 0,
        blackCard: "",
        scores: {},
        hands: {},
        playedCards: {},

        // player state
        hand: [],
        selectedCards: [],
        blanksRequired: 1,

        // czar flow
        czarRevealing: false,
        czarPicking: false,
        revealIndex: 0,
        revealOrder: [],
        shuffledPlayedCards: [],

        init() {
            const qs = new URLSearchParams(window.location.search);
            this.roomId = qs.get("roomId");
            this.playerName = qs.get("playerName");

            this.loadState();
            setInterval(() => this.loadState(), 2000);
        },

        get isCzar() {
            return this.czar === this.playerName;
        },

        get currentReveal() {
            const player = this.revealOrder[this.revealIndex - 1];
            return player ? this.playedCards[player] : [];
        },

        async loadState() {
            const res = await fetch(`/game-state/${this.roomId}`);
            const data = await res.json();
            if (!res.ok) return;

            const s = data.response;

            this.czar = s.czar;
            this.roundStatus = s.roundStatus;
            this.blackCard = s.currentBlackCard ?? "";
            this.scores = s.scores ?? {};
            this.playedCards = s.playedCards ?? {};

            this.blanksRequired =
                (this.blackCard.match(/_/g) || []).length || 1;

            if (!this.isCzar) {
                this.hand = s.hands?.[this.playerName] ?? [];
            }

            // RESET FLOW when round changes
            if (this.roundStatus !== 2) {
                this.resetCzarFlow();
            }

            // Prepare czar reveal
            if (this.isCzar && this.roundStatus === 2 && !this.czarPicking) {
                this.prepareReveal();
            }
        },

        resetCzarFlow() {
            this.czarRevealing = false;
            this.czarPicking = false;
            this.revealIndex = 0;
            this.revealOrder = [];
            this.shuffledPlayedCards = [];
            this.selectedCards = [];
        },

        toggleCard(card) {
            if (this.selectedCards.includes(card)) {
                this.selectedCards = this.selectedCards.filter(c => c !== card);
            } else if (this.selectedCards.length < this.blanksRequired) {
                this.selectedCards.push(card);
            }
        },

        async submitCards() {
            await fetch(`/play-cards/${this.roomId}/${this.playerName}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ selectedCards: this.selectedCards })
            });

            await fetch(
                `/draw-white/${this.roomId}/${this.playerName}/${this.blanksRequired}`,
                { method: "POST" }
            );

            this.selectedCards = [];
        },

        async drawBlack() {
            const res = await fetch(`/draw-black/${this.roomId}`, { method: "POST" });
            const data = await res.json();
            if (res.ok) this.blackCard = data.response;
        },

        async startRound() {
            await fetch(`/start-round/${this.roomId}/${this.playerName}`, {
                method: "POST"
            });
        },

        prepareReveal() {
            if (this.revealOrder.length) return;

            this.revealOrder = Object.keys(this.playedCards);
            this.shuffle(this.revealOrder);
            this.czarRevealing = true;
        },

        revealNext() {
            this.revealIndex++;

            if (this.revealIndex >= this.revealOrder.length) {
                this.czarRevealing = false;
                this.czarPicking = true;

                this.shuffledPlayedCards =
                    Object.entries(this.playedCards);
                this.shuffle(this.shuffledPlayedCards);
            }
        },

        async selectWinner(player) {
            await fetch(
                `/select-winner?roomId=${this.roomId}&czarPlayerName=${this.playerName}&winnerPlayer=${player}`,
                { method: "POST" }
            );
            this.resetCzarFlow();
        },

        shuffle(arr) {
            for (let i = arr.length - 1; i > 0; i--) {
                const j = Math.floor(Math.random() * (i + 1));
                [arr[i], arr[j]] = [arr[j], arr[i]];
            }
        }
    };
}
