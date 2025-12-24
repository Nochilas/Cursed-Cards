function createGame() {
    return {
        loading: false,
        error: "",
        inviteLink: "",
        copied: false,

        async create() {
            this.error = "";
            this.inviteLink = "";
            this.loading = true;

            try {
                const res = await fetch("/create-game", {
                    method: "POST"
                });

                const data = await res.json();

                if (!res.ok) {
                    this.error = data.errorMessage ?? "Unknown error";
                    return;
                }

                this.inviteLink =
                    `${window.location.origin}/join.html?roomId=${data.response}`;

            } catch (err) {
                this.error = "An error occurred while creating the game.";
                console.error(err);
            } finally {
                this.loading = false;
            }
        },

        async copy() {
            try {
                await navigator.clipboard.writeText(this.inviteLink);
                this.copied = true;
                setTimeout(() => this.copied = false, 1200);
            } catch {
                alert("Copy failed");
            }
        }
    };
}
