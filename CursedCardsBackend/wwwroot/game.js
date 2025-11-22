let currentBlackCard = "";
let blanksRequired = 1;
let selectedCards = [];
let hand = [];

// UI refs
const blackCardDiv = document.getElementById("blackCard");
const handContainer = document.getElementById("handContainer");
const selectedList = document.getElementById("selectedList");
const submitBtn = document.getElementById("submitCardsBtn");
const drawBlackBtn = document.getElementById("drawBlackBtn");
const startRoundBtn = document.getElementById("startRoundBtn");
const czarInfo = document.getElementById("czarInfo");

// At first, hide both buttons
drawBlackBtn.style.display = "none";
startRoundBtn.style.display = "none";

/** Loads the current game state. */
async function loadState() {
    const res = await fetch(`/game-state/${roomId}`, {
        method: "GET"
    });
    const data = await res.json();

    if (!res.ok) {
        console.error(data.errorMessage);
        return;
    }

    const apiResponse = data.response;

    // Czar info
    czarInfo.textContent = `Czar: ${apiResponse.czar}`;

    // Only the czar can see the "draw black card" and "start round" buttons
    if (apiResponse.czar === playerName) {
        drawBlackBtn.style.display = "block";
        startRoundBtn.style.display = "block";

        // If a black card is drawn, disable the draw black button and enable the start round
        if (apiResponse.currentBlackCard) {
            disableButton(drawBlackBtn);
            enableButton(startRoundBtn);
        }
        // If a black card is not drawn, enable the draw black and disable the start round
        else {
            disableButton(startRoundBtn);
            enableButton(drawBlackBtn);
        }

        // If the round has started, disable both buttons
        if (apiResponse.roundStatus == 1) {
            disableButton(drawBlackBtn);
            disableButton(startRoundBtn);
        }
    } else {
        // Only the players can see their hand
        hand = apiResponse.hands[playerName] ?? [];
        renderHand();
    }


    // If present, count the blanks (underscores)
    if (apiResponse.currentBlackCard) {
        currentBlackCard = apiResponse.currentBlackCard;
        blackCardDiv.textContent = currentBlackCard;
        countRequiredBlanks();
    }

    // The czar will see the winner selection menu
    renderWinnerSelection(apiResponse);
}

/** Renders the player hand. */
function renderHand() {
    handContainer.innerHTML = "";

    hand.forEach(card => {
        const div = document.createElement("div");
        div.className = "card";
        div.textContent = card;

        if (selectedCards.includes(card)) {
            div.classList.add("selected");
        }

        div.onclick = () => selectCard(card, div);

        handContainer.appendChild(div);
    });
}

/** Selects a card from a player hand. */
function selectCard(card, element) {
    if (selectedCards.includes(card)) {
        selectedCards = selectedCards.filter(c => c !== card);
        element.classList.remove("selected");
    } else {
        if (selectedCards.length >= blanksRequired) {
            return;
        }
        selectedCards.push(card);
        element.classList.add("selected");
    }

    renderSelectedList();
}

/** Renders the selected cards. */
function renderSelectedList() {
    selectedList.innerHTML = "";
    selectedCards.forEach(card => {
        const listItem = document.createElement("li");
        listItem.textContent = card;
        selectedList.appendChild(listItem);
    });

    // Show button only if enough cards selected
    submitBtn.style.display = selectedCards.length === blanksRequired ? "block" : "none";
}

/** Disables a button. */
function disableButton(btn) {
    btn.disabled = true;
    btn.classList.add("disabled-btn");
}

/** Enables a button. */
function enableButton(btn) {
    btn.disabled = false;
    btn.classList.remove("disabled-btn");
}

/** Counts the blanks (underscores) in a black card. */
function countRequiredBlanks() {
    blanksRequired = (currentBlackCard.match(/_/g) || []).length || 1;
}

/** Render winner selection for the czar. */
function renderWinnerSelection(apiResponse) {
    const container = document.getElementById("czarSelectionContainer");
    const title = document.getElementById("czarSelectionTitle");

    // Reset
    container.innerHTML = "";
    title.style.display = "none";

    // Show only when the round is over
    if (apiResponse.roundStatus !== 2) {
        return;
    }

    // Only the czar can see the answers
    if (apiResponse.czar !== playerName) {
        container.textContent = "Czar picking winner...";
        return;
    }

    // Show all answers for czar
    title.style.display = "block";

    const playedCards = apiResponse.playedCards ?? {};

    Object.entries(playedCards).forEach(([player, cards]) => {
        const btn = document.createElement("button");
        btn.className = "czar-choice-btn";
        btn.textContent = `${cards.join(" | ")}`;

        btn.onclick = () => selectWinner(player);

        container.appendChild(btn);
    });
}

/** Czar selects the winner. */
async function selectWinner(winnerPlayer) {
    const res = await fetch(`/select-winner?roomId=${roomId}&czarPlayerName=${playerName}&winnerPlayer=${winnerPlayer}`, {
        method: "POST"
    });

    const data = await res.json();

    if (!res.ok) {
        alert(data.errorMessage);
        return;
    }

    // TODO improve render
    alert(`Winner selected: ${winnerPlayer}`);
}

/** CZAR ACTION: draw black card */
drawBlackBtn.onclick = async () => {
    const res = await fetch(`/draw-black/${roomId}`, { method: "POST" });
    const data = await res.json();

    if (res.ok) {
        currentBlackCard = data.response;
        blackCardDiv.textContent = currentBlackCard;

        countRequiredBlanks();

        // Disable the draw button to avoid another draw
        disableButton(drawBlackBtn);

        // Now the round can start
        enableButton(startRoundBtn);
    }
};

/** CZAR ACTION: start round */
startRoundBtn.onclick = async () => {
    const res = await fetch(`/start-round/${roomId}/${playerName}`, { method: "POST" });
    const data = await res.json();

    if (!res.ok) {
        alert(data.errorMessage);
        return;
    }
};


/** Player submits chosen cards */
submitBtn.onclick = async () => {
    const body = {
        selectedCards: selectedCards
    };

    const res = await fetch(`/play-cards/${roomId}/${playerName}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });

    const data = await res.json();

    if (res.ok) {
        console.log("Cards sent successfully");
        disableButton(submitBtn);
    } else {
        alert(data.errorMessage);
    }
};

// POLLING
setInterval(loadState, 2000);
loadState();
