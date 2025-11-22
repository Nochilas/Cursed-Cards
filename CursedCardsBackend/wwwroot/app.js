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
        resultDiv.textContent = `Game link: http://localhost:5291/join.html?roomId=${data.response}`;
    } catch (err) {
        resultDiv.textContent = "An error occurred while creating the game.";
        console.error(err);
    }
});
