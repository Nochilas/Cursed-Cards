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

    // Czar can draw black card and start the round
    if (apiResponse.czar === playerName) {
        drawBlackBtn.style.display = "block";
        startRoundBtn.style.display = "block";
    }

    // Hand
    hand = apiResponse.hands[playerName] ?? [];
    renderHand();

    // If present, count the blanks (underscores)
    if (apiResponse.currentBlackCard) {
        currentBlackCard = apiResponse.currentBlackCard;
        blackCardDiv.textContent = currentBlackCard;
        blanksRequired = (currentBlackCard.match(/_/g) || []).length || 1;

        // Now the round can start
        startRoundBtn.disabled = false;
    } else {
        startRoundBtn.disabled = true;
    }
}

/** Renders the player hand. */
function renderHand() {
    handContainer.innerHTML = "";

    hand.forEach(card => {
        const div = document.createElement("div");
        div.className = "card";
        div.textContent = card;

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

// CZAR ACTION: draw black card
drawBlackBtn.onclick = async () => {
    const res = await fetch(`/draw-black/${roomId}`, { method: "POST" });
    const data = await res.json();

    if (res.ok) {
        currentBlackCard = data.response;
        blackCardDiv.textContent = currentBlackCard;

        // TODO check the other line
        blanksRequired = (currentBlackCard.match(/_/g) || []).length || 1;

        // Now the round can start
        startRoundBtn.disabled = false;
    }
};

// CZAR ACTION: start round
startRoundBtn.onclick = async () => {
    const res = await fetch(`/start-round/${roomId}/${playerName}`, { method: "POST" });
    const data = await res.json();

    if (!res.ok) {
        alert(data.errorMessage);
        return;
    }

    alert("Round iniziato!");
};


// Player submits chosen cards
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
        alert("Cards sent successfully");
        submitBtn.style.display = "none";
    } else {
        alert(data.errorMessage);
    }
};

// POLLING
setInterval(loadState, 2000);
loadState();
