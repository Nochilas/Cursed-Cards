function lobby() {
    return {
        roomId: "",
        playerName: "",

        players: [],
        czar: "",
        gameStarted: false,

        connection: null,

        isCzar: false,

        async init() {
            const params = new URLSearchParams(window.location.search);
            this.roomId = params.get("roomId");
            this.playerName = params.get("playerName");

            // Create connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/gamehub")
                .withAutomaticReconnect()
                .build();

            // Listener before starting
            this.connection.on("LobbyUpdated", state => {
                this.players = [...state.players];
                this.czar = state.czar;
                this.gameStarted = state.gameStarted;

                // UI: show Start Game button to czar only
                this.isCzar = this.czar === this.playerName;

                if (this.gameStarted) {
                    this.redirectToGame();
                }
            });

            // Start conneciton
            await this.connection.start();

            // Enter the room
            await this.connection.invoke("JoinRoom", this.roomId);
        },

        async startGame() {
            const res = await fetch(
                `/start-game/${this.roomId}/${this.playerName}`,
                { method: "POST" }
            );
            const data = await res.json();
            if (data.hasError) {
                alert(data.errorMessage);
            }
        },

        redirectToGame() {
            window.location.href =
                `/game/game.html?roomId=${this.roomId}&playerName=${this.playerName}`;
        }
    };
}
