document.getElementById("createGameBtn").addEventListener("click", async () => {
    const resultDiv = document.getElementById("result");
    resultDiv.textContent = "Creating game...";

    try {
        const res = await fetch("/create-game", {
            method: "POST"
        });

        // Check for errors
        if (!res.ok) {
            const errorData = await res.json();
            resultDiv.textContent = errorData.errorMessage ?? "Unknown error";
            return;
        }

        const data = await res.json();

        const inviteLink = `${window.location.origin}/join.html?roomId=${data.response}`;
        resultDiv.innerHTML = `
            <div class="invite-link-box">
                <input type="text" id="inviteLinkInput" value="${inviteLink}" readonly />
                <button id="copyInviteBtn" title="Copy link">📋</button>
            </div>
        `;

        document.getElementById("copyInviteBtn").onclick = async () => {
            try {
                await navigator.clipboard.writeText(inviteLink);
                document.getElementById("copyInviteBtn").textContent = "✅";
                setTimeout(() => {
                    document.getElementById("copyInviteBtn").textContent = "📋";
                }, 1200);
            } catch {
                alert("Copy failed");
            }
        };

    } catch (err) {
        resultDiv.textContent = "An error occurred while creating the game.";
        console.error(err);
    }
});