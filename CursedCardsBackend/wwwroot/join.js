function joinGame() {
    return {
        roomId: "",
        playerName: "",
        loading: false,
        joined: false,
        error: "",

        init() {
            const params = new URLSearchParams(window.location.search);
            this.roomId = params.get("roomId");

            if (!this.roomId) {
                this.error = "Missing or invalid RoomId";
            }
        },

        get roomText() {
            return this.roomId
                ? `Room Code: ${this.roomId}`
                : "Error: no room specified.";
        },

        async join() {
            if (!this.playerName.trim()) {
                this.error = "Username is required";
                return;
            }

            if (!this.roomId) {
                this.error = "Missing or invalid RoomId";
                return;
            }

            this.error = "";
            this.loading = true;

            try {
                const res = await fetch(
                    `/join-game/${this.roomId}/${this.playerName}`,
                    { method: "POST" }
                );

                const data = await res.json();
                if (data.hasError) {
                    this.error = data.errorMessage ?? "Unknown error";
                    return;
                }

                this.joined = true;

                // Redirect to lobby
                window.location.href =
                    `/lobby.html?roomId=${this.roomId}&playerName=${data.response}`;

            } catch (err) {
                this.error = "An error occurred while joining the game.";
                console.error(err);
            } finally {
                this.loading = false;
            }
        }
    };
}
