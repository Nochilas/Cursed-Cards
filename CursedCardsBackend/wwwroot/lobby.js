// Polling every 2 seconds
setInterval(refreshLobby, 2000);

// First loading is immediate
refreshLobby();

/** Refresh the lobby */
async function refreshLobby() {
    const res = await fetch(`/lobby-state/${roomId}`);
    const data = await res.json();

    if (!res.ok) {
        const errorData = await res.json();
        resultDiv.textContent = errorData.errorMessage ?? "Unknown error polling lobby";
        return;
    }

    const apiResponse = data.response;

    updatePlayersList(apiResponse.players);

    if (apiResponse.czar) {
        updateCzarUI(apiResponse.czar);
    }

    if (apiResponse.gameStarted) {
        redirectToGame();
    }
}

/** Update the displayed list of the players. */
function updatePlayersList(players) {
    const playersList = document.getElementById("playersList");
    playersList.innerHTML = "";

    players.forEach(player => {
        const listItem = document.createElement("li");
        listItem.textContent = player;
        playersList.appendChild(listItem);
    });
}

/** Updates the UI for the player chosen as czar. */
function updateCzarUI(czar) {
    document.getElementById("czarLabel").textContent = `Czar: ${czar}`;

    const startBtn = document.getElementById("startBtn");
    if (czar === playerName) {
        startBtn.style.display = "block";
        startBtn.style.color = "green";
    } else {
        startBtn.style.display = "none";
    }
}

/** Starts the game, changing the state. */
async function startGame() {
    const res = await fetch(`/start-game/${roomId}/${playerName}`, { method: "POST" });
    const data = await res.json();

    if (!res.ok) {
        alert(data.errorMessage);
        return;
    }
}

/** Once the game starts, redirect to the game page. */
function redirectToGame() {
    window.location.href = `/game.html?roomId=${roomId}&playerName=${playerName}`;
}
