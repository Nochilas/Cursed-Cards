function lobby() {
    return {
        roomId: "",
        playerName: "",

        players: [],
        czar: "",
        gameStarted: false,

        /** Inits the lobby. */
        init() {
            const params = new URLSearchParams(window.location.search);
            this.roomId = params.get("roomId");
            this.playerName = params.get("playerName");

            // First loading is immediate
            this.refresh();

            // Polling every 2 seconds
            setInterval(() => this.refresh(), 2000);
        },

        get isCzar() {
            return this.czar === this.playerName;
        },

        /** Refresh UI. */
        async refresh() {
            try {
                // Load state
                const res = await fetch(`/lobby-state/${this.roomId}`);
                const data = await res.json();

                if (!res.ok) {
                    console.error(data.errorMessage ?? "Lobby polling error");
                    return;
                }

                // Show players
                const state = data.response;

                this.players = state.players ?? [];
                this.czar = state.czar ?? "";
                this.gameStarted = state.gameStarted ?? false;

                if (this.gameStarted) {
                    this.redirectToGame();
                }

            } catch (err) {
                console.error("Error refreshing lobby", err);
            }
        },

        /** Starts the game. */
        async startGame() {
            try {
                const res = await fetch(
                    `/start-game/${this.roomId}/${this.playerName}`,
                    { method: "POST" }
                );

                const data = await res.json();

                if (!res.ok) {
                    alert(data.errorMessage);
                }

            } catch (err) {
                console.error("Error starting game", err);
            }
        },

        /** Redirect players to game. */
        redirectToGame() {
            window.location.href =
                `/game.html?roomId=${this.roomId}&playerName=${this.playerName}`;
        }
    };
}
