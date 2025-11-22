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
        const res = await fetch(`/join-game/${roomId}/${playerName}`, {
            method: "POST"
        });

        if (!res.ok) {
            throw new Error("API Error");
        }

        const data = await res.json();
        resultDiv.textContent = `You joined the game as ${data.response}!`;

        // TODO: reindirizzare alla schermata di lobby
        // window.location.href = `/lobby.html?roomId=${roomId}&player=${username}`;

    } catch (err) {
        resultDiv.textContent = "An error occurred while joining the game.";
        console.error(err);
    }
});
