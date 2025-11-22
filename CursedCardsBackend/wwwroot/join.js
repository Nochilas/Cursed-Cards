// Reads roomId from url, es: ?roomId=ABC123
const params = new URLSearchParams(window.location.search);
const roomId = params.get("roomId");

const roomInfo = document.getElementById("roomInfo");
roomInfo.textContent = roomId
    ? `Room Code: ${roomId}`
    : "Error: no room specified.";

document.getElementById("joinBtn").addEventListener("click", async () => {
    const resultDiv = document.getElementById("result");
    const playerName = document.getElementById("usernameInput").value.trim();

    if (!playerName) {
        resultDiv.textContent = "Username is required";
        return;
    }

    if (!roomId) {
        resultDiv.textContent = "Missing or invalid RoomId";
        return;
    }

    resultDiv.textContent = "Connecting...";

    try {
        // The player joins the game
        const joinGameResult = await fetch(`/join-game/${roomId}/${playerName}`, {
            method: "POST"
        });

        // Check for errors
        if (!joinGameResult.ok) {
            const errorData = await joinGameResult.json();
            resultDiv.textContent = errorData.errorMessage ?? "Unknown error";
            return;
        }

        const data = await joinGameResult.json();
        resultDiv.textContent = `You joined the game as ${data.response}!`;

        // Disable the join button after the player successfully joins
        document.getElementById("joinBtn").disabled = true;

        // Draw the starting deck for the player that joined
        const drawCardsResult = await fetch(`/draw-white/${roomId}/${playerName}/10`, {
            method: "POST"
        });

        // Check for errors
        if (!drawCardsResult.ok) {
            const errorData = await drawCardsResult.json();
            resultDiv.textContent = errorData.errorMessage ?? "Unknown error";
            return;
        }

        // TODO: redirect to lobby page
        // window.location.href = `/lobby.html?roomId=${roomId}&player=${username}`;
    } catch (err) {
        resultDiv.textContent = "An error occurred while joining the game.";
        console.error(err);
    }
});
