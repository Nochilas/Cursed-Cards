// State variables (card reveal)
let revealIndex = 0;
let revealOrder = [];
let state = {};
let czarRevealing = false;
let czarPicking = false;
let shuffledPlayedCards = null;

let currentBlackCard = "";
let blanksRequired = 1;
let selectedCards = [];
let hand = [];

// UI refs
const blackCardDiv = document.getElementById("blackCard");
const handContainer = document.getElementById("handContainer");
const playerHandContainer = document.getElementById("playerCards");
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
    state = apiResponse;

    // Scoreboard
    renderScoreboard(apiResponse.scores);

    // Czar info
    czarInfo.textContent = `Czar: ${apiResponse.czar}`;

    if (apiResponse.roundStatus === 0
        || apiResponse.roundStatus === 3) {
        // If the game hasn't started and there's no card, reset the card UI
        if (!apiResponse.currentBlackCard) {
            currentBlackCard = "";
            blackCardDiv.textContent = "No black card";
        }

        // Reset player selections
        selectedCards = [];
        selectedList.innerHTML = "";

        // Hide submit button
        submitBtn.style.display = "none";

        // If this player is the new czar, hide his hand
        if (apiResponse.czar === playerName) {
            hand = [];
            handContainer.innerHTML = "";
            playerHandContainer.style.display = 'none';
        } else {
            playerHandContainer.style.display = 'block';
        }
    }

    // Only the czar can see the "draw black card" and "start round" buttons
    if (apiResponse.czar === playerName) {
        // Czar is picking: round is finished
        if (apiResponse.roundStatus === 2) {
            // If czar is not revealing nor picking, setup UI for czar reveal
            if (!czarRevealing && !czarPicking) {
                setupCzarRevealUI(apiResponse);
            }
        }
        // Czar is not picking: round is not finished
        else {
            drawBlackBtn.style.display = "block";
            startRoundBtn.style.display = "block";

            // If a black card is drawn, disable the draw black button and enable the start round
            if (apiResponse.currentBlackCard) {
                disableButton(drawBlackBtn);

                // Enable start round button only if round hasn't started yet
                if (apiResponse.RoundStatus === 0) {
                    enableButton(startRoundBtn);
                }
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
        }
    } else {
        // Hide buttons from the former czar (now a player)
        drawBlackBtn.style.display = "none";
        startRoundBtn.style.display = "none";

        // Only the players can see their hand
        hand = apiResponse.hands[playerName] ?? [];
        renderHand();
    }

    // Check for the black card on every render
    if (apiResponse.currentBlackCard) {
        currentBlackCard = apiResponse.currentBlackCard;
        blackCardDiv.textContent = currentBlackCard;
        countRequiredBlanks();
    }

    // The czar will see the winner selection menu
    renderWinnerSelection(apiResponse);
}

/** Sets up the UI for czar card reveal. */
function setupCzarRevealUI(apiResponse) {
    // Hide played cards
    hideCzarSelectionContainer();

    // Show "step by step" reveal UI
    document.getElementById("czarRevealArea").style.display = "block";

    // Shuffle the player order
    revealOrder = Object.keys(apiResponse.playedCards);
    shuffle(revealOrder);

    revealIndex = 0;

    // Reset UI
    document.getElementById("showCardsBtn").style.display = "block";
    document.getElementById("czarSingleReveal").innerHTML = "";
    document.getElementById("nextCardBtn").style.display = "none";
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
    if (selectedCards.length === blanksRequired) {
        submitBtn.style.display = "block";
        enableButton(submitBtn);
    } else {
        submitBtn.style.display = "none";
    }
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
    if (apiResponse.roundStatus !== 2 || czarRevealing) {
        return;
    }

    // Only the czar can see the answers
    if (apiResponse.czar !== playerName) {
        container.textContent = "Czar picking winner...";
        return;
    }

    // Show all answers for czar
    title.style.display = "block";

    // Shuffle the played cards
    if (!shuffledPlayedCards) {
        shuffledPlayedCards = Object.entries(apiResponse.playedCards ?? []);
        shuffle(shuffledPlayedCards);
    }

    shuffledPlayedCards.forEach(([player, cards]) => {
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

    czarPicking = false;
    hideCzarSelectionContainer();

    // TODO improve render
    alert(`Winner selected: ${winnerPlayer}`);
}

/** Shuffles the played cards. */
function shuffle(arr) {
    for (let i = arr.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [arr[i], arr[j]] = [arr[j], arr[i]];
    }
}

/** Revelas the next card. */
function showNextReveal() {
    const revealBox = document.getElementById("czarSingleReveal");

    if (revealIndex >= revealOrder.length) {
        czarRevealing = false;
        czarPicking = true;

        // All cards have been revealed
        revealBox.innerHTML = "<i>All cards revealed</i>";

        // Show all answers and select winner
        document.getElementById("czarSelectionContainer").style.display = "block";

        // Hide step-by-step UI
        document.getElementById("czarRevealArea").style.display = "none";

        return;
    }

    const player = revealOrder[revealIndex];
    const cards = state.playedCards[player];

    revealBox.innerHTML = `
        <div class="reveal-cards">
            ${cards.map(c => `<div class="card">${c}</div>`).join("")}
        </div>
    `;

    revealIndex++;
}

/** Hides the czar selection container. */
function hideCzarSelectionContainer() {
    document.getElementById("czarSelectionContainer").style.display = "none";
}

/** Renders the scoreboard. */
function renderScoreboard(scores) {
    const scoreList = document.getElementById("scoreList");
    scoreList.innerHTML = "";

    Object.entries(scores ?? {}).forEach(([player, score]) => {
        const li = document.createElement("li");
        li.textContent = `${player}: ${score}`;
        scoreList.appendChild(li);
    });
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

    const playCardsResponse = await fetch(`/play-cards/${roomId}/${playerName}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });

    const data = await playCardsResponse.json();

    if (playCardsResponse.ok) {
        disableButton(submitBtn);
    } else {
        alert(data.errorMessage);
    }

    // After the player played the cards, they are gone
    // So the player draws new cards (same number of played cards)
    const drawCardsResult = await fetch(`/draw-white/${roomId}/${playerName}/${blanksRequired}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });

    const drawCardsData = await drawCardsResult.json();
    if (!drawCardsResult.ok) {
        alert(drawCardsData.errorMessage)
    }
};

/** Show cards function on "Show Cards". */
document.getElementById("showCardsBtn").onclick = () => {
    czarRevealing = true;
    showNextReveal();
    document.getElementById("showCardsBtn").style.display = "none";
    document.getElementById("nextCardBtn").style.display = "block";
};

/** Show cards function on "Next". */
document.getElementById("nextCardBtn").onclick = () => {
    showNextReveal();
};

// POLLING
setInterval(loadState, 2000);
loadState();
